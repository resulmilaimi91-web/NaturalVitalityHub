from fastapi import APIRouter, Depends
from fastapi.responses import JSONResponse
from sqlalchemy.orm import Session
from sqlalchemy import func
from app.database import get_db
from app.models import User, Product, Purchase
from app.auth import get_current_user
from app.schemas import DashboardStats, ProductResponse

router = APIRouter(prefix="/dashboard", tags=["dashboard"])

@router.get("/seller", response_model=DashboardStats)
def seller_dashboard(user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    if not user.is_seller and not user.is_admin:
        return DashboardStats()

    products = db.query(Product).filter(Product.seller_id == user.id).all()
    product_ids = [p.id for p in products]

    total_products = len(products)
    total_sales = sum(p.sales_count or 0 for p in products)
    total_downloads = sum(p.downloads_count or 0 for p in products)

    recent = db.query(Purchase).filter(
        Purchase.product_id.in_(product_ids),
        Purchase.status == "completed"
    ).order_by(Purchase.created_at.desc()).limit(10).all()

    total_revenue = db.query(func.sum(Purchase.amount)).filter(
        Purchase.product_id.in_(product_ids),
        Purchase.status == "completed"
    ).scalar() or 0.0

    recent_sales = []
    for r in recent:
        product = db.query(Product).filter(Product.id == r.product_id).first()
        recent_sales.append({
            "id": r.id,
            "product_title": product.title if product else "",
            "amount": r.amount,
            "created_at": r.created_at.isoformat(),
            "status": r.status,
        })

    return DashboardStats(
        total_products=total_products,
        total_sales=total_sales,
        total_revenue=total_revenue,
        total_downloads=total_downloads,
        recent_sales=recent_sales,
    )

@router.get("/buyer", response_model=DashboardStats)
def buyer_dashboard(user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    purchases = db.query(Purchase).filter(Purchase.buyer_id == user.id).order_by(Purchase.created_at.desc()).all()
    total_purchases = len(purchases)
    total_spent = sum(p.amount for p in purchases if p.status == "completed")
    active_licenses = sum(1 for p in purchases if p.status == "completed" and p.license_key)

    recent = []
    for p in purchases[:10]:
        product = db.query(Product).filter(Product.id == p.product_id).first()
        recent.append({
            "id": p.id,
            "product_title": product.title if product else "",
            "product_slug": product.slug if product else "",
            "amount": p.amount,
            "status": p.status,
            "created_at": p.created_at.isoformat(),
            "license_key": p.license_key or "",
        })

    return DashboardStats(
        total_products=total_purchases,
        total_sales=active_licenses,
        total_revenue=total_spent,
        total_downloads=total_purchases,
        recent_sales=recent,
    )

@router.get("/analytics", response_model=DashboardStats)
def sales_analytics(user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    if not user.is_seller and not user.is_admin:
        return DashboardStats()
    
    # Get all products for this seller
    products = db.query(Product).filter(Product.seller_id == user.id).all()
    product_ids = [p.id for p in products]
    
    if not product_ids:
        return DashboardStats()
    
    # Total products
    total_products = len(products)
    
    # Total sales and revenue
    completed_purchases = db.query(Purchase).filter(
        Purchase.product_id.in_(product_ids),
        Purchase.status == "completed"
    ).all()
    
    total_sales = len(completed_purchases)
    total_revenue = sum(p.amount for p in completed_purchases)
    total_downloads = sum(p.product.downloads_count or 0 for p in completed_purchases if p.product)
    
    # Daily sales (last 30 days)
    from datetime import datetime, timedelta
    thirty_days_ago = datetime.now(timezone.utc) - timedelta(days=30)
    daily_raw = db.query(
        func.date(Purchase.created_at).label('date'),
        func.sum(Purchase.amount).label('amount'),
        func.count(Purchase.id).label('count')
    ).filter(
        Purchase.product_id.in_(product_ids),
        Purchase.status == "completed",
        Purchase.created_at >= thirty_days_ago
    ).group_by(func.date(Purchase.created_at)).order_by(func.date(Purchase.created_at)).all()
    
    daily_sales = []
    for row in daily_raw:
        daily_sales.append({
            "date": str(row.date),
            "amount": float(row.amount or 0),
            "count": int(row.count or 0)
        })
    
    # Category breakdown
    category_raw = db.query(
        Product.category,
        func.count(Purchase.id).label('sales'),
        func.sum(Purchase.amount).label('revenue')
    ).join(Purchase, Product.id == Purchase.product_id).filter(
        Purchase.product_id.in_(product_ids),
        Purchase.status == "completed"
    ).group_by(Product.category).all()
    
    category_breakdown = []
    for row in category_raw:
        category_breakdown.append({
            "category": row.category,
            "sales": int(row.sales or 0),
            "revenue": float(row.revenue or 0)
        })
    
    # Top products
    top_raw = db.query(
        Product.title,
        func.count(Purchase.id).label('sales'),
        func.sum(Purchase.amount).label('revenue')
    ).join(Purchase, Product.id == Purchase.product_id).filter(
        Purchase.product_id.in_(product_ids),
        Purchase.status == "completed"
    ).group_by(Product.id, Product.title).order_by(func.sum(Purchase.amount).desc()).limit(10).all()
    
    top_products = []
    for row in top_raw:
        top_products.append({
            "product_title": row.title,
            "sales": int(row.sales or 0),
            "revenue": float(row.revenue or 0)
        })
    
    # Recent sales (last 10)
    recent = []
    for p in completed_purchases[-10:]:
        product = db.query(Product).filter(Product.id == p.product_id).first()
        recent.append({
            "id": p.id,
            "product_title": product.title if product else "",
            "product_slug": product.slug if product else "",
            "amount": p.amount,
            "status": p.status,
            "created_at": p.created_at.isoformat(),
        })
    
    return DashboardStats(
        total_products=total_products,
        total_sales=total_sales,
        total_revenue=total_revenue,
        total_downloads=total_downloads,
        recent_sales=recent,
        daily_sales=daily_sales,
        category_breakdown=category_breakdown,
        top_products=top_products,
    )

@router.get("/seller/products", response_model=list[ProductResponse])
def seller_products(user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    if not user.is_seller and not user.is_admin:
        return []
    products = db.query(Product).filter(Product.seller_id == user.id).order_by(Product.created_at.desc()).all()
    result = []
    for p in products:
        r = ProductResponse.model_validate(p)
        if p.seller:
            r.seller_username = p.seller.username
        result.append(r)
    return result
