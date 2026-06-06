#!/usr/bin/env python3
import os, sys, random, tempfile
from PIL import Image, ImageDraw, ImageFont, ImageEnhance
from gtts import gTTS
from moviepy import AudioFileClip, ImageClip
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from googleapiclient.http import MediaFileUpload

SCOPES = ["https://www.googleapis.com/auth/youtube.upload"]

def get_font(size):
    paths = ["arial.ttf", "C:/Windows/Fonts/arial.ttf", "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"]
    for p in paths:
        try: return ImageFont.truetype(p, size)
        except: pass
    return ImageFont.load_default()

def krijo_imazh(titulli, size=(1920, 1080)):
    bg = Image.new("RGB", size, (10, 10, 30))
    draw = ImageDraw.Draw(bg)
    for y in range(size[1]):
        ratio = y / size[1]
        r = int(10 + ratio * 40)
        g = int(10 + ratio * 10)
        b = int(30 + ratio * 80)
        draw.line([(0, y), (size[0], y)], fill=(r, g, b))

    font = get_font(80)
    words = titulli.split()
    lines, current = [], ""
    for w in words:
        if len(current + " " + w) <= 25:
            current += " " + w if current else w
        else:
            lines.append(current)
            current = w
    if current: lines.append(current)

    total_h = len(lines) * 100
    start_y = (size[1] - total_h) // 2

    for i, line in enumerate(lines):
        bbox = draw.textbbox((0, 0), line, font=font)
        lw = bbox[2] - bbox[0]
        x = (size[0] - lw) // 2
        y = start_y + i * 100
        draw.text((x+3, y+3), line, fill=(0, 0, 0), font=font)
        draw.text((x, y), line, fill=(255, 255, 255), font=font)

    for j in range(5):
        dx = size[0] // 2 - 100 + j * 50
        draw.ellipse([dx, start_y + len(lines) * 100 + 40, dx + 30, start_y + len(lines) * 100 + 70], fill=(0, 200, 255))

    path = os.path.join(tempfile.gettempdir(), "generated_bg.jpg")
    bg.save(path, quality=95)
    return path

def krijo_audio(texti, lang="en"):
    tts = gTTS(text=texti, lang=lang)
    path = os.path.join(tempfile.gettempdir(), "generated_audio.mp3")
    tts.save(path)
    return path

def krijo_videon(audio_path, image_path, output_path):
    audio = AudioFileClip(audio_path)
    video = ImageClip(image_path).with_duration(audio.duration)
    video = video.with_audio(audio)
    video.write_videofile(output_path, fps=24, codec="libx264", audio_codec="aac")
    return output_path

def autentiko():
    flow = InstalledAppFlow.from_client_secrets_file(
        os.path.join(os.path.dirname(__file__), "client_secret.json"), SCOPES
    )
    creds = flow.run_local_server(port=0)
    return build("youtube", "v3", credentials=creds)

def ngarko(youtube, video_path, title, desc, tags):
    body = {
        "snippet": {"title": title, "description": desc, "tags": tags, "categoryId": "22"},
        "status": {"privacyStatus": "public"}
    }
    media = MediaFileUpload(video_path, chunksize=-1, resumable=True, mimetype="video/mp4")
    request = youtube.videos().insert(part="snippet,status", body=body, media_body=media)
    response = None
    while response is None:
        status, response = request.next_chunk()
        if status:
            print(f"  Upload: {int(status.progress() * 100)}%")
    return f"https://youtu.be/{response['id']}"

if __name__ == "__main__":
    print("=" * 50)
    print("  YouTube Auto - Krijo dhe Posto Automatic")
    print("=" * 50)

    titulli = input("\nTitulli i videos: ").strip()
    if not titulli:
        titulli = "AI Tools That Will Change Your Life"

    texti = input("Teksti per audio (ose Enter per default): ").strip()
    if not texti:
        texti = f"Welcome to our video about {titulli}. This is the most important guide you will watch today. Subscribe for more content!"

    lang = input("Gjuha (en/sq): ").strip() or "en"

    print("\n1. Duke krijuar imazhin...")
    image_path = krijo_imazh(titulli)
    print(f"  Imazhi: {image_path}")

    print("2. Duke krijuar audio...")
    audio_path = krijo_audio(texti, lang)
    print(f"  Audio: {audio_path}")

    print("3. Duke krijuar videon...")
    output_path = os.path.join(tempfile.gettempdir(), "final_video.mp4")
    krijo_videon(audio_path, image_path, output_path)
    print(f"  Video: {output_path}")

    upload = input("\nTa ngarkosh ne YouTube? (y/n): ").strip().lower()
    if upload == "y":
        print("4. Duke u lidhur me YouTube...")
        youtube = autentiko()
        print("5. Duke ngarkuar...")
        desc = f"{titulli}\n\nSubscribe for more!"
        tags = titulli.split() + ["YouTube", "2026"]
        url = ngarko(youtube, output_path, titulli, desc, tags)
        print(f"\nPUBLISHED: {url}")
    else:
        print(f"\nVideo gati: {output_path}")
