import os
import shutil
import uuid
import logging
from fastapi import APIRouter, Depends, HTTPException, UploadFile, File, Form, Query
from sqlalchemy.orm import Session
from typing import Optional
from app.database import get_db
from app.models import User, Product, ProductVersion
from app.schemas import ProductCreate, ProductUpdate, ProductResponse, ProductVersionCreate
from app.auth import get_current_user, sanitize_input

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/products", tags=["products"])
UPLOAD_DIR = os.path.join(os.path.dirname(os.path.dirname(__file__)), "static", "uploads")

# Allowed file types and max sizes
ALLOWED_THUMBNAIL_TYPES = {'.jpg', '.jpeg', '.png', '.gif', '.webp'}
ALLOWED_PRODUCT_TYPES = {'.zip', '.rar', '.7z', '.tar', '.gz', '.pdf', '.exe', '.msi', '.dmg', '.pkg'}
MAX_THUMBNAIL_SIZE = 5 * 1024 * 1024  # 5MB
MAX_PRODUCT_SIZE = 500 * 1024 * 1024  # 500MB

def validate_file_upload(file: UploadFile, file_type: str = "product") -> None:
    if not file or not file.filename:
        return
    
    ext = os.path.splitext(file.filename)[1].lower()
    
    if file_type == "thumbnail":
        if ext not in ALLOWED_THUMBNAIL_TYPES:
            raise HTTPException(status_code=400, detail=f"Thumbnail must be one of: {', '.join(ALLOWED_THUMBNAIL_TYPES)}")
    else:
        if ext not in ALLOWED_PRODUCT_TYPES:
            raise HTTPException(status_code=400, detail=f"Product file must be one of: {', '.join(ALLOWED_PRODUCT_TYPES)}")
    
    # Check filename for malicious content
    if '..' in file.filename or '/' in file.filename or '\\' in file.filename:
        raise HTTPException(status_code=400, detail="Invalid filename")

def sanitize_filename(filename: str) -> str:
    # Remove path separators and null bytes
    filename = os.path.basename(filename)
    filename = filename.replace('\x00', '')
    # Only allow alphanumeric, dash, underscore, dot
    filename = ''.join(c for c in filename if c.isalnum() or c in '-_.')
    return filename[:255]  # Limit length

@router.get("", response_model=list[ProductResponse])
def list_products(
    category: Optional[str] = None,
    search: Optional[str] = None,
    seller_id: Optional[str] = None,
    featured: Optional[bool] = None,
    page: int = Query(1, ge=1),
    per_page: int = Query(12, ge=1, le=50),
    db: Session = Depends(get_db),
):
    query = db.query(Product).filter(Product.is_active == True)
    if category:
        query = query.filter(Product.category == category)
    if search:
        query = query.filter(Product.title.ilike(f"%{search}%"))
    if seller_id:
        query = query.filter(Product.seller_id == seller_id)
    if featured:
        query = query.filter(Product.is_featured == True)
    query = query.order_by(Product.created_at.desc())
    total = query.count()
    products = query.offset((page - 1) * per_page).limit(per_page).all()
    result = []
    for p in products:
        r = ProductResponse.model_validate(p)
        if p.seller:
            r.seller_username = p.seller.username
        r.versions = []
        result.append(r)
    return result

@router.get("/featured", response_model=list[ProductResponse])
def featured_products(db: Session = Depends(get_db)):
    products = db.query(Product).filter(Product.is_featured == True, Product.is_active == True).order_by(Product.sales_count.desc()).limit(8).all()
    result = []
    for p in products:
        r = ProductResponse.model_validate(p)
        if p.seller:
            r.seller_username = p.seller.username
        r.versions = []
        result.append(r)
    return result

