from fastapi import APIRouter, Depends, HTTPException, Request
from sqlalchemy.orm import Session
from app.database import get_db
from app.models import User, Product, Purchase, License
from app.schemas import LicenseResponse, LicenseValidate
from app.auth import get_current_user
from app.services.licenses import generate_license_key
from datetime import datetime, timezone

router = APIRouter(prefix="/licenses", tags=["licenses"])

@router.get("/my", response_model=list[LicenseResponse])
def my_licenses(user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    licenses = db.query(License).join(Purchase).filter(Purchase.buyer_id == user.id).all()
    return [LicenseResponse.model_validate(l) for l in licenses]

@router.post("/validate")
def validate_license(data: LicenseValidate, db: Session = Depends(get_db)):
    lic = db.query(License).filter(License.license_key == data.license_key).first()
    if not lic:
        return {"valid": False, "message": "License key not found"}
    if not lic.is_active:
        return {"valid": False, "message": "License key is deactivated"}
    if lic.expires_at and lic.expires_at < datetime.now(timezone.utc):
        return {"valid": False, "message": "License key has expired"}

    if data.domain and lic.domain and lic.domain != data.domain:
        return {"valid": False, "message": "Domain mismatch"}

    product = db.query(Product).filter(Product.id == lic.product_id).first()
    return {
        "valid": True,
        "license_key": lic.license_key,
        "product": product.title if product else "",
        "product_version": product.current_version if product else "",
        "expires_at": lic.expires_at.isoformat() if lic.expires_at else None,
    }

@router.post("/activate")
def activate_license(data: LicenseValidate, db: Session = Depends(get_db)):
    lic = db.query(License).filter(License.license_key == data.license_key).first()
    if not lic:
        return {"success": False, "message": "License key not found"}
    if not lic.is_active:
        return {"success": False, "message": "License key is deactivated"}

    if data.domain:
        lic.domain = data.domain
    if not lic.activated_at:
        lic.activated_at = datetime.now(timezone.utc)
    db.commit()
    return {"success": True, "message": "License activated"}
