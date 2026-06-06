#!/usr/bin/env python3
import os, sys, json, random, datetime

sys.path.insert(0, os.path.dirname(__file__))
os.chdir(os.path.dirname(__file__))

from video_creator import create_video_from_script, upload_to_youtube

TOPICS_FILE = os.path.join(os.path.dirname(__file__), "topics.txt")
STATE_FILE = os.path.join(os.path.dirname(__file__), "auto_state.json")
LOG_FILE = os.path.join(os.path.dirname(__file__), "upload_log.json")

AUTO_SCRIPTS = {
    "AI Tools and Technology": [
        {"text": "Welcome to the AI revolution of 2026"},
        {"text": "ChatGPT and Claude are changing how we work"},
        {"text": "Runway creates stunning videos from text prompts"},
        {"text": "GitHub Copilot makes coding 55 percent faster"},
        {"text": "Subscribe for daily AI content and updates"}
    ],
    "How to Make Money Online 2026": [
        {"text": "Want to make money online in 2026? Watch this"},
        {"text": "Freelancing on Upwork and Fiverr is booming"},
        {"text": "YouTube and TikTok offer passive income streams"},
        {"text": "Digital products sell while you sleep"},
        {"text": "Start your online income journey today"}
    ],
    "Best Free Apps for Productivity": [
        {"text": "These free apps will boost your productivity 10x"},
        {"text": "Notion organizes your entire life for free"},
        {"text": "Todoist keeps you on track with smart reminders"},
        {"text": "Canva AI creates professional designs instantly"},
        {"text": "Download these apps and transform your workflow"}
    ],
    "ChatGPT Tips and Tricks": [
        {"text": "Master ChatGPT with these insider secrets"},
        {"text": "Specific prompts get 10x better results"},
        {"text": "Chain prompts for complex task automation"},
        {"text": "Custom instructions personalize your experience"},
        {"text": "Become a ChatGPT power user today"}
    ],
    "How to Start a YouTube Channel": [
        {"text": "Starting a YouTube channel in 2026 is simple"},
        {"text": "Pick a niche and create consistent content"},
        {"text": "AI tools make thumbnails and scripts easy"},
        {"text": "Post weekly and engage with your audience"},
        {"text": "Start your YouTube journey right now"}
    ],
    "Passive Income Ideas with AI": [
        {"text": "AI is creating new passive income opportunities"},
        {"text": "Sell AI generated content on marketplaces"},
        {"text": "Build automated businesses with chatbots"},
        {"text": "Create digital products with AI assistance"},
        {"text": "Build your passive income empire today"}
    ],
    "Top 5 Gadgets You Need": [
        {"text": "These gadgets will change your life in 2026"},
        {"text": "Smart rings track your health 24/7"},
        {"text": "AI glasses translate languages instantly"},
        {"text": "Solar chargers give you unlimited power"},
        {"text": "Get these must have gadgets now"}
    ],
    "How to Learn Coding Fast": [
        {"text": "Learn coding 10x faster with AI tools"},
        {"text": "Python is the best language for beginners"},
        {"text": "GitHub Copilot writes code while you learn"},
        {"text": "Build projects instead of watching tutorials"},
        {"text": "Start coding and build your career today"}
    ],
    "Digital Marketing Strategy": [
        {"text": "Digital marketing in 2026 runs on AI"},
        {"text": "Create content 10x faster with AI tools"},
        {"text": "Automate your social media posting schedule"},
        {"text": "AI analytics reveal your best strategies"},
        {"text": "Implement these tactics and grow fast"}
    ],
    "Side Hustle Ideas for Beginners": [
        {"text": "Best side hustles for beginners in 2026"},
        {"text": "Faceless YouTube channels use AI content"},
        {"text": "Print on demand needs zero inventory"},
        {"text": "AI services sell on freelance platforms"},
        {"text": "Pick one and start making money today"}
    ]
}

def load_topics():
    if os.path.exists(TOPICS_FILE):
        with open(TOPICS_FILE, "r", encoding="utf-8") as f:
            topics = [line.strip() for line in f if line.strip()]
            if topics:
                return topics
    return list(AUTO_SCRIPTS.keys())

def get_script(topic):
    if topic in AUTO_SCRIPTS:
        return AUTO_SCRIPTS[topic]
    return [
        {"text": f"Welcome to our video about {topic}"},
        {"text": f"The most important things about {topic} in 2026"},
        {"text": f"Why {topic} matters more than ever right now"},
        {"text": f"Practical tips you can use with {topic} today"},
        {"text": f"Subscribe for more content about {topic}"}
    ]

def load_state():
    if os.path.exists(STATE_FILE):
        with open(STATE_FILE, "r") as f:
            return json.load(f)
    return {"last_index": 0, "last_run": None, "total_posted": 0}

def save_state(state):
    with open(STATE_FILE, "w") as f:
        json.dump(state, f, indent=2)

def log_upload(topic, title, url):
    logs = []
    if os.path.exists(LOG_FILE):
        with open(LOG_FILE, "r") as f:
            logs = json.load(f)
    logs.append({
        "topic": topic, "title": title, "url": url,
        "success": "youtu.be" in str(url),
        "timestamp": datetime.datetime.now().isoformat()
    })
    with open(LOG_FILE, "w") as f:
        json.dump(logs, f, indent=2)

def main():
    topics = load_topics()
    state = load_state()

    idx = state["last_index"] % len(topics)
    topic = topics[idx]
    sections = get_script(topic)

    title_templates = [
        f"{topic} - What Nobody Tells You",
        f"Why {topic} is Changing Everything",
        f"{topic} Explained Simply",
        f"Top Tips for {topic} in 2026",
        f"The Truth About {topic}"
    ]
    title = random.choice(title_templates)

    desc = f"Complete guide to {topic} in 2026. Learn the best strategies and tips that work."
    hashtags = f"#{topic.replace(' ','')} #2026 #Guide #Tips #Learn"

    print(f"Topic: {topic}")
    print(f"Title: {title}")

    video_path = create_video_from_script(title, sections, desc, hashtags, lang="en")
    print(f"Video: {video_path}")

    url = upload_to_youtube(video_path, title, f"{desc}\n\n{hashtags}", hashtags.split(), "public")
    log_upload(topic, title, url)

    state["last_index"] = idx + 1
    state["total_posted"] = state.get("total_posted", 0) + 1
    state["last_run"] = datetime.datetime.now().isoformat()
    state["last_url"] = url
    save_state(state)

    print(f"Published: {url}")
    print(f"Total posted: {state['total_posted']}")

if __name__ == "__main__":
    main()
