"""
Seed script - creates sample products and users for demo/testing.
Run: python seed.py
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
os.chdir(os.path.dirname(os.path.abspath(__file__)))

from app.database import SessionLocal, engine, Base
from app.models import User, Product, ProductVersion
from app.config import settings
from app.auth import hash_password

Base.metadata.create_all(bind=engine)
db = SessionLocal()

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
    db.refresh(admin)

SAMPLE_PRODUCTS = [
    {
        "title": "500 ChatGPT Business Prompts Pack",
        "slug": "chatgpt-business-prompts-pack",
        "description": "Premium collection of 500 tested ChatGPT prompts for business. Covers marketing, sales, customer support, content creation, email campaigns, and strategy. Each prompt includes input variables and expected output format. Save hours of prompt engineering.",
        "short_description": "500 tested AI prompts for marketing, sales, content & more",
        "price": 29.99,
        "sale_price": 19.99,
        "category": "template",
        "tags": "AI, prompts, ChatGPT, business, marketing, GPT",
        "current_version": "2.1",
    },
    {
        "title": "Social Media Canva Templates Bundle",
        "slug": "social-media-canva-templates",
        "description": "200+ premium Canva templates for Instagram, Facebook, LinkedIn, TikTok and Twitter. Includes stories, posts, carousels, cover images, and ads. Fully editable, drag-and-drop. Brand kit included. Perfect for social media managers and businesses.",
        "short_description": "200+ Canva templates for all social media platforms",
        "price": 39.99,
        "sale_price": 24.99,
        "category": "template",
        "tags": "Canva, templates, social media, design, Instagram",
        "current_version": "1.3",
    },
    {
        "title": "Ultimate Notion Business OS Template",
        "slug": "notion-business-os-template",
        "description": "Complete Notion workspace for business management. Includes project management, CRM, finance tracker, content calendar, goal tracking, meeting notes, and HR dashboard. 50+ interconnected databases. Automated workflows included. Used by 2000+ businesses.",
        "short_description": "Complete Notion workspace: CRM, projects, finance & more",
        "price": 49.99,
        "sale_price": 29.99,
        "category": "template",
        "tags": "Notion, template, business, productivity, CRM",
        "current_version": "3.0",
    },
    {
        "title": "Lightroom Mobile Presets - Cinematic Pack",
        "slug": "lightroom-mobile-presets-cinematic",
        "description": "50 professional Lightroom mobile presets for cinematic photography. Includes street, portrait, landscape, and moody styles. Works with Lightroom CC and Mobile. One-click apply. Before/after examples included. Perfect for Instagram photographers.",
        "short_description": "50 cinematic Lightroom presets for mobile & desktop",
        "price": 19.99,
        "sale_price": 9.99,
        "category": "other",
        "tags": "Lightroom, presets, photography, cinematic, mobile",
        "current_version": "1.0",
    },
    {
        "title": "WooCommerce Product Table Plugin",
        "slug": "woocommerce-product-table-plugin",
        "description": "Advanced WooCommerce product table plugin. Display products in sortable, filterable tables with AJAX cart. Variable products, quantity inputs, column customization, CSV export. Compatible with any WordPress theme.",
        "short_description": "Display WooCommerce products in sortable data tables",
        "price": 59.99,
        "sale_price": 39.99,
        "category": "plugin",
        "tags": "WooCommerce, WordPress, plugin, ecommerce, table",
        "current_version": "2.4",
    },
    {
        "title": "React SaaS Dashboard Starter Kit",
        "slug": "react-saas-dashboard-starter",
        "description": "Production-ready React dashboard starter with authentication, charts, tables, dark mode, and API integration. Built with React 19, Tailwind CSS 4, TypeScript. Includes Stripe billing, user management, settings pages, and 10+ pre-built widgets.",
        "short_description": "React + Tailwind dashboard with auth, billing & dark mode",
        "price": 79.99,
        "sale_price": 49.99,
        "category": "code",
        "tags": "React, dashboard, Tailwind, TypeScript, SaaS, starter",
        "current_version": "1.0",
    },
]

for data in SAMPLE_PRODUCTS:
    existing = db.query(Product).filter(Product.slug == data["slug"]).first()
    if existing:
        print(f"[SKIP] Product already exists: {data['title']}")
        continue

    product = Product(
        seller_id=admin.id,
        title=data["title"],
        slug=data["slug"],
        description=data["description"],
        short_description=data["short_description"],
        price=data["price"],
        sale_price=data["sale_price"],
        category=data["category"],
        tags=data["tags"],
        current_version=data["current_version"],
        is_active=True,
        is_featured=True,
    )
    db.add(product)
    db.flush()

    pv = ProductVersion(
        product_id=product.id,
        version=data["current_version"],
        changelog="Initial release",
    )
    db.add(pv)
    print(f"[CREATED] {data['title']} - EUR {data['price']}")

db.commit()
db.close()

print("\n[DONE] Seed completed!")
print(f"Admin: {settings.ADMIN_EMAIL} / {settings.ADMIN_PASSWORD}")
print("Login as admin to see products in dashboard")
