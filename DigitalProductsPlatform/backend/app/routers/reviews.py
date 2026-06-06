from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
from app.database import get_db
from app.models import User, Product, Review, Purchase
from app.schemas import ReviewCreate, ReviewResponse
from app.auth import get_current_user

router = APIRouter(prefix="/reviews", tags=["reviews"])

@router.post("", response_model=ReviewResponse)
def create_review(data: ReviewCreate, user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    product = db.query(Product).filter(Product.id == data.product_id).first()
    if not product:
        raise HTTPException(status_code=404, detail="Product not found")

    purchase = db.query(Purchase).filter(
        Purchase.buyer_id == user.id,
        Purchase.product_id == data.product_id,
        Purchase.status == "completed"
    ).first()
    if not purchase and not user.is_admin:
        raise HTTPException(status_code=403, detail="You must purchase this product to review it")

    existing = db.query(Review).filter(Review.product_id == data.product_id, Review.user_id == user.id).first()
    if existing:
        raise HTTPException(status_code=400, detail="You already reviewed this product")

    if data.rating < 1 or data.rating > 5:
        raise HTTPException(status_code=400, detail="Rating must be between 1 and 5")

    review = Review(product_id=data.product_id, user_id=user.id, rating=data.rating, text=data.text)
    db.add(review)
    db.commit()
    db.refresh(review)
    r = ReviewResponse.model_validate(review)
    r.username = user.username
    return r

@router.get("/product/{product_id}", response_model=list[ReviewResponse])
def get_product_reviews(product_id: str, db: Session = Depends(get_db)):
    reviews = db.query(Review).filter(Review.product_id == product_id).order_by(Review.created_at.desc()).all()
    result = []
    for rv in reviews:
        r = ReviewResponse.model_validate(rv)
        if rv.user:
            r.username = rv.user.username
        result.append(r)
    return result