@router.get("/{slug}", response_model=ProductResponse)
def get_product(slug: str, db: Session = Depends(get_db)):
    product = db.query(Product).filter(Product.slug == slug).first()
    if not product:
        raise HTTPException(status_code=404, detail="Product not found")
    r = ProductResponse.model_validate(product)
    if product.seller:
        r.seller_username = product.seller.username
    versions = db.query(ProductVersion).filter(ProductVersion.product_id == product.id).order_by(ProductVersion.created_at.desc()).all()
    r.versions = [{"id": v.id, "version": v.version, "changelog": v.changelog, "created_at": v.created_at.isoformat()} for v in versions]
    return r

@router.post("", response_model=ProductResponse)
def create_product(
    title: str = Form(...),
    description: str = Form(""),
    short_description: str = Form(""),
    price: float = Form(...),
    sale_price: float = Form(0.0),
    category: str = Form("plugin"),
    tags: str = Form(""),
    current_version: str = Form("1.0.0"),
    requires_license: bool = Form(True),
    file: UploadFile = File(None),
    thumbnail: UploadFile = File(None),
    user: User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    if not user.is_seller and not user.is_admin:
        raise HTTPException(status_code=403, detail="Only sellers can create products")

    # Sanitize inputs
    title = sanitize_input(title)
    description = sanitize_input(description) if description else ""
    short_description = sanitize_input(short_description) if short_description else ""
    tags = sanitize_input(tags) if tags else ""
    
    # Validate price
    if price <= 0:
        raise HTTPException(status_code=400, detail="Price must be greater than 0")
    if sale_price < 0:
        raise HTTPException(status_code=400, detail="Sale price cannot be negative")
    if sale_price > 0 and sale_price >= price:
        raise HTTPException(status_code=400, detail="Sale price must be less than regular price")

    slug = title.lower().replace(" ", "-").replace("--", "-")
    slug = "".join(c for c in slug if c.isalnum() or c in "-_")
    existing = db.query(Product).filter(Product.slug == slug).first()
    if existing:
        slug = f"{slug}-{uuid.uuid4().hex[:4]}"

    # Validate files
    validate_file_upload(thumbnail, "thumbnail")
    validate_file_upload(file, "product")

    product = Product(
        seller_id=user.id,
        title=title,
        slug=slug,
        description=description,
        short_description=short_description,
        price=price,
        sale_price=sale_price,
        category=category,
        tags=tags,
        current_version=current_version,
        requires_license=requires_license,
    )

    if thumbnail:
        ext = os.path.splitext(thumbnail.filename)[1] or ".jpg"
        thumb_path = f"products/{product.id}/thumbnail{ext}"
        full_path = os.path.join(UPLOAD_DIR, thumb_path)
        os.makedirs(os.path.dirname(full_path), exist_ok=True)
        content = thumbnail.file.read()
        
        # Check file size
        if len(content) > MAX_THUMBNAIL_SIZE:
            raise HTTPException(status_code=400, detail=f"Thumbnail too large (max {MAX_THUMBNAIL_SIZE // 1024 // 1024}MB)")
        
        with open(full_path, "wb") as f:
            f.write(content)
        product.thumbnail_url = f"/static/uploads/{thumb_path}"
        logger.info(f"Thumbnail uploaded: {thumb_path} by user {user.email}")

    if file:
        ext = os.path.splitext(file.filename)[1] or ".zip"
        file_path = f"products/{product.id}/release-{current_version}{ext}"
        full_path = os.path.join(UPLOAD_DIR, file_path)
        os.makedirs(os.path.dirname(full_path), exist_ok=True)
        content = file.file.read()
        
        # Check file size
        if len(content) > MAX_PRODUCT_SIZE:
            raise HTTPException(status_code=400, detail=f"Product file too large (max {MAX_PRODUCT_SIZE // 1024 // 1024}MB)")
        with open(full_path, "wb") as f:
            f.write(content)
        product.file_path = f"/static/uploads/{file_path}"
        product.file_size = len(content)

        pv = ProductVersion(
            product_id=product.id,
            version=current_version,
            changelog="Initial release",
            file_path=product.file_path,
            file_size=product.file_size,
        )
        db.add(pv)

    db.add(product)
    db.commit()
    db.refresh(product)
    r = ProductResponse.model_validate(product)
    if product.seller:
        r.seller_username = product.seller.username
    return r

@router.put("/{product_id}", response_model=ProductResponse)
def update_product(
    product_id: str,
    data: ProductUpdate,
    user: User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    product = db.query(Product).filter(Product.id == product_id).first()
    if not product:
        raise HTTPException(status_code=404, detail="Product not found")
    if product.seller_id != user.id and not user.is_admin:
        raise HTTPException(status_code=403, detail="Not your product")

    update_data = data.model_dump(exclude_unset=True)
    for key, value in update_data.items():
        setattr(product, key, value)
    db.commit()
    db.refresh(product)
    r = ProductResponse.model_validate(product)
    if product.seller:
        r.seller_username = product.seller.username
    return r

@router.delete("/{product_id}")
def delete_product(product_id: str, user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    product = db.query(Product).filter(Product.id == product_id).first()
    if not product:
        raise HTTPException(status_code=404, detail="Product not found")
    if product.seller_id != user.id and not user.is_admin:
        raise HTTPException(status_code=403, detail="Not your product")
    db.delete(product)
    db.commit()
    return {"message": "Product deleted"}

@router.post("/{product_id}/versions")
def create_version(
    product_id: str,
    version: str = Form(...),
    changelog: str = Form(""),
    file: UploadFile = File(None),
    user: User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    product = db.query(Product).filter(Product.id == product_id).first()
    if not product:
        raise HTTPException(status_code=404, detail="Product not found")
    if product.seller_id != user.id and not user.is_admin:
        raise HTTPException(status_code=403, detail="Not your product")

    pv = ProductVersion(
        product_id=product_id,
        version=version,
        changelog=changelog,
    )
    if file:
        ext = os.path.splitext(file.filename)[1] or ".zip"
        file_path = f"products/{product_id}/release-{version}{ext}"
        full_path = os.path.join(UPLOAD_DIR, file_path)
        os.makedirs(os.path.dirname(full_path), exist_ok=True)
        content = file.file.read()
        with open(full_path, "wb") as f:
            f.write(content)
        pv.file_path = f"/static/uploads/{file_path}"
        pv.file_size = len(content)
        product.file_path = pv.file_path
        product.file_size = pv.file_size

    product.current_version = version
    db.add(pv)
    db.commit()
    return {"message": f"Version {version} created", "version_id": pv.id}

@router.get("/{product_id}/download")
def download_product(
    product_id: str,
    version_id: Optional[str] = None,
    user: User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    from app.models import Purchase
    purchase = db.query(Purchase).filter(
        Purchase.buyer_id == user.id,
        Purchase.product_id == product_id,
        Purchase.status == "completed"
    ).first()
    if not purchase and not user.is_admin:
        raise HTTPException(status_code=403, detail="You haven't purchased this product")

    if version_id:
        pv = db.query(ProductVersion).filter(ProductVersion.id == version_id, ProductVersion.product_id == product_id).first()
    else:
        pv = db.query(ProductVersion).filter(ProductVersion.product_id == product_id).order_by(ProductVersion.created_at.desc()).first()

    if not pv or not pv.file_path:
        raise HTTPException(status_code=404, detail="No file available")

    product = db.query(Product).filter(Product.id == product_id).first()
    if product:
        product.downloads_count = (product.downloads_count or 0) + 1
        db.commit()

    full_path = os.path.join(UPLOAD_DIR, pv.file_path.replace("/static/uploads/", ""))
    if not os.path.exists(full_path):
        raise HTTPException(status_code=404, detail="File not found")

    from fastapi.responses import FileResponse
    return FileResponse(full_path, filename=f"{product.slug}-v{pv.version}.zip")
