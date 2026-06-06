#!/bin/bash
set -e

# DigiStore Production Deployment Script
# Uses: Docker, Let's Encrypt SSL, PostgreSQL

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  DigiStore - Production Deployment${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Check requirements
command -v docker >/dev/null 2>&1 || { echo -e "${RED}Docker is required but not installed.${NC}"; exit 1; }
command -v docker-compose >/dev/null 2>&1 || { echo -e "${RED}docker-compose is required but not installed.${NC}"; exit 1; }

# Get domain
read -p "Enter your domain (e.g., digistore.com): " DOMAIN
if [ -z "$DOMAIN" ]; then
    echo -e "${RED}Domain is required${NC}"
    exit 1
fi

# Generate secrets
SECRET_KEY=$(python3 -c "import secrets; print(secrets.token_hex(32))")
ADMIN_PASS=$(python3 -c "import secrets; print(secrets.token_urlsafe(12))")

echo -e "${YELLOW}Generated admin password: ${ADMIN_PASS}${NC}"
echo -e "${YELLOW}Save this password! It will not be shown again.${NC}"
echo ""

# Clone / copy project
if [ ! -f "docker-compose.yml" ]; then
    echo -e "${RED}Run this script from the project root directory${NC}"
    exit 1
fi

# Ask for Paysera credentials
echo "=== Paysera Configuration ==="
echo "Get these from https://developers.paysera.com"
read -p "Paysera Client ID: " PAYSERA_CLIENT_ID
read -p "Paysera Client Secret: " PAYSERA_CLIENT_SECRET
read -p "Paysera Project ID: " PAYSERA_PROJECT_ID
read -p "Paysera Webhook Secret: " PAYSERA_WEBHOOK_SECRET

# Create .env
cat > .env << EOF
DATABASE_URL=postgresql://digistore:${SECRET_KEY}@postgres:5432/digistore
SECRET_KEY=${SECRET_KEY}
APP_URL=https://${DOMAIN}
APP_NAME=DigiStore
ADMIN_EMAIL=admin@${DOMAIN}
ADMIN_PASSWORD=${ADMIN_PASS}
PAYSERA_CLIENT_ID=${PAYSERA_CLIENT_ID}
PAYSERA_CLIENT_SECRET=${PAYSERA_CLIENT_SECRET}
PAYSERA_PROJECT_ID=${PAYSERA_PROJECT_ID}
PAYSERA_WEBHOOK_SECRET=${PAYSERA_WEBHOOK_SECRET}
EOF

echo -e "${GREEN}.env file created${NC}"

# Setup SSL with Let's Encrypt
echo ""
echo "=== SSL Certificate Setup ==="
echo "Make sure your domain DNS points to this server's IP!"
read -p "Press Enter when DNS is configured..."

# Create nginx SSL config
cat > nginx.ssl.conf << EOF
server {
    listen 80;
    server_name ${DOMAIN} www.${DOMAIN};
    return 301 https://\$server_name\$request_uri;
}

server {
    listen 443 ssl http2;
    server_name ${DOMAIN} www.${DOMAIN};

    ssl_certificate /etc/letsencrypt/live/${DOMAIN}/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/${DOMAIN}/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    client_max_body_size 500M;

    location /static/ {
        alias /app/static/;
        expires 30d;
        add_header Cache-Control "public, immutable";
    }

    location / {
        proxy_pass http://127.0.0.1:8000;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_read_timeout 120s;
    }
}
EOF

# Install certbot and get SSL
sudo apt-get update
sudo apt-get install -y certbot python3-certbot-nginx
sudo certbot --nginx -d ${DOMAIN} -d www.${DOMAIN} --non-interactive --agree-tos --email admin@${DOMAIN}

# Update nginx config
cp nginx.ssl.conf nginx.conf

# Start services
echo ""
echo "=== Starting Services ==="
docker-compose down 2>/dev/null || true
docker-compose up -d --build

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  Deployment Complete!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "Website: ${YELLOW}https://${DOMAIN}${NC}"
echo -e "Admin:   ${YELLOW}admin@${DOMAIN} / ${ADMIN_PASS}${NC}"
echo ""
echo -e "Paysera webhook URL:"
echo -e "${YELLOW}https://${DOMAIN}/webhooks/paysera${NC}"
echo ""
echo -e "Quick commands:"
echo -e "  View logs:    ${YELLOW}docker-compose logs -f${NC}"
echo -e "  Restart:      ${YELLOW}docker-compose restart${NC}"
echo -e "  Stop:         ${YELLOW}docker-compose down${NC}"
echo -e "  Update:       ${YELLOW}git pull && docker-compose up -d --build${NC}"
echo ""
