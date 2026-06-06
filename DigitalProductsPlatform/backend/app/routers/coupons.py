from datetime import datetime, timezone
from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
from app.database import get_db
from app.models import User, Coupon
from app.auth import get_current_user
from app.schemas import CouponCreate, CouponUpdate, CouponResponse, CouponValidate

router = APIRouter(prefix="/coupons", tags=["coupons"])

@router.post("", response_model=CouponResponse)
def create_coupon(data: CouponCreate, user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    if not user.is_seller and not user.is_admin:
        raise HTTPException(status_code=403, detail="Only sellers can create coupons")
    existing = db.query(Coupon).filter(Coupon.code == data.code.upper(), Coupon.seller_id == user.id).first()
    if existing:
        raise HTTPException(status_code=400, detail="Coupon code already exists")
    coupon = Coupon(
        seller_id=user.id,
        code=data.code.upper(),
        discount_type=data.discount_type,
        discount_value=data.discount_value,
        min_purchase=data.min_purchase,
        max_uses=data.max_uses,
        expires_at=data.expires_at,
    )
    db.add(coupon)
    db.commit()
    db.refresh(coupon)
    return CouponResponse.model_validate(coupon)

@router.get("", response_model=list[CouponResponse])
def list_coupons(user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    if not user.is_seller and not user.is_admin:
        return []
    coupons = db.query(Coupon).filter(Coupon.seller_id == user.id).order_by(Coupon.created_at.desc()).all()
    return [CouponResponse.model_validate(c) for c in coupons]

@router.get("/{coupon_id}", response_model=CouponResponse)
def get_coupon(coupon_id: str, user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    coupon = db.query(Coupon).filter(Coupon.id == coupon_id, Coupon.seller_id == user.id).first()
    if not coupon:
        raise HTTPException(status_code=404, detail="Coupon not found")
    return CouponResponse.model_validate(coupon)

@router.put("/{coupon_id}", response_model=CouponResponse)
def update_coupon(coupon_id: str, data: CouponUpdate, user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    coupon = db.query(Coupon).filter(Coupon.id == coupon_id, Coupon.seller_id == user.id).first()
    if not coupon:
        raise HTTPException(status_code=404, detail="Coupon not found")
    update_data = data.model_dump(exclude_unset=True)
    for key, value in update_data.items():
        setattr(coupon, key, value)
    db.commit()
    db.refresh(coupon)
    return CouponResponse.model_validate(coupon)

@router.delete("/{coupon_id}")
def delete_coupon(coupon_id: str, user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    coupon = db.query(Coupon).filter(Coupon.id == coupon_id, Coupon.seller_id == user.id).first()
    if not coupon:
        raise HTTPException(status_code=404, detail="Coupon not found")
    db.delete(coupon)
    db.commit()
    return {"message": "Coupon deleted"}

@router.post("/validate", response_model=CouponResponse)
def validate_coupon(data: CouponValidate, db: Session = Depends(get_db)):
    coupon = db.query(Coupon).filter(Coupon.code == data.code.upper(), Coupon.is_active == True).first()
    if not coupon:
        raise HTTPException(status_code=404, detail="Invalid coupon code")
    if coupon.expires_at and coupon.expires_at < datetime.now(timezone.utc):
        raise HTTPException(status_code=400, detail="Coupon has expired")
    if coupon.max_uses > 0 and coupon.current_uses >= coupon.max_uses:
        raise HTTPException(status_code=400, detail="Coupon usage limit reached")
    if data.amount < coupon.min_purchase:
        raise HTTPException(status_code=400, detail=f"Minimum purchase amount is €{coupon.min_purchase:.2f}")
    r = CouponResponse.model_validate(coupon)
    if coupon.discount_type == "percentage":
        r.discounted_amount = round(data.amount * (1 - coupon.discount_value / 100), 2)
    else:
        r.discounted_amount = max(0, data.amount - coupon.discount_value)
    return r
