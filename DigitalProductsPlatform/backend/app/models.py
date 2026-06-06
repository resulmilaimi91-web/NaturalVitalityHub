import uuid
from datetime import datetime, timezone
from sqlalchemy import Column, String, Text, Float, Integer, Boolean, DateTime, ForeignKey
from sqlalchemy.orm import relationship
from app.database import Base

def generate_uuid():
    return str(uuid.uuid4())

def utcnow():
    return datetime.now(timezone.utc)

class User(Base):
    __tablename__ = "users"

    id = Column(String, primary_key=True, default=generate_uuid)
    email = Column(String, unique=True, index=True, nullable=False)
    username = Column(String, unique=True, index=True, nullable=False)
    hashed_password = Column(String, nullable=False)
    full_name = Column(String, default="")
    bio = Column(String, default="")
    website = Column(String, default="")
    twitter = Column(String, default="")
    github = Column(String, default="")
    is_seller = Column(Boolean, default=False)
    is_admin = Column(Boolean, default=False)
    balance = Column(Float, default=0.0)
    created_at = Column(DateTime, default=utcnow)
    updated_at = Column(DateTime, default=utcnow, onupdate=utcnow)

    products = relationship("Product", back_populates="seller")
    purchases = relationship("Purchase", back_populates="buyer")

class Product(Base):
    __tablename__ = "products"

    id = Column(String, primary_key=True, default=generate_uuid)
    seller_id = Column(String, ForeignKey("users.id"), nullable=False)
    title = Column(String, nullable=False)
    slug = Column(String, unique=True, index=True, nullable=False)
    description = Column(Text, default="")
    short_description = Column(String, default="")
    price = Column(Float, nullable=False)
    sale_price = Column(Float, default=0.0)
    category = Column(String, default="plugin")
    tags = Column(String, default="")
    thumbnail_url = Column(String, default="")
    file_path = Column(String, default="")
    file_size = Column(Float, default=0.0)
    file_type = Column(String, default="zip")
    current_version = Column(String, default="1.0.0")
    downloads_count = Column(Integer, default=0)
    sales_count = Column(Integer, default=0)
    is_active = Column(Boolean, default=True)
    is_featured = Column(Boolean, default=False)
    requires_license = Column(Boolean, default=True)
    created_at = Column(DateTime, default=utcnow)
    updated_at = Column(DateTime, default=utcnow, onupdate=utcnow)

    seller = relationship("User", back_populates="products")
    versions = relationship("ProductVersion", back_populates="product", order_by="ProductVersion.created_at.desc()")
    purchases = relationship("Purchase", back_populates="product")

class ProductVersion(Base):
    __tablename__ = "product_versions"

    id = Column(String, primary_key=True, default=generate_uuid)
    product_id = Column(String, ForeignKey("products.id"), nullable=False)
    version = Column(String, nullable=False)
    changelog = Column(Text, default="")
    file_path = Column(String, default="")
    file_size = Column(Float, default=0.0)
    is_active = Column(Boolean, default=True)
    created_at = Column(DateTime, default=utcnow)

    product = relationship("Product", back_populates="versions")

class Purchase(Base):
    __tablename__ = "purchases"

    id = Column(String, primary_key=True, default=generate_uuid)
    buyer_id = Column(String, ForeignKey("users.id"), nullable=False)
    product_id = Column(String, ForeignKey("products.id"), nullable=False)
    amount = Column(Float, nullable=False)
    currency = Column(String, default="EUR")
    status = Column(String, default="pending")
    paysera_order_id = Column(String, default="")
    paysera_status = Column(String, default="")
    license_key = Column(String, default="")
    affiliate_code = Column(String, default="")
    affiliate_commission = Column(Float, default=0.0)
    created_at = Column(DateTime, default=utcnow)
    updated_at = Column(DateTime, default=utcnow, onupdate=utcnow)

    buyer = relationship("User", back_populates="purchases")
    product = relationship("Product", back_populates="purchases")

class AffiliateLink(Base):
    __tablename__ = "affiliate_links"

    id = Column(String, primary_key=True, default=generate_uuid)
    user_id = Column(String, ForeignKey("users.id"), nullable=False)
    code = Column(String, unique=True, index=True, nullable=False)
    commission_percentage = Column(Float, default=10.0)
    clicks = Column(Integer, default=0)
    sales_count = Column(Integer, default=0)
    revenue_generated = Column(Float, default=0.0)
    is_active = Column(Boolean, default=True)
    created_at = Column(DateTime, default=utcnow)

    user = relationship("User")

class AffiliateClick(Base):
    __tablename__ = "affiliate_clicks"

    id = Column(String, primary_key=True, default=generate_uuid)
    link_id = Column(String, ForeignKey("affiliate_links.id"), nullable=False)
    ip = Column(String, default="")
    user_agent = Column(String, default="")
    referrer = Column(String, default="")
    created_at = Column(DateTime, default=utcnow)

class Review(Base):
    __tablename__ = "reviews"

    id = Column(String, primary_key=True, default=generate_uuid)
    product_id = Column(String, ForeignKey("products.id"), nullable=False)
    user_id = Column(String, ForeignKey("users.id"), nullable=False)
    rating = Column(Integer, default=5)
    text = Column(Text, default="")
    created_at = Column(DateTime, default=utcnow)

    user = relationship("User")

class Coupon(Base):
    __tablename__ = "coupons"

    id = Column(String, primary_key=True, default=generate_uuid)
    seller_id = Column(String, ForeignKey("users.id"), nullable=False)
    code = Column(String, index=True, nullable=False)
    discount_type = Column(String, default="percentage")
    discount_value = Column(Float, nullable=False)
    min_purchase = Column(Float, default=0.0)
    max_uses = Column(Integer, default=0)
    current_uses = Column(Integer, default=0)
    is_active = Column(Boolean, default=True)
    expires_at = Column(DateTime, nullable=True)
    created_at = Column(DateTime, default=utcnow)

class License(Base):
    __tablename__ = "licenses"

    id = Column(String, primary_key=True, default=generate_uuid)
    purchase_id = Column(String, ForeignKey("purchases.id"), nullable=False)
    product_id = Column(String, ForeignKey("products.id"), nullable=False)
    license_key = Column(String, unique=True, index=True, nullable=False)
    domain = Column(String, default="")
    is_active = Column(Boolean, default=True)
    max_activations = Column(Integer, default=1)
    activated_at = Column(DateTime, nullable=True)
    expires_at = Column(DateTime, nullable=True)
    created_at = Column(DateTime, default=utcnow)
    updated_at = Column(DateTime, default=utcnow, onupdate=utcnow)

    purchase = relationship("Purchase")
    product = relationship("Product")
