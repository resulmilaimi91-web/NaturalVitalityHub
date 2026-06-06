import os
from pydantic_settings import BaseSettings
from dotenv import load_dotenv

load_dotenv()

class Settings(BaseSettings):
    DATABASE_URL: str = os.getenv("DATABASE_URL", "sqlite:///./digital_products.db")
    SECRET_KEY: str = os.getenv("SECRET_KEY", "change-this-to-a-random-secret-key")
    ALGORITHM: str = "HS256"
    ACCESS_TOKEN_EXPIRE_MINUTES: int = 1440

    PAYSERA_CLIENT_ID: str = os.getenv("PAYSERA_CLIENT_ID", "")
    PAYSERA_CLIENT_SECRET: str = os.getenv("PAYSERA_CLIENT_SECRET", "")
    PAYSERA_PROJECT_ID: str = os.getenv("PAYSERA_PROJECT_ID", "")
    PAYSERA_WEBHOOK_SECRET: str = os.getenv("PAYSERA_WEBHOOK_SECRET", "")

    SMTP_HOST: str = os.getenv("SMTP_HOST", "")
    SMTP_PORT: int = int(os.getenv("SMTP_PORT", "587"))
    SMTP_USER: str = os.getenv("SMTP_USER", "")
    SMTP_PASSWORD: str = os.getenv("SMTP_PASSWORD", "")
    SMTP_FROM: str = os.getenv("SMTP_FROM", "noreply@digistore.com")
    SMTP_TLS: bool = os.getenv("SMTP_TLS", "true").lower() == "true"

    APP_NAME: str = os.getenv("APP_NAME", "DigiStore")
    APP_URL: str = os.getenv("APP_URL", "http://localhost:8000")
    ADMIN_EMAIL: str = os.getenv("ADMIN_EMAIL", "admin@digistore.com")
    ADMIN_PASSWORD: str = os.getenv("ADMIN_PASSWORD", "admin123")

    class Config:
        env_file = ".env"

settings = Settings()
