#!/usr/bin/env python3
import os, sys, json, random, datetime, time

sys.path.insert(0, os.path.dirname(__file__))
os.chdir(os.path.dirname(__file__))

from video_creator import create_video_from_script, upload_to_youtube

STATE_FILE = os.path.join(os.path.dirname(__file__), "master_state.json")
LOG_FILE = os.path.join(os.path.dirname(__file__), "upload_log.json")

def generate_variations(topic, index):
    titles = [
        f"{topic} - What Nobody Tells You in 2026",
        f"Why {topic} is Changing Everything in 2026",
        f"{topic}: The Complete Guide for Beginners",
        f"Top 5 {topic} Secrets You Need to Know",
        f"{topic} Explained in 5 Minutes - 2026 Edition",
        f"How to Master {topic} in 2026",
        f"{topic} Tips That Actually Work",
        f"The Truth About {topic} Nobody Talks About",
        f"{topic}: Everything You Need to Know",
        f"Stop Doing This Wrong - {topic} Guide 2026"
    ]

    descriptions = [
        f"Discover everything about {topic} in this comprehensive guide. Learn the best strategies, tips, and tools that are working in 2026. Whether you are a beginner or expert, this video will help you master {topic} quickly and effectively.",
        f"This video covers the most important aspects of {topic} that you need to understand in 2026. From basics to advanced techniques, we break it all down in a simple and easy to follow format.",
        f"Want to learn about {topic}? This complete guide walks you through everything from start to finish. Updated for 2026 with the latest information and proven strategies that deliver results.",
        f"The ultimate guide to {topic} in 2026. We share insider tips, common mistakes to avoid, and the best practices that experts use. Perfect for anyone looking to get started or improve their skills.",
        f"Everything you need to know about {topic} explained in simple terms. This video is your go-to resource for understanding how {topic} works and how to apply it in your daily life."
    ]

    hashtag_sets = [
        f"#{topic.replace(' ','')} #2026 #Guide #Tips #Tutorial",
        f"#{topic.replace(' ','')} #Learn #HowTo #Beginner #2026",
        f"#{topic.replace(' ','')} #Secrets #Master #Complete #2026",
        f"#{topic.replace(' ','')} #Best #Top #Essential #2026",
        f"#{topic.replace(' ','')} #Truth #Facts #MustKnow #2026"
    ]

    section_templates = [
        [
            {"text": f"Welcome! Today we deep dive into {topic}"},
            {"text": f"The first thing you need to know about {topic} is the basics"},
            {"text": f"Here are the most common mistakes people make with {topic}"},
            {"text": f"The best strategies for {topic} that actually deliver results"},
            {"text": f"Subscribe and comment your experience with {topic} below"}
        ],
        [
            {"text": f"Is {topic} really as important as people say? Lets find out"},
            {"text": f"The history and evolution of {topic} leading to 2026"},
            {"text": f"Why experts predict {topic} will dominate in the coming years"},
            {"text": f"Practical applications of {topic} you can use today"},
            {"text": f"Like this video if you learned something about {topic}"}
        ],
        [
            {"text": f"Everyone is talking about {topic} but why? Lets explore"},
            {"text": f"The science behind {topic} explained in simple terms"},
            {"text": f"How {topic} is affecting different industries right now"},
            {"text": f"Step by step guide to getting started with {topic}"},
            {"text": f"Share this video with someone who needs to learn about {topic}"}
        ],
        [
            {"text": f"What makes {topic} so special in 2026? Here is the answer"},
            {"text": f"Real world examples of {topic} success stories"},
            {"text": f"The tools and resources you need for {topic}"},
            {"text": f"Advanced techniques for {topic} that pros use"},
            {"text": f"Subscribe for more videos about {topic} every week"}
        ],
        [
            {"text": f"Let me show you the truth about {topic} in 2026"},
            {"text": f"Common myths about {topic} that need to be debunked"},
            {"text": f"The future predictions for {topic} and what they mean"},
            {"text": f"How to avoid the biggest pitfalls in {topic}"},
            {"text": f"Drop a comment with your biggest question about {topic}"}
        ]
    ]

    i = index % len(titles)
    return {
        "title": titles[i],
        "desc": descriptions[i],
        "hashtags": hashtag_sets[i],
        "sections": section_templates[i]
    }

def load_state():
    if os.path.exists(STATE_FILE):
        with open(STATE_FILE, "r") as f:
            return json.load(f)
    return {"posted": 0, "last_time": None}

def save_state(state):
    with open(STATE_FILE, "w") as f:
        json.dump(state, f, indent=2)

def log_upload(topic, title, url, success):
    logs = []
    if os.path.exists(LOG_FILE):
        with open(LOG_FILE, "r") as f:
            logs = json.load(f)
    logs.append({
        "topic": topic,
        "title": title,
        "url": url,
        "success": success,
        "timestamp": datetime.datetime.now().isoformat()
    })
    with open(LOG_FILE, "w") as f:
        json.dump(logs, f, indent=2)

def post_one_video(topic, index):
    content = generate_variations(topic, index)
    print(f"\n--- Video {index+1} ---")
    print(f"Title: {content['title']}")
    print(f"Creating video...")

    video_path = create_video_from_script(
        title=content["title"],
        sections=content["sections"],
        desc=content["desc"],
        hashtags=content["hashtags"],
        lang="en"
    )
    print(f"Video: {video_path}")

    print(f"Uploading...")
    url = upload_to_youtube(
        video_path,
        content["title"],
        f"{content['desc']}\n\n{content['hashtags']}",
        content["hashtags"].split(),
        "public"
    )

    success = url and "youtu.be" in str(url)
    log_upload(topic, content["title"], url, success)

    if success:
        print(f"PUBLISHED: {url}")
    else:
        print("Upload failed")

    return success, url

def main():
    print("=" * 55)
    print("  YouTube Master Auto Poster")
    print("  Topic + Quantity = Full Automation")
    print("=" * 55)
    print()

    topic = input("Enter topic: ").strip()
    if not topic:
        print("No topic entered. Exiting.")
        return

    count = input("How many videos? ").strip()
    try:
        count = int(count)
    except ValueError:
        print("Invalid number. Setting to 1.")
        count = 1

    state = load_state()
    start_index = state["posted"]

    print(f"\nTopic: {topic}")
    print(f"Videos to post: {count}")
    print(f"Starting from index: {start_index + 1}")
    print(f"Auto interval: every 1 hour")
    print()

    for i in range(count):
        video_index = start_index + i
        print(f"\n{'='*55}")
        print(f"  Processing video {i+1} of {count}")
        print(f"{'='*55}")

        success, url = post_one_video(topic, video_index)

        state["posted"] = video_index + 1
        state["last_time"] = datetime.datetime.now().isoformat()
        state["last_topic"] = topic
        state["last_url"] = url
        save_state(state)

        if i < count - 1:
            print(f"\nWaiting 1 hour before next video...")
            print(f"Next video at: {(datetime.datetime.now() + datetime.timedelta(hours=1)).strftime('%H:%M')}")
            time.sleep(3600)

    print(f"\n{'='*55}")
    print(f"  ALL DONE! {count} videos posted!")
    print(f"  Total videos posted: {state['posted']}")
    print(f"{'='*55}")

if __name__ == "__main__":
    main()
