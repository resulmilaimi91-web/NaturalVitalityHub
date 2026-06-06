import secrets
print(f"Secret key: {secrets.token_hex(32)}")
print(f"Admin password: {secrets.token_urlsafe(12)}")
