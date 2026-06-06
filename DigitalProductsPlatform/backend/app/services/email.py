import os
import smtplib
import ssl
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart
from typing import Optional
from jinja2 import Environment, FileSystemLoader
from app.config import settings

templates = Environment(loader=FileSystemLoader(os.path.join(os.path.dirname(os.path.dirname(__file__)), "templates", "emails")))

def send_email(to_email: str, subject: str, template_name: str, context: dict) -> bool:
    if not settings.SMTP_HOST:
        print(f"[EMAIL] SMTP not configured — skipping email to {to_email} (subject: {subject})")
        return False
    try:
        html = templates.get_template(template_name).render(**context)
        msg = MIMEMultipart("alternative")
        msg["From"] = settings.SMTP_FROM
        msg["To"] = to_email
        msg["Subject"] = subject
        msg.attach(MIMEText(html, "html"))

        ctx = ssl.create_default_context()
        with smtplib.SMTP(settings.SMTP_HOST, settings.SMTP_PORT) as server:
            if settings.SMTP_TLS:
                server.starttls(context=ctx)
            if settings.SMTP_USER:
                server.login(settings.SMTP_USER, settings.SMTP_PASSWORD)
            server.sendmail(settings.SMTP_FROM, to_email, msg.as_string())
        print(f"[EMAIL] Sent to {to_email}: {subject}")
        return True
    except Exception as e:
        print(f"[EMAIL] Error sending to {to_email}: {e}")
        return False

def send_purchase_confirmation(to_email: str, username: str, product_title: str, amount: float, license_key: str = "", download_url: str = ""):
    send_email(to_email, f"Purchase Confirmation — {product_title}", "purchase_confirmation.html", {
        "username": username,
        "product_title": product_title,
        "amount": amount,
        "app_name": settings.APP_NAME,
        "app_url": settings.APP_URL,
        "license_key": license_key,
        "download_url": download_url,
    })

def send_license_key(to_email: str, username: str, product_title: str, license_key: str):
    send_email(to_email, f"Your License Key — {product_title}", "license_key.html", {
        "username": username,
        "product_title": product_title,
        "license_key": license_key,
        "app_name": settings.APP_NAME,
        "app_url": settings.APP_URL,
    })

def send_password_reset(to_email: str, username: str, reset_url: str):
    send_email(to_email, "Password Reset — " + settings.APP_NAME, "password_reset.html", {
        "username": username,
        "reset_url": reset_url,
        "app_name": settings.APP_NAME,
        "app_url": settings.APP_URL,
    })

def send_sale_notification(to_email: str, seller_name: str, product_title: str, amount: float, buyer_email: str):
    send_email(to_email, f"New Sale — {product_title}", "sale_notification.html", {
        "seller_name": seller_name,
        "product_title": product_title,
        "amount": amount,
        "buyer_email": buyer_email,
        "app_name": settings.APP_NAME,
        "app_url": settings.APP_URL,
    })
