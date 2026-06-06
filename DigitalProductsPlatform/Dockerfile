FROM python:3.12-slim

WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    nginx \
    curl \
    && rm -rf /var/lib/apt/lists/*

COPY backend/requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY backend/ .

COPY nginx.conf /etc/nginx/sites-enabled/default

RUN mkdir -p /app/static/uploads

RUN echo '#!/bin/bash\n\
set -e\n\
nginx\n\
exec gunicorn app.main:app --worker-class uvicorn.workers.UvicornWorker --bind 127.0.0.1:8000 --workers 4' > /start.sh && chmod +x /start.sh

EXPOSE 80

CMD ["/start.sh"]
