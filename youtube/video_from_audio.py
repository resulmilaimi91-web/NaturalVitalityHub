#!/usr/bin/env python3
import os
from moviepy import AudioFileClip, ImageClip
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from googleapiclient.http import MediaFileUpload

AUDIO_FILE = "muzika_virale.mp3"
BACKGROUND_IMAGE = "sfondi_video.jpg"
OUTPUT_VIDEO = "video_gati.mp4"

VIDEO_TITLE = "Muzika me Virale e Momentit 2026 #shorts"
VIDEO_DESCRIPTION = "Degjoni ket hit! Mos harroni te abonoheni per me shume muzikë virale. #music #viral"
VIDEO_TAGS = ["music", "viral", "shorts", "trending music"]

SCOPES = ["https://www.googleapis.com/auth/youtube.upload"]

def krijo_videon():
    print("Duke krijuar videon...")
    try:
        audio_clip = AudioFileClip(AUDIO_FILE)
        kohezgjatja = audio_clip.duration
        video_clip = ImageClip(BACKGROUND_IMAGE).set_duration(kohezgjatja)
        video_clip = video_clip.with_audio(audio_clip)
        video_clip.write_videofile(OUTPUT_VIDEO, fps=24, codec="libx264", audio_codec="aac")
        print("Videoja u krijua me sukses!")
    except Exception as e:
        print(f"Gabim gjate krijimit te videos: {e}")

def autentiko_youtube():
    print("Duke u lidhur me YouTube...")
    flow = InstalledAppFlow.from_client_secrets_file("client_secret.json", SCOPES)
    credentials = flow.run_local_server(port=0)
    return build("youtube", "v3", credentials=credentials)

def ngarko_ne_youtube(youtube):
    print("Duke ngarkuar videon ne YouTube...")
    body = {
        "snippet": {
            "title": VIDEO_TITLE,
            "description": VIDEO_DESCRIPTION,
            "tags": VIDEO_TAGS,
            "categoryId": "10"
        },
        "status": {
            "privacyStatus": "public"
        }
    }
    media = MediaFileUpload(OUTPUT_VIDEO, chunksize=-1, resumable=True, mimetype="video/mp4")
    request = youtube.videos().insert(part="snippet,status", body=body, media_body=media)
    response = None
    while response is None:
        status, response = request.next_chunk()
        if status:
            print(f"Perparimi: {int(status.progress() * 100)}%")
    print(f"Videoja u postua me sukses. URL: https://youtu.be/{response['id']}")

if __name__ == "__main__":
    krijo_videon()
    youtube_client = autentiko_youtube()
    ngarko_ne_youtube(youtube_client)
