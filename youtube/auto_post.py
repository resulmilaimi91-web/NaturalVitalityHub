#!/usr/bin/env python3
import os, sys, json, random, datetime

sys.path.insert(0, os.path.dirname(__file__))
os.chdir(os.path.dirname(__file__))

from video_creator import create_video_from_script, upload_to_youtube

TOPICS_FILE = os.path.join(os.path.dirname(__file__), "topics.txt")
LOG_FILE = os.path.join(os.path.dirname(__file__), "upload_log.json")
STATE_FILE = os.path.join(os.path.dirname(__file__), "state.json")

SCRIPTS = {
    "AI Tools and Technology": {
        "sections": [
            {"text": "Welcome to the top AI tools transforming our world in 2026"},
            {"text": "ChatGPT and Claude lead the revolution in writing and analysis"},
            {"text": "Runway and Midjourney dominate video and image generation"},
            {"text": "GitHub Copilot boosts developer productivity by 55 percent"},
            {"text": "AI is more accessible than ever. Subscribe for more!"}
        ],
        "desc": "Discover the top AI tools that are changing how we work and create in 2026",
        "hashtags": "#AI2026 #AITools #Technology #Innovation #Productivity"
    },
    "How to Make Money Online 2026": {
        "sections": [
            {"text": "Want to make money online in 2026? Here are the best ways"},
            {"text": "Start with freelancing on platforms like Upwork and Fiverr"},
            {"text": "Create content on YouTube and TikTok for passive income"},
            {"text": "Sell digital products like courses and templates"},
            {"text": "Use AI tools to automate your income streams"}
        ],
        "desc": "Learn the best ways to make money online in 2026 with these proven methods",
        "hashtags": "#MakeMoneyOnline #PassiveIncome #SideHustle #2026 #OnlineIncome"
    },
    "Best Free Apps for Productivity": {
        "sections": [
            {"text": "Here are the best free productivity apps you need in 2026"},
            {"text": "Notion for project management and notes organization"},
            {"text": "Todoist for task management and daily planning"},
            {"text": "Canva AI for professional design without skills"},
            {"text": "Download these apps and boost your productivity today"}
        ],
        "desc": "The best free productivity apps that will transform your workflow in 2026",
        "hashtags": "#Productivity #FreeApps #Apps2026 #Workflow #Efficiency"
    },
    "ChatGPT Tips and Tricks": {
        "sections": [
            {"text": "Master ChatGPT with these powerful tips and tricks"},
            {"text": "Use specific prompts to get better results every time"},
            {"text": "Chain multiple prompts for complex tasks and analysis"},
            {"text": "Create custom instructions for personalized responses"},
            {"text": "Start using these tricks and become a ChatGPT pro"}
        ],
        "desc": "Advanced ChatGPT tips and tricks that most users dont know about",
        "hashtags": "#ChatGPT #AITips #Productivity #LearnAI #TechTips"
    },
    "How to Start a YouTube Channel": {
        "sections": [
            {"text": "Starting a YouTube channel in 2026 is easier than ever"},
            {"text": "Pick a niche you are passionate about and stick to it"},
            {"text": "Use AI tools to create thumbnails and scripts automatically"},
            {"text": "Consistency is key - post at least once a week"},
            {"text": "Start your YouTube journey today and build your audience"}
        ],
        "desc": "Complete guide to starting a successful YouTube channel in 2026",
        "hashtags": "#YouTube #YouTubeChannel #ContentCreator #YouTuber2026"
    },
    "Passive Income Ideas with AI": {
        "sections": [
            {"text": "AI is creating amazing passive income opportunities in 2026"},
            {"text": "Create and sell AI-generated content like images and videos"},
            {"text": "Build automated businesses with AI chatbots and agents"},
            {"text": "Sell digital products created with AI assistance"},
            {"text": "Start building your passive income empire with AI today"}
        ],
        "desc": "How to use AI to create multiple streams of passive income in 2026",
        "hashtags": "#PassiveIncome #AI #MakeMoneyOnline #Automation #2026"
    },
    "Top 5 Gadgets You Need": {
        "sections": [
            {"text": "These top 5 gadgets will change your life in 2026"},
            {"text": "Smart rings that track your health 24/7"},
            {"text": "AI glasses that translate languages in real time"},
            {"text": "Portable solar chargers for unlimited power"},
            {"text": "Get these gadgets and upgrade your lifestyle"}
        ],
        "desc": "The must-have tech gadgets that everyone is talking about in 2026",
        "hashtags": "#Gadgets #Tech #Innovation #MustHave #2026"
    },
    "How to Learn Coding Fast": {
        "sections": [
            {"text": "Learning to code in 2026 is faster than ever with AI"},
            {"text": "Start with Python - the most beginner friendly language"},
            {"text": "Use GitHub Copilot to write code faster and learn patterns"},
            {"text": "Build real projects instead of just watching tutorials"},
            {"text": "Start coding today and build your tech career"}
        ],
        "desc": "The fastest way to learn coding in 2026 using AI tools and modern methods",
        "hashtags": "#Coding #Programming #LearnToCode #Python #TechCareer"
    },
    "Digital Marketing Strategy": {
        "sections": [
            {"text": "Digital marketing in 2026 is all about AI and automation"},
            {"text": "Use AI to create content 10x faster than before"},
            {"text": "Automate social media posting with smart scheduling tools"},
            {"text": "Analyze your data with AI powered analytics tools"},
            {"text": "Implement these strategies and grow your business today"}
        ],
        "desc": "The ultimate digital marketing strategy guide for 2026",
        "hashtags": "#DigitalMarketing #Marketing #AI #Business #Growth"
    },
    "Side Hustle Ideas for Beginners": {
        "sections": [
            {"text": "Here are the best side hustle ideas for beginners in 2026"},
            {"text": "Start a faceless YouTube channel with AI generated content"},
            {"text": "Sell print on demand products with no inventory needed"},
            {"text": "Offer AI powered services on freelance platforms"},
            {"text": "Pick one idea and start making money this week"}
        ],
        "desc": "Easy side hustle ideas that anyone can start in 2026 with little investment",
        "hashtags": "#SideHustle #MakeMoney #Beginners #ExtraIncome #2026"
    }
}

