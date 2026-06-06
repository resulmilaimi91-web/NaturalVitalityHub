#!/usr/bin/env python3
import sys, os, pickle
os.chdir(r'D:\ANDROID\opencode')
from google_auth_oauthlib.flow import InstalledAppFlow

SCOPES = ['https://www.googleapis.com/auth/youtube.upload']
CLIENT_SECRET_FILE = r'D:\ANDROID\opencode\client_secret.json'
TOKEN_FILE = r'D:\ANDROID\opencode\token.pickle'

flow = InstalledAppFlow.from_client_secrets_file(CLIENT_SECRET_FILE, SCOPES)
creds = flow.run_local_server(port=8080, open_browser=True, success_message='OK')
with open(TOKEN_FILE, 'wb') as token:
    pickle.dump(creds, token)
print("SUKSES: Autentifikimi u krye! Token u ruajt.")
