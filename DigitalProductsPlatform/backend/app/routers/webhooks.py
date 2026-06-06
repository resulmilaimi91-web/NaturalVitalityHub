import json
import logging
from fastapi import APIRouter, Depends, HTTPException, Request
from sqlalchemy.orm import Session
from app.database import get_db
from app.models import User, Product, Purchase, License, AffiliateLink
from app.services.licenses import generate_license_key
from app.services.payments import verify_webhook_signature
from app.services.email import send_purchase_confirmation, send_license_key, send_sale_notification
from app.config import settings
from datetime import datetime, timezone

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/webhooks", tags=["webhooks"])

@router.post("/paysera")
async def paysera_webhook(request: Request, db: Session = Depends(get_db)):
    body = await request.body()
    signature = request.headers.get("X-Paysera-Signature", "")
    
    # Log webhook attempt
    client_ip = request.client.host if request.client else "unknown"
    logger.info(f"Webhook received from {client_ip}")

    if not verify_webhook_signature(body, signature):
        logger.warning(f"Invalid webhook signature from {client_ip}")
        raise HTTPException(status_code=400, detail="Invalid signature")

    try:
        data = json.loads(body)
    except json.JSONDecodeError:
        logger.warning(f"Invalid JSON in webhook from {client_ip}")
        raise HTTPException(status_code=400, detail="Invalid JSON")

    order_id = data.get("order_id") or data.get("id")
    status = data.get("status") or data.get("order", {}).get("status", "")
    
    logger.info(f"Webhook data: order_id={order_id}, status={status}")

    purchase = db.query(Purchase).filter(Purchase.paysera_order_id == order_id).first()
    if not purchase:
        logger.warning(f"Webhook for unknown order: {order_id}")
        return {"received": True, "note": "Order not found"}

    if status == "paid":
        purchase.status = "completed"
        purchase.paysera_status = "paid"

        product = db.query(Product).filter(Product.id == purchase.product_id).first()
        if product:
            product.sales_count = (product.sales_count or 0) + 1

        if purchase.license_key:
            return {"received": True}

        license_key = generate_license_key(purchase.product_id, purchase.id)
        purchase.license_key = license_key

        lic = License(
            purchase_id=purchase.id,
            product_id=purchase.product_id,
            license_key=license_key,
            is_active=True,
        )
        db.add(lic)

        if purchase.affiliate_code:
            aff_link = db.query(AffiliateLink).filter(AffiliateLink.code == purchase.affiliate_code, AffiliateLink.is_active == True).first()
            if aff_link:
                commission = round(purchase.amount * aff_link.commission_percentage / 100, 2)
                purchase.affiliate_commission = commission
                aff_link.sales_count = (aff_link.sales_count or 0) + 1
                aff_link.revenue_generated = (aff_link.revenue_generated or 0) + commission
                affiliate_user = db.query(User).filter(User.id == aff_link.user_id).first()
                if affiliate_user:
                    affiliate_user.balance = (affiliate_user.balance or 0) + commission

        seller = db.query(User).filter(User.id == product.seller_id).first() if product else None
        if seller:
            seller.balance = (seller.balance or 0) + purchase.amount

        buyer = db.query(User).filter(User.id == purchase.buyer_id).first()
        if buyer:
            send_purchase_confirmation(
                to_email=buyer.email,
                username=buyer.username,
                product_title=product.title if product else "Product",
                amount=purchase.amount,
                license_key=license_key,
                download_url=f"{settings.APP_URL}/my-purchases",
            )
            if license_key:
                send_license_key(
                    to_email=buyer.email,
                    username=buyer.username,
                    product_title=product.title if product else "Product",
                    license_key=license_key,
                )
        if seller:
            send_sale_notification(
                to_email=seller.email,
                seller_name=seller.username,
                product_title=product.title if product else "Product",
                amount=purchase.amount,
                buyer_email=buyer.email if buyer else "",
            )

    elif status in ("canceled", "closed"):
        purchase.status = "cancelled"
        purchase.paysera_status = status

    purchase.updated_at = datetime.now(timezone.utc)
    db.commit()

    return {"received": True}

@router.post("/paysera/test")
async def paysera_test_webhook(request: Request):
    body = await request.json()
    print(f"Test webhook received: {body}")
    return {"received": True}
