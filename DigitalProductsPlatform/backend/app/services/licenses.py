import uuid
import hashlib
from datetime import datetime, timezone, timedelta
from typing import Optional

def generate_license_key(
    product_id: str,
    purchase_id: str,
    prefix: str = "DGS"
) -> str:
    raw = f"{prefix}-{product_id[:8]}-{purchase_id[:8]}"
    raw = raw.upper()
    parts = []
    for i in range(0, len(raw), 4):
        chunk = raw[i:i+4]
        if len(chunk) == 4:
            parts.append(chunk)
    if parts:
        last = parts[-1]
        h = hashlib.md5(last.encode()).hexdigest()[:4].upper()
        parts.append(h)
    return "-".join(parts)

def validate_license_format(license_key: str) -> bool:
    parts = license_key.split("-")
    if len(parts) < 3:
        return False
    return all(len(p) == 4 for p in parts[:3])

def calculate_expiry(days: int = 365) -> datetime:
    return datetime.now(timezone.utc) + timedelta(days=days)
