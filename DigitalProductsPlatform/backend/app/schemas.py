from pydantic import BaseModel, EmailStr
from typing import Optional
from datetime import datetime

class UserUpdate(BaseModel):
    full_name: Optional[str] = None
    username: Optional[str] = None
    bio: Optional[str] = None
    website: Optional[str] = None
    twitter: Optional[str] = None
    github: Optional[str] = None

class ChangePasswordRequest(BaseModel):
    current_password: str
    new_password: str

class UserRegister(BaseModel):
    email: str
    username: str
    password: str
    full_name: Optional[str] = ""
    is_seller: bool = False

class UserLogin(BaseModel):
    email: str
    password: str

class UserResponse(BaseModel):
    id: str
    email: str
    username: str
    full_name: str
    bio: str = ""
    website: str = ""
    twitter: str = ""
    github: str = ""
    is_seller: bool
    is_admin: bool
    balance: float
    created_at: datetime

    class Config:
        from_attributes = True

class TokenResponse(BaseModel):
    access_token: str
    token_type: str = "bearer"
    user: UserResponse

class ProductCreate(BaseModel):
    title: str
    description: str = ""
    short_description: str = ""
    price: float
    sale_price: Optional[float] = 0.0
    category: str = "plugin"
    tags: str = ""
    current_version: str = "1.0.0"
    requires_license: bool = True

class ProductUpdate(BaseModel):
    title: Optional[str] = None
    description: Optional[str] = None
    short_description: Optional[str] = None
    price: Optional[float] = None
    sale_price: Optional[float] = None
    category: Optional[str] = None
    tags: Optional[str] = None
    is_active: Optional[bool] = None
    is_featured: Optional[bool] = None
    requires_license: Optional[bool] = None

class VersionInfo(BaseModel):
    id: str = ""
    version: str = ""
    changelog: str = ""
    created_at: str = ""

class ProductResponse(BaseModel):
    id: str
    seller_id: str
    title: str
    slug: str
    description: str
    short_description: str
    price: float
    sale_price: float
    category: str
    tags: str
    thumbnail_url: str
    file_size: float
    current_version: str
    downloads_count: int
    sales_count: int
    is_active: bool
    is_featured: bool
    requires_license: bool
    created_at: datetime
    updated_at: datetime
    seller_username: Optional[str] = None
    versions: list = []

    class Config:
        from_attributes = True

class ProductVersionCreate(BaseModel):
    version: str
    changelog: str = ""

class PurchaseResponse(BaseModel):
    id: str
    buyer_id: str
    product_id: str
    amount: float
    currency: str
    status: str
    license_key: str
    created_at: datetime
    product_title: Optional[str] = None
    product_slug: Optional[str] = None

    class Config:
        from_attributes = True

class LicenseResponse(BaseModel):
    id: str
    purchase_id: str
    product_id: str
    license_key: str
    domain: str
    is_active: bool
    created_at: datetime
    expires_at: Optional[datetime] = None

    class Config:
        from_attributes = True

class LicenseValidate(BaseModel):
    license_key: str
    domain: str = ""
    product_slug: str = ""

class AffiliateLinkCreate(BaseModel):
    code: str
    commission_percentage: float = 10.0

class AffiliateLinkResponse(BaseModel):
    id: str
    user_id: str
    code: str
    commission_percentage: float
    clicks: int
    sales_count: int
    revenue_generated: float
    is_active: bool
    created_at: datetime
    url: str = ""

    class Config:
        from_attributes = True

class AffiliateStats(BaseModel):
    total_links: int = 0
    total_clicks: int = 0
    total_sales: int = 0
    total_revenue: float = 0.0
    recent_clicks: list = []

class ReviewCreate(BaseModel):
    product_id: str
    rating: int = 5
    text: str = ""

class ReviewResponse(BaseModel):
    id: str
    product_id: str
    user_id: str
    username: str = ""
    rating: int
    text: str
    created_at: datetime

    class Config:
        from_attributes = True

class CouponCreate(BaseModel):
    code: str
    discount_type: str = "percentage"
    discount_value: float
    min_purchase: float = 0.0
    max_uses: int = 0
    expires_at: Optional[datetime] = None

class CouponUpdate(BaseModel):
    is_active: Optional[bool] = None
    max_uses: Optional[int] = None
    expires_at: Optional[datetime] = None

class CouponResponse(BaseModel):
    id: str
    seller_id: str
    code: str
    discount_type: str
    discount_value: float
    min_purchase: float
    max_uses: int
    current_uses: int
    is_active: bool
    expires_at: Optional[datetime] = None
    created_at: datetime
    discounted_amount: Optional[float] = None

    class Config:
        from_attributes = True

class CouponValidate(BaseModel):
    code: str
    amount: float = 0.0

class CheckoutRequest(BaseModel):
    product_id: str
    coupon_code: str = ""
    success_url: str = ""
    failure_url: str = ""

class CartCheckoutRequest(BaseModel):
    product_ids: list[str]
    coupon_code: str = ""
    success_url: str = ""
    failure_url: str = ""

class CheckoutResponse(BaseModel):
    payment_url: str
    order_id: str

class DashboardStats(BaseModel):
    total_products: int = 0
    total_sales: int = 0
    total_revenue: float = 0.0
    total_downloads: int = 0
    recent_sales: list = []
    daily_sales: list = []  # [{date: "2026-05-20", amount: 120.0, count: 5}]
    category_breakdown: list = []  # [{category: "plugin", sales: 10, revenue: 150.0}]
    top_products: list = []  # [{product_title: "...", sales: 5, revenue: 200.0}]
