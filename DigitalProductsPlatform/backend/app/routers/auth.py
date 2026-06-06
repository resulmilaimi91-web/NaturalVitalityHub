import os
import secrets
import logging
from fastapi import APIRouter, Depends, HTTPException, status, Request
from fastapi.responses import HTMLResponse, RedirectResponse
from fastapi.templating import Jinja2Templates
from sqlalchemy.orm import Session
from pydantic import BaseModel
from app.database import get_db
from app.models import User
from app.schemas import UserRegister, UserLogin, UserUpdate, ChangePasswordRequest, TokenResponse, UserResponse
from app.auth import (
    hash_password, verify_password, create_access_token, get_current_user,
    is_account_locked, record_failed_attempt, clear_failed_attempts,
    validate_password_strength, sanitize_input, validate_email
)
from app.services.email import send_password_reset
from app.config import settings

logger = logging.getLogger(__name__)

templates = Jinja2Templates(directory=os.path.join(os.path.dirname(os.path.dirname(__file__)), "..", "templates"))

class ForgotPasswordRequest(BaseModel):
    email: str

class ResetPasswordRequest(BaseModel):
    token: str
    password: str

router = APIRouter(prefix="/auth", tags=["auth"])

@router.post("/register", response_model=TokenResponse)
def register(data: UserRegister, request: Request, db: Session = Depends(get_db)):
    # Sanitize inputs
    data.email = sanitize_input(data.email.lower().strip())
    data.username = sanitize_input(data.username.strip())
    data.full_name = sanitize_input(data.full_name) if data.full_name else ""
    
    # Validate email
    if not validate_email(data.email):
        raise HTTPException(status_code=400, detail="Invalid email format")
    
    # Validate username
    if len(data.username) < 3 or len(data.username) > 20:
        raise HTTPException(status_code=400, detail="Username must be 3-20 characters")
    if not data.username.isalnum():
        raise HTTPException(status_code=400, detail="Username must contain only letters and numbers")
    
    # Validate password strength
    is_valid, message = validate_password_strength(data.password)
    if not is_valid:
        raise HTTPException(status_code=400, detail=message)
    
    # Check existing
    existing = db.query(User).filter(
        (User.email == data.email) | (User.username == data.username)
    ).first()
    if existing:
        logger.warning(f"Registration attempt with existing email/username: {data.email}")
        raise HTTPException(status_code=400, detail="Email or username already exists")

    user = User(
        email=data.email,
        username=data.username,
        hashed_password=hash_password(data.password),
        full_name=data.full_name,
        is_seller=data.is_seller,
    )
    db.add(user)
    db.commit()
    db.refresh(user)

    logger.info(f"New user registered: {data.email} from {request.client.host if request.client else 'unknown'}")
    token = create_access_token({"sub": user.id, "email": user.email})
    return TokenResponse(access_token=token, user=UserResponse.model_validate(user))

@router.post("/login", response_model=TokenResponse)
def login(data: UserLogin, request: Request, db: Session = Depends(get_db)):
    # Sanitize inputs
    email = sanitize_input(data.email.lower().strip())
    
    # Check account lockout
    if is_account_locked(email):
        logger.warning(f"Login attempt on locked account: {email} from {request.client.host if request.client else 'unknown'}")
        raise HTTPException(status_code=429, detail="Account temporarily locked. Try again later.")
    
    user = db.query(User).filter(User.email == email).first()
    if not user or not verify_password(data.password, user.hashed_password):
        record_failed_attempt(email)
        logger.warning(f"Failed login attempt for: {email} from {request.client.host if request.client else 'unknown'}")
        raise HTTPException(status_code=401, detail="Invalid email or password")

    clear_failed_attempts(email)
    logger.info(f"Successful login: {email} from {request.client.host if request.client else 'unknown'}")
    
    token = create_access_token({"sub": user.id, "email": user.email})
    return TokenResponse(access_token=token, user=UserResponse.model_validate(user))

@router.get("/me", response_model=UserResponse)
def get_me(user: User = Depends(get_current_user)):
    return UserResponse.model_validate(user)

@router.put("/profile", response_model=UserResponse)
def update_profile(data: UserUpdate, user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    update_data = data.model_dump(exclude_unset=True)
    
    # Sanitize inputs
    for key in ['full_name', 'username', 'bio', 'website', 'twitter', 'github']:
        if key in update_data and update_data[key]:
            update_data[key] = sanitize_input(update_data[key])
    
    if "username" in update_data and update_data["username"] != user.username:
        if len(update_data["username"]) < 3 or len(update_data["username"]) > 20:
            raise HTTPException(status_code=400, detail="Username must be 3-20 characters")
        if not update_data["username"].isalnum():
            raise HTTPException(status_code=400, detail="Username must contain only letters and numbers")
        existing = db.query(User).filter(User.username == update_data["username"]).first()
        if existing:
            raise HTTPException(status_code=400, detail="Username already taken")
    
    for key, value in update_data.items():
        setattr(user, key, value)
    db.commit()
    db.refresh(user)
    return UserResponse.model_validate(user)

@router.post("/change-password")
def change_password(data: ChangePasswordRequest, user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    # Validate current password
    if not verify_password(data.current_password, user.hashed_password):
        logger.warning(f"Failed password change attempt for user: {user.email}")
        raise HTTPException(status_code=400, detail="Current password is incorrect")
    
    # Validate new password strength
    is_valid, message = validate_password_strength(data.new_password)
    if not is_valid:
        raise HTTPException(status_code=400, detail=message)
    
    user.hashed_password = hash_password(data.new_password)
    db.commit()
    logger.info(f"Password changed for user: {user.email}")
    return {"message": "Password changed successfully"}

@router.get("/public/{user_id}", response_model=UserResponse)
def get_public_profile(user_id: str, db: Session = Depends(get_db)):
    user = db.query(User).filter(User.id == user_id).first()
    if not user:
        raise HTTPException(status_code=404, detail="User not found")
    return UserResponse.model_validate(user)

@router.post("/forgot-password")
def forgot_password(data: ForgotPasswordRequest, request: Request, db: Session = Depends(get_db)):
    email = sanitize_input(data.email.lower().strip())
    user = db.query(User).filter(User.email == email).first()
    
    if user:
        token = create_access_token({"sub": user.id, "purpose": "password_reset"}, expires_minutes=60)
        reset_url = f"{settings.APP_URL}/reset-password?token={token}"
        send_password_reset(to_email=user.email, username=user.username, reset_url=reset_url)
        logger.info(f"Password reset requested for: {email} from {request.client.host if request.client else 'unknown'}")
    
    # Always return same message to prevent email enumeration
    return {"message": "If that email exists, a reset link has been sent"}

@router.post("/reset-password")
def reset_password(data: ResetPasswordRequest, db: Session = Depends(get_db)):
    from app.auth import decode_token
    
    # Validate password strength
    is_valid, message = validate_password_strength(data.password)
    if not is_valid:
        raise HTTPException(status_code=400, detail=message)
    
    payload = decode_token(data.token)
    if not payload or payload.get("purpose") != "password_reset":
        logger.warning("Invalid password reset token used")
        raise HTTPException(status_code=400, detail="Invalid or expired token")
    
    user = db.query(User).filter(User.id == payload.get("sub")).first()
    if not user:
        raise HTTPException(status_code=400, detail="User not found")
    
    user.hashed_password = hash_password(data.password)
    db.commit()
    logger.info(f"Password reset completed for user: {user.email}")
    return {"message": "Password reset successful"}
