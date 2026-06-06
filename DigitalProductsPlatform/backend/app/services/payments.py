import httpx
import hmac
import hashlib
import json
import logging
from typing import Optional
from app.config import settings

logger = logging.getLogger(__name__)

PAYSERA_AUTH_URL = "https://api.paysera.com/auth/realms/Paysera/protocol/openid-connect/token"
PAYSERA_ORDERS_URL = "https://api.paysera.com/merchant-order/integration/v1/orders"
PAYSERA_LINKS_URL = "https://api.paysera.com/checkout-payment-link/integration/v1/payment-links"

async def get_access_token() -> Optional[str]:
    async with httpx.AsyncClient() as client:
        try:
            response = await client.post(
                PAYSERA_AUTH_URL,
                data={
                    "grant_type": "client_credentials",
                    "client_id": settings.PAYSERA_CLIENT_ID,
                    "client_secret": settings.PAYSERA_CLIENT_SECRET,
                },
                headers={"Content-Type": "application/x-www-form-urlencoded"},
                timeout=30
            )
            if response.is_success:
                return response.json().get("access_token")
            print(f"Paysera auth error: {response.text}")
            return None
        except Exception as e:
            print(f"Paysera auth exception: {e}")
            return None

async def create_payment_order(
    reference: str,
    amount: float,
    currency: str = "EUR",
    success_url: str = "",
    failure_url: str = "",
    callback_url: str = "",
) -> Optional[dict]:
    token = await get_access_token()
    if not token:
        return None

    amount_cents = int(round(amount * 100))
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json",
    }
    payload = {
        "project_id": settings.PAYSERA_PROJECT_ID,
        "purchase": {
            "reference": reference,
            "amount": str(amount_cents),
            "currency": currency,
        },
    }
    if success_url or failure_url or callback_url:
        payload["redirect_urls"] = {}
        if success_url:
            payload["redirect_urls"]["success_url"] = success_url
        if failure_url:
            payload["redirect_urls"]["failure_url"] = failure_url
        if callback_url:
            payload["redirect_urls"]["callback_url"] = callback_url

    async with httpx.AsyncClient() as client:
        try:
            response = await client.post(
                PAYSERA_ORDERS_URL,
                json=payload,
                headers=headers,
                timeout=30
            )
            if response.is_success:
                return response.json()
            print(f"Paysera order error: {response.status_code} {response.text}")
            return None
        except Exception as e:
            print(f"Paysera order exception: {e}")
            return None

async def create_payment_link(
    order_id: str,
    amount: float,
    name: str = "",
    language: str = "en",
    email: str = "",
    lifetime: int = 86400,
) -> Optional[dict]:
    token = await get_access_token()
    if not token:
        return None

    amount_cents = int(round(amount * 100))
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json",
    }
    payload = {
        "order_id": order_id,
        "name": name or "Order Payment",
        "experience": {
            "language": language,
        },
        "purchase": {
            "amount": amount_cents,
        },
        "lifetime": lifetime,
    }
    if email:
        payload["payer_information"] = {"email": email}

    async with httpx.AsyncClient() as client:
        try:
            response = await client.post(
                PAYSERA_LINKS_URL,
                json=payload,
                headers=headers,
                timeout=30
            )
            if response.is_success:
                return response.json()
            print(f"Paysera link error: {response.status_code} {response.text}")
            return None
        except Exception as e:
            print(f"Paysera link exception: {e}")
            return None

def verify_webhook_signature(payload: bytes, signature: str) -> bool:
    # Always verify signature if secret is configured
    if not settings.PAYSERA_WEBHOOK_SECRET:
        logger.warning("PAYSERA_WEBHOOK_SECRET not configured - webhook verification disabled!")
        return False  # Reject if no secret configured (security first)
    
    if not signature:
        logger.warning("No signature provided in webhook")
        return False
    
    try:
        expected = hmac.new(
            settings.PAYSERA_WEBHOOK_SECRET.encode(),
            payload,
            hashlib.sha256
        ).hexdigest()
        return hmac.compare_digest(expected, signature)
    except Exception as e:
        logger.error(f"Webhook signature verification error: {e}")
        return False
