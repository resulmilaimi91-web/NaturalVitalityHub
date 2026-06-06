#!/bin/bash
# Quick deploy for fresh Ubuntu 22.04/24.04 server

set -e

echo "=== DigiStore Quick Server Setup ==="

# Update system
sudo apt-get update && sudo apt-get upgrade -y

# Install Docker
if ! command -v docker &> /dev/null; then
    curl -fsSL https://get.docker.com | sudo bash
    sudo usermod -aG docker $USER
fi

# Install docker-compose
if ! command -v docker-compose &> /dev/null; then
    sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
fi

# Clone project
cd /opt
sudo git clone https://github.com/YOUR_USERNAME/digistore.git
sudo chown -R $USER:$USER digistore
cd digistore

# Run deploy
bash deploy.sh
