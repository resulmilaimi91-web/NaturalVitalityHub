import time
from collections import defaultdict
from fastapi import Request, Response
from starlette.middleware.base import BaseHTTPMiddleware
from starlette.responses import JSONResponse

# ==================== RATE LIMITING ====================
class RateLimitMiddleware(BaseHTTPMiddleware):
    def __init__(self, app):
        super().__init__(app)
        self.requests = defaultdict(list)
        self.window = 60  # 1 minute window
        self.max_requests = 100  # per IP per window
        
        # Stricter limits for auth endpoints
        self.auth_limits = {
            '/auth/login': {'max': 5, 'window': 900},  # 5 per 15 min
            '/auth/register': {'max': 3, 'window': 3600},  # 3 per hour
            '/auth/forgot-password': {'max': 3, 'window': 3600},  # 3 per hour
        }

    def get_client_ip(self, request: Request) -> str:
        forwarded = request.headers.get("X-Forwarded-For")
        if forwarded:
            return forwarded.split(",")[0].strip()
        return request.client.host if request.client else "unknown"

    def is_rate_limited(self, ip: str, path: str) -> bool:
        now = time.time()
        
        # Check for specific endpoint limits
        for endpoint, limits in self.auth_limits.items():
            if path.startswith(endpoint):
                key = f"{ip}:{endpoint}"
                self.requests[key] = [t for t in self.requests[key] if now - t < limits['window']]
                if len(self.requests[key]) >= limits['max']:
                    return True
                self.requests[key].append(now)
                return False
        
        # General rate limit
        self.requests[ip] = [t for t in self.requests[ip] if now - t < self.window]
        if len(self.requests[ip]) >= self.max_requests:
            return True
        self.requests[ip].append(now)
        return False

    async def dispatch(self, request: Request, call_next):
        ip = self.get_client_ip(request)
        path = request.url.path
        
        if self.is_rate_limited(ip, path):
            return JSONResponse(
                status_code=429,
                content={"detail": "Too many requests. Please try again later."},
                headers={"Retry-After": "60"}
            )
        
        response = await call_next(request)
        return response

# ==================== SECURITY HEADERS ====================
class SecurityHeadersMiddleware(BaseHTTPMiddleware):
    async def dispatch(self, request: Request, call_next):
        response = await call_next(request)
        
        # Security headers
        response.headers["X-Content-Type-Options"] = "nosniff"
        response.headers["X-Frame-Options"] = "DENY"
        response.headers["X-XSS-Protection"] = "1; mode=block"
        response.headers["Referrer-Policy"] = "strict-origin-when-cross-origin"
        response.headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()"
        response.headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.tailwindcss.com https://cdnjs.cloudflare.com; style-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com; font-src 'self' https://cdnjs.cloudflare.com; img-src 'self' data: https:;"
        
        # HSTS (only for HTTPS)
        if request.url.scheme == "https":
            response.headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains"
        
        # Remove server header
        if "server" in response.headers:
            del response.headers["server"]
        
        return response

# ==================== CORS CONFIGURATION ====================
def get_cors_origins():
    origins = [
        "http://localhost:8000",
        "http://127.0.0.1:8000",
    ]
    if hasattr(__import__('app.config', fromlist=['settings']), 'settings'):
        from app.config import settings
        if settings.APP_URL and settings.APP_URL != "http://localhost:8000":
            origins.append(settings.APP_URL)
    return origins
