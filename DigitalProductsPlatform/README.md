# DigiStore - Digital Products Marketplace

Platformë full-stack për shitjen e produkteve digitale (plugins, themes, templates, code) me integrim Paysera, license management, dhe auto-update system.

## Features

- Auth (register/login JWT)
- Product CRUD + file upload + version management
- Browse products me search & category filter
- **Paysera payment integration** (pagesa në IBAN Lituanez)
- License key generation & auto-update API
- Seller dashboard (sales, revenue, downloads)
- Buyer dashboard (purchases, licenses)
- Webhook për konfirmim pagesash

## Quick Start (Development)

```bash
# 1. Instalo Python 3.10+
# 2. Instalo dependencies
cd backend
pip install -r requirements.txt

# 3. Konfiguro .env
copy .env.example .env
# Vendos të dhënat e Paysera

# 4. Nis serverin
python -m uvicorn app.main:app --host 0.0.0.0 --port 8000

# 5. Hap në browser
http://localhost:8000
```

## Production Deployment (Docker)

### 1. Blej nje VPS
**Recommended specs:**
| Plan | RAM | CPU | Storage | Price/muaj |
|------|-----|-----|---------|------------|
| Minimal | 1GB | 1 core | 25GB SSD | ~€4-6 |
| Recommended | 2GB | 2 cores | 50GB SSD | ~€8-12 |

**Hosting providers:**
- **Hetzner** (Gjermani) - €3.99/muaj - https://hetzner.com
- **Contabo** (Gjermani) - €6.99/muaj - https://contabo.com
- **DigitalOcean** - $6/muaj - https://digitalocean.com
- **Netcup** (Gjermani) - €3.50/muaj - https://netcup.de

### 2. Blej nje Domain
- **Namecheap** - ~€8/vit
- **GoDaddy** - ~€10/vit
- **Porkbun** - ~€7/vit

Vendos DNS-në e domain-it të tregojë nga IP e VPS-së.

### 3. Deploy me një komandë

```bash
# SSH në VPS
ssh root@ip-juaj

# Instalo Docker dhe klono projektin
bash <(curl -s https://raw.githubusercontent.com/...)

# Ose manualisht:
git clone https://github.com/your/project.git
cd digistore
bash deploy.sh
```

Script-i `deploy.sh` bën automatikisht:
- Gjeneron SECRET_KEY dhe admin password
- Konfiguron SSL me Let's Encrypt
- Nis PostgreSQL + App me Docker
- Konfiguron Nginx

### 4. Konfiguro Paysera

1. Hap llogari **Paysera Business** (https://paysera.com)
2. Shko te **Developers** → **API Integration**
3. Krijo **Project** të ri
4. Merr **Client ID**, **Client Secret**, **Project ID**
5. Vendos **Webhook URL**: `https://yt-domaini.com/webhooks/paysera`
6. Fut këto në `.env`

### 5. Gati për shitje!

Platforma jeton në: **https://yt-domaini.com**

## Paysera Integration Details

- **Merchant**: Lithuania (Paysera licensed by Bank of Lithuania)
- **Settlement**: EUR to Lithuanian IBAN
- **Fees**: ~1-2% per transaction
- **Payout**: Next business day
- **Payment methods**: Cards, banklinks, wallets, SEPA

### Payment Flow:
1. User klikon "Purchase"
2. Backend krijon order në Paysera
3. Redirect te Paysera checkout
4. Pas pagesës, Paysera dërgon webhook
5. Sistemi gjeneron license key
6. User shkarkon produktin nga "My Purchases"

## Default Accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@digistore.com | admin123 (ndërroje në production) |
| Seller | Regjistrohu si seller | - |
| Buyer | Regjistrohu si buyer | - |

## API Endpoints

### Auth
- `POST /auth/register` - Register
- `POST /auth/login` - Login
- `GET /auth/me` - Current user

### Products
- `GET /products` - List products
- `GET /products/featured` - Featured
- `GET /products/{slug}` - Detail
- `POST /products` - Create (multipart)
- `PUT /products/{id}` - Update
- `DELETE /products/{id}` - Delete
- `POST /products/{id}/versions` - Add version
- `GET /products/{id}/download` - Download (auth + purchase required)

### Checkout
- `POST /checkout/create` - Paysera checkout
- `GET /checkout/purchases` - My purchases

### Licenses
- `GET /licenses/my` - My licenses
- `POST /licenses/validate` - Validate license key
- `POST /licenses/activate` - Activate license

### Webhooks
- `POST /webhooks/paysera` - Paysera callback

### Dashboard
- `GET /dashboard/seller` - Seller stats
- `GET /dashboard/buyer` - Buyer stats
- `GET /dashboard/seller/products` - Seller's products

## Project Structure

```
digistore/
├── backend/
│   ├── app/
│   │   ├── main.py          # FastAPI app
│   │   ├── config.py        # Settings (.env)
│   │   ├── database.py      # DB connection
│   │   ├── models.py        # SQLAlchemy models
│   │   ├── schemas.py       # Pydantic schemas
│   │   ├── auth.py          # JWT auth
│   │   ├── routers/         # API endpoints
│   │   └── services/        # Paysera, licenses
│   ├── templates/           # HTML templates (Tailwind CSS)
│   ├── static/uploads/      # Product files
│   └── requirements.txt
├── Dockerfile.prod          # Production container
├── docker-compose.yml       # PostgreSQL + App
├── nginx.conf               # Nginx config
├── deploy.sh                # Deploy script (automatic)
├── .env.production          # Example env
└── README.md
```

## Commands

```bash
# Lokal (development)
cd backend && python -m uvicorn app.main:app --reload

# Me Docker (production)
docker-compose up -d --build

# Logs
docker-compose logs -f

# Stop
docker-compose down

# Update (pas git pull)
docker-compose up -d --build
```
