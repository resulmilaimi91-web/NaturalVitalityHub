#!/usr/bin/env python3
import os
import sys
import json
import pickle
from google.auth.transport.requests import Request
from google.oauth2.credentials import Credentials
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from googleapiclient.http import MediaFileUpload

SCOPES = ["https://www.googleapis.com/auth/youtube.upload"]
CLIENT_SECRET_FILE = r"D:\ANDROID\opencode\client_secret.json"
TOKEN_FILE = r"D:\ANDROID\opencode\token.pickle"

def get_authenticated_service():
    creds = None
    if os.path.exists(TOKEN_FILE):
        with open(TOKEN_FILE, "rb") as token:
            creds = pickle.load(token)
    if not creds or not creds.valid:
        if creds and creds.expired and creds.refresh_token:
            creds.refresh(Request())
        else:
            flow = InstalledAppFlow.from_client_secrets_file(CLIENT_SECRET_FILE, SCOPES)
            creds = flow.run_local_server(port=0)
        with open(TOKEN_FILE, "wb") as token:
            pickle.dump(creds, token)
    return build("youtube", "v3", credentials=creds)

def upload_video(video_path, title, description, tags, privacy_status="public"):
    youtube = get_authenticated_service()
    body = {
        "snippet": {
            "title": title,
            "description": description,
            "tags": tags,
            "categoryId": "22"
        },
        "status": {
            "privacyStatus": privacy_status,
            "selfDeclaredMadeForKids": False
        }
    }
    media = MediaFileUpload(video_path, chunksize=-1, resumable=True)
    request = youtube.videos().insert(part="snippet,status", body=body, media_body=media)
    response = None
    while response is None:
        status, response = request.next_chunk()
        if status:
            print(f"Uploaded {int(status.progress() * 100)}%")
    return f"https://youtu.be/{response['id']}"

if __name__ == "__main__":
    args = sys.argv[1:]
    if len(args) < 3:
        print("Përdorimi: python youtube_upload.py <video_path> <title> <description> [privacy] [tags...]")
        sys.exit(1)
    video_path = args[0]
    title = args[1]
    description = args[2]
    tags = args[4:] if len(args) > 4 else []
    privacy = args[3] if len(args) > 3 else "public"
    if not os.path.exists(video_path):
        print(f"Gabim: Video nuk u gjet {video_path}")
        sys.exit(1)
    url = upload_video(video_path, title, description, tags, privacy)
    print(json.dumps({"url": url, "success": True}))
