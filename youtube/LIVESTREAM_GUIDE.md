# YouTube 24/7 Livestream Guide

## Step 1: Create Video
1. Open `LivestreamCreator.bat`
2. Enter title and duration (hours)
3. Click "CREATE VIDEO"
4. Wait for video to be created

## Step 2: Setup OBS Studio
1. Download OBS Studio from https://obsproject.com
2. Open OBS Studio
3. Go to Settings -> Stream
4. Select "YouTube" as service
5. Enter your Stream Key from YouTube Studio

## Step 3: Add Video Source
1. In OBS, click "+" under Sources
2. Select "Media Source"
3. Click "Browse" and select your video
4. Check "Loop" option
5. Click OK

## Step 4: Start Streaming
1. Click "Start Streaming" in OBS
2. Video will loop automatically
3. Stream runs 24/7 until you stop it

## OBS Settings Recommended:
- Video Bitrate: 4500 Kbps
- Encoder: x264
- Audio Bitrate: 160 kHz
- Keyframe Interval: 2 seconds
- Preset: performance
- Profile: high

## To Keep Stream Active:
- Use a VPS or cloud server
- Install OBS + your video
- Start stream and leave running
- Monitor via YouTube Studio

## Stream Key Location:
YouTube Studio -> Go Live -> Stream -> Stream Key
