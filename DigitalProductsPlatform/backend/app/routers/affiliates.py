import uuid
from fastapi import APIRouter, Depends, HTTPException, Request
from sqlalchemy.orm import Session
from app.database import get_db
from app.models import User, AffiliateLink, AffiliateClick, Purchase
from app.schemas import AffiliateLinkCreate, AffiliateLinkResponse, AffiliateStats
from app.auth import get_current_user
from app.config import settings

router = APIRouter(prefix="/affiliates", tags=["affiliates"])

@router.post("/links", response_model=AffiliateLinkResponse)
def create_affiliate_link(data: AffiliateLinkCreate, user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    existing = db.query(AffiliateLink).filter(AffiliateLink.code == data.code).first()
    if existing:
        raise HTTPException(status_code=400, detail="Affiliate code already taken")
    link = AffiliateLink(user_id=user.id, code=data.code, commission_percentage=data.commission_percentage)
    db.add(link)
    db.commit()
    db.refresh(link)
    r = AffiliateLinkResponse.model_validate(link)
    r.url = f"{settings.APP_URL}?ref={link.code}"
    return r

@router.get("/links", response_model=list[AffiliateLinkResponse])
def list_affiliate_links(user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    links = db.query(AffiliateLink).filter(AffiliateLink.user_id == user.id).order_by(AffiliateLink.created_at.desc()).all()
    result = []
    for link in links:
        r = AffiliateLinkResponse.model_validate(link)
        r.url = f"{settings.APP_URL}?ref={link.code}"
        result.append(r)
    return result

@router.delete("/links/{link_id}")
def delete_affiliate_link(link_id: str, user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    link = db.query(AffiliateLink).filter(AffiliateLink.id == link_id, AffiliateLink.user_id == user.id).first()
    if not link:
        raise HTTPException(status_code=404, detail="Link not found")
    db.delete(link)
    db.commit()
    return {"message": "Link deleted"}

@router.post("/click/{code}")
def track_affiliate_click(code: str, request: Request, db: Session = Depends(get_db)):
    link = db.query(AffiliateLink).filter(AffiliateLink.code == code, AffiliateLink.is_active == True).first()
    if not link:
        return {"tracked": False}
    link.clicks = (link.clicks or 0) + 1
    click = AffiliateClick(
        link_id=link.id,
        ip=request.client.host if request.client else "",
        user_agent=request.headers.get("user-agent", ""),
        referrer=request.headers.get("referer", ""),
    )
    db.add(click)
    db.commit()
    return {"tracked": True}

@router.get("/stats", response_model=AffiliateStats)
def affiliate_stats(user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    links = db.query(AffiliateLink).filter(AffiliateLink.user_id == user.id).all()
    total_links = len(links)
    total_clicks = sum(l.clicks or 0 for l in links)
    total_sales = sum(l.sales_count or 0 for l in links)
    total_revenue = sum(l.revenue_generated or 0 for l in links)
    recent = []
    for l in links:
        clicks = db.query(AffiliateClick).filter(AffiliateClick.link_id == l.id).order_by(AffiliateClick.created_at.desc()).limit(5).all()
        for c in clicks:
            recent.append({
                "code": l.code,
                "ip": c.ip,
                "created_at": c.created_at.isoformat(),
            })
    return AffiliateStats(
        total_links=total_links,
        total_clicks=total_clicks,
        total_sales=total_sales,
        total_revenue=total_revenue,
        recent_clicks=recent[:20],
    )
