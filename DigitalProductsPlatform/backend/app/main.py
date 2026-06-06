import os
import logging
from fastapi import FastAPI, Request
from fastapi.staticfiles import StaticFiles
from fastapi.templating import Jinja2Templates
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import HTMLResponse, RedirectResponse
from app.database import engine, Base, SessionLocal
from app.config import settings
from app.models import User
from app.auth import hash_password
from app.security import RateLimitMiddleware, SecurityHeadersMiddleware, get_cors_origins

from app.routers import auth, products, purchases, licenses, webhooks, dashboard, coupons, reviews, affiliates

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('security.log'),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

Base.metadata.create_all(bind=engine)

app = FastAPI(title=settings.APP_NAME, version="1.0.0")

# Security middleware (order matters - first added = outermost)
app.add_middleware(SecurityHeadersMiddleware)
app.add_middleware(RateLimitMiddleware)

# CORS - Restricted origins only
app.add_middleware(
    CORSMiddleware,
    allow_origins=get_cors_origins(),
    allow_credentials=True,
    allow_methods=["GET", "POST", "PUT", "DELETE"],
    allow_headers=["Authorization", "Content-Type"],
)

app.mount("/static", StaticFiles(directory=os.path.join(os.path.dirname(__file__), "..", "static")), name="static")

app.include_router(auth.router)
app.include_router(products.router)
app.include_router(purchases.router)
app.include_router(licenses.router)
app.include_router(webhooks.router)
app.include_router(dashboard.router)
app.include_router(coupons.router)
app.include_router(reviews.router)
app.include_router(affiliates.router)

templates = Jinja2Templates(directory=os.path.join(os.path.dirname(__file__), "..", "templates"))

def create_admin():
    db = SessionLocal()
    try:
        admin = db.query(User).filter(User.email == settings.ADMIN_EMAIL).first()
        if not admin:
            admin = User(
                email=settings.ADMIN_EMAIL,
                username="admin",
                hashed_password=hash_password(settings.ADMIN_PASSWORD),
                full_name="Administrator",
                is_seller=True,
                is_admin=True,
            )
            db.add(admin)
            db.commit()
            print(f"Admin user created: {settings.ADMIN_EMAIL} / {settings.ADMIN_PASSWORD}")
    finally:
        db.close()

create_admin()

@app.get("/", response_class=HTMLResponse)
def home(request: Request, ref: str = ""):
    response = templates.TemplateResponse(request, "index.html", {"request": request, "app_name": settings.APP_NAME})
    if ref:
        response.set_cookie(key="affiliate_ref", value=ref, max_age=86400 * 30)
    return response

@app.get("/login", response_class=HTMLResponse)
def login_page(request: Request):
    return templates.TemplateResponse(request, "login.html", {"request": request, "app_name": settings.APP_NAME})

@app.get("/register", response_class=HTMLResponse)
def register_page(request: Request):
    return templates.TemplateResponse(request, "register.html", {"request": request, "app_name": settings.APP_NAME})

@app.get("/products", response_class=HTMLResponse)
def products_page(request: Request):
    return templates.TemplateResponse(request, "products.html", {"request": request, "app_name": settings.APP_NAME})

@app.get("/products/{slug}", response_class=HTMLResponse)
def product_detail_page(request: Request, slug: str):
    return templates.TemplateResponse(request, "product_detail.html", {"request": request, "app_name": settings.APP_NAME, "slug": slug})

@app.get("/seller", response_class=HTMLResponse)
def seller_dashboard_page(request: Request):
    return templates.TemplateResponse(request, "seller.html", {"request": request, "app_name": settings.APP_NAME})

@app.get("/seller/new", response_class=HTMLResponse)
def new_product_page(request: Request):
    return templates.TemplateResponse(request, "product_form.html", {"request": request, "app_name": settings.APP_NAME, "edit": False})

@app.get("/seller/edit/{product_id}", response_class=HTMLResponse)
def edit_product_page(request: Request, product_id: str):
    return templates.TemplateResponse(request, "product_form.html", {"request": request, "app_name": settings.APP_NAME, "edit": True, "product_id": product_id})

@app.get("/my-purchases", response_class=HTMLResponse)
def my_purchases_page(request: Request):
    return templates.TemplateResponse(request, "purchases.html", {"request": request, "app_name": settings.APP_NAME})

@app.get("/checkout/success", response_class=HTMLResponse)
def checkout_success_page(request: Request):
    return templates.TemplateResponse(request, "checkout_success.html", {"request": request, "app_name": settings.APP_NAME})

@app.get("/checkout/failure", response_class=HTMLResponse)
def checkout_failure_page(request: Request):
    return templates.TemplateResponse(request, "checkout_failure.html", {"request": request, "app_name": settings.APP_NAME})

@app.get("/forgot-password", response_class=HTMLResponse)
def forgot_password_page(request: Request):
    return templates.TemplateResponse(request, "forgot_password.html", {"request": request, "app_name": settings.APP_NAME})

@app.get("/reset-password", response_class=HTMLResponse)
def reset_password_page(request: Request, token: str = ""):
    return templates.TemplateResponse(request, "reset_password.html", {"request": request, "app_name": settings.APP_NAME, "token": token})

@app.get("/cart", response_class=HTMLResponse)
def cart_page(request: Request):
    return templates.TemplateResponse(request, "cart.html", {"request": request, "app_name": settings.APP_NAME})

@app.get("/profile", response_class=HTMLResponse)
def profile_page(request: Request):
    return templates.TemplateResponse(request, "profile.html", {"request": request, "app_name": settings.APP_NAME})

@app.get("/settings", response_class=HTMLResponse)
def settings_page(request: Request):
    return templates.TemplateResponse(request, "settings.html", {"request": request, "app_name": settings.APP_NAME})

@app.get("/analytics", response_class=HTMLResponse)
def analytics_page(request: Request):
    return templates.TemplateResponse(request, "analytics.html", {"request": request, "app_name": settings.APP_NAME})

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("app.main:app", host="0.0.0.0", port=8000, reload=True)
