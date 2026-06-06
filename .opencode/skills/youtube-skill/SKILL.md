---
name: youtube-automation
description: Use when the user wants to post videos to YouTube, automate YouTube uploads, or connect creato.ai with YouTube. Handles daily video scheduling and YouTube API integration.
---

# YouTube Automation Skill

This skill helps you automate YouTube video posting using content from creato.ai.

## Setup Requirements

1. **YouTube API Key** - Get from Google Cloud Console
2. **OAuth 2.0 Credentials** - For uploading videos to your channel
3. **creato.ai API access** - For fetching generated videos

## Daily Posting Workflow

1. Fetch latest video from creato.ai
2. Prepare metadata (title, description, tags)
3. Upload to YouTube via API
4. Log the upload result

## YouTube Data API v3

- `POST /youtube/v3/videos` - Upload a video
- `GET /youtube/v3/channels` - Get channel info
- `GET /youtube/v3/videos?myRating=like` - Get liked videos
