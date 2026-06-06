from datetime import datetime, timezone
from fastapi import APIRouter, Depends, HTTPException, Request
from fastapi.responses import HTMLResponse, RedirectResponse
from fastapi.templating import Jinja2Templates
from sqlalchemy.orm import Session
from app.database import get_db
from app.models import User, Product, Purchase, License, Coupon
from app.schemas import CheckoutRequest, CartCheckoutRequest, CheckoutResponse, PurchaseResponse
from app.auth import get_current_user
from app.services.payments import create_payment_order, create_payment_link
from app.services.licenses import generate_license_key
from app.config import settings
import json

router = APIRouter(prefix="/checkout", tags=["checkout"])

@router.post("/create", response_model=CheckoutResponse)
async def checkout(
    data: CheckoutRequest,
    request: Request,
    user: User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    product = db.query(Product).filter(Product.id == data.product_id).first()
    if not product or not product.is_active:
        raise HTTPException(status_code=404, detail="Product not found")

    existing_purchase = db.query(Purchase).filter(
        Purchase.buyer_id == user.id,
        Purchase.product_id == product.id,
        Purchase.status == "completed"
    ).first()
    if existing_purchase:
        raise HTTPException(status_code=400, detail="You already own this product")

    amount = product.sale_price if product.sale_price > 0 else product.price
    if data.coupon_code:
        coupon = db.query(Coupon).filter(Coupon.code == data.coupon_code.upper(), Coupon.is_active == True).first()
        if not coupon:
            raise HTTPException(status_code=400, detail="Invalid coupon code")
        if coupon.expires_at and coupon.expires_at < datetime.now(timezone.utc):
            raise HTTPException(status_code=400, detail="Coupon has expired")
        if coupon.max_uses > 0 and coupon.current_uses >= coupon.max_uses:
            raise HTTPException(status_code=400, detail="Coupon usage limit reached")
        if amount < coupon.min_purchase:
            raise HTTPException(status_code=400, detail=f"Minimum purchase amount is €{coupon.min_purchase:.2f}")
        if coupon.discount_type == "percentage":
            amount = round(amount * (1 - coupon.discount_value / 100), 2)
        else:
            amount = max(0, amount - coupon.discount_value)
        coupon.current_uses = (coupon.current_uses or 0) + 1
    reference = f"ORDER-{user.id[:8]}-{product.id[:8]}"

    success_url = data.success_url or f"{settings.APP_URL}/checkout/success"
    failure_url = data.failure_url or f"{settings.APP_URL}/checkout/failure"
    callback_url = f"{settings.APP_URL}/webhooks/paysera"

    order = await create_payment_order(
        reference=reference,
        amount=amount,
        currency="EUR",
        success_url=success_url,
        failure_url=failure_url,
        callback_url=callback_url,
    )
    if not order:
        raise HTTPException(status_code=502, detail="Payment service unavailable")

    order_id = order.get("id") or order.get("order_id")
    affiliate_ref = request.cookies.get("affiliate_ref", "")
    purchase = Purchase(
        buyer_id=user.id,
        product_id=product.id,
        amount=amount,
        currency="EUR",
        status="pending",
        paysera_order_id=order_id,
        affiliate_code=affiliate_ref,
    )
    db.add(purchase)
    db.commit()

    payment_link = await create_payment_link(
        order_id=order_id,
        amount=amount,
        name=product.title,
        language="en",
        email=user.email,
    )
    if not payment_link:
        raise HTTPException(status_code=502, detail="Failed to create payment link")

    payment_url = payment_link.get("payment_URL") or payment_link.get("link", {}).get("url", "")
    return CheckoutResponse(payment_url=payment_url, order_id=order_id)

@router.post("/cart", response_model=CheckoutResponse)
async def cart_checkout(
    data: CartCheckoutRequest,
    user: User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    if not data.product_ids:
        raise HTTPException(status_code=400, detail="No products selected")

    products = db.query(Product).filter(Product.id.in_(data.product_ids), Product.is_active == True).all()
    if len(products) != len(data.product_ids):
        raise HTTPException(status_code=404, detail="One or more products not found")

    existing = db.query(Purchase).filter(
        Purchase.buyer_id == user.id,
        Purchase.product_id.in_(data.product_ids),
        Purchase.status == "completed"
    ).all()
    existing_ids = {p.product_id for p in existing}
    products = [p for p in products if p.id not in existing_ids]
    if not products:
        raise HTTPException(status_code=400, detail="You already own all selected products")

    total = sum(p.sale_price if p.sale_price > 0 else p.price for p in products)
    if data.coupon_code:
        coupon = db.query(Coupon).filter(Coupon.code == data.coupon_code.upper(), Coupon.is_active == True).first()
        if not coupon:
            raise HTTPException(status_code=400, detail="Invalid coupon code")
        if coupon.expires_at and coupon.expires_at < datetime.now(timezone.utc):
            raise HTTPException(status_code=400, detail="Coupon has expired")
        if coupon.max_uses > 0 and coupon.current_uses >= coupon.max_uses:
            raise HTTPException(status_code=400, detail="Coupon usage limit reached")
        if total < coupon.min_purchase:
            raise HTTPException(status_code=400, detail=f"Minimum purchase amount is €{coupon.min_purchase:.2f}")
        if coupon.discount_type == "percentage":
            total = round(total * (1 - coupon.discount_value / 100), 2)
        else:
            total = max(0, total - coupon.discount_value)
        coupon.current_uses = (coupon.current_uses or 0) + 1

    reference = f"CART-{user.id[:8]}-{len(products)}items"
    success_url = data.success_url or f"{settings.APP_URL}/checkout/success"
    failure_url = data.failure_url or f"{settings.APP_URL}/checkout/failure"
    callback_url = f"{settings.APP_URL}/webhooks/paysera"

    order = await create_payment_order(
        reference=reference,
        amount=total,
        currency="EUR",
        success_url=success_url,
        failure_url=failure_url,
        callback_url=callback_url,
    )
    if not order:
        raise HTTPException(status_code=502, detail="Payment service unavailable")

    order_id = order.get("id") or order.get("order_id")
    for product in products:
        purchase = Purchase(
            buyer_id=user.id,
            product_id=product.id,
            amount=product.sale_price if product.sale_price > 0 else product.price,
            currency="EUR",
            status="pending",
            paysera_order_id=order_id,
        )
        db.add(purchase)
    db.commit()

    payment_link = await create_payment_link(
        order_id=order_id,
        amount=total,
        name=f"Cart ({len(products)} items)",
        language="en",
        email=user.email,
    )
    if not payment_link:
        raise HTTPException(status_code=502, detail="Failed to create payment link")

    payment_url = payment_link.get("payment_URL") or payment_link.get("link", {}).get("url", "")
    return CheckoutResponse(payment_url=payment_url, order_id=order_id)

@router.get("/success")
def checkout_success(request: Request):
    return {"message": "Payment successful! You can now download your product."}

@router.get("/failure")
def checkout_failure(request: Request):
    return {"message": "Payment was cancelled or failed. Please try again."}

@router.get("/purchases", response_model=list[PurchaseResponse])
def my_purchases(user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    purchases = db.query(Purchase).filter(Purchase.buyer_id == user.id).order_by(Purchase.created_at.desc()).all()
    result = []
    for p in purchases:
        r = PurchaseResponse.model_validate(p)
        if p.product:
            r.product_title = p.product.title
            r.product_slug = p.product.slug
        result.append(r)
    return result