def load_topics():
    if not os.path.exists(TOPICS_FILE):
        return list(SCRIPTS.keys())
    with open(TOPICS_FILE, "r", encoding="utf-8") as f:
        return [line.strip() for line in f if line.strip()]

def load_state():
    if os.path.exists(STATE_FILE):
        with open(STATE_FILE, "r") as f:
            return json.load(f)
    return {"last_index": 0, "last_run": None}

def save_state(state):
    with open(STATE_FILE, "w") as f:
        json.dump(state, f)

def log_upload(topic, url, success):
    logs = []
    if os.path.exists(LOG_FILE):
        with open(LOG_FILE, "r") as f:
            logs = json.load(f)
    logs.append({
        "topic": topic,
        "url": url,
        "success": success,
        "timestamp": datetime.datetime.now().isoformat()
    })
    with open(LOG_FILE, "w") as f:
        json.dump(logs, f, indent=2)

def generate_script(topic):
    if topic in SCRIPTS:
        return SCRIPTS[topic]
    words = topic.split()
    return {
        "sections": [
            {"text": f"Welcome to our video about {topic}"},
            {"text": f"Today we explore the most important aspects of {topic}"},
            {"text": f"Key insights that will change how you think about {topic}"},
            {"text": f"Practical tips you can apply immediately to {topic}"},
            {"text": f"Subscribe for more content about {topic} and share with friends"}
        ],
        "desc": f"Complete guide to {topic} in 2026 - tips, strategies, and insights",
        "hashtags": f"#{topic.replace(' ','')} #2026 #Tips #Guide #Learning"
    }

def main():
    print("=" * 55)
    print("  YouTube Auto Creator - Daily Video Generator")
    print("=" * 55)

    topics = load_topics()
    state = load_state()
    next_index = state["last_index"] % len(topics)
    topic = topics[next_index]

    print(f"Today's topic: {topic}")
    script = generate_script(topic)

    print("Creating video...")
    video_path = create_video_from_script(
        title=topic,
        sections=script["sections"],
        desc=script["desc"],
        hashtags=script["hashtags"],
        lang="en"
    )
    print(f"Video created: {video_path}")

    print("Uploading to YouTube...")
    url = upload_to_youtube(
        video_path, topic,
        f"{script['desc']}\n\n{script['hashtags']}",
        script["hashtags"].split(),
        "public"
    )

    success = url and "youtu.be" in str(url)
    log_upload(topic, url, success)

    state["last_index"] = next_index + 1
    state["last_run"] = datetime.datetime.now().isoformat()
    save_state(state)

    if success:
        print(f"\nPUBLISHED: {url}")
    else:
        print(f"\nUpload failed. Check logs.")

    return url

if __name__ == "__main__":
    main()
