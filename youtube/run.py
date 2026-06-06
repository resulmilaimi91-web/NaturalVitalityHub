#!/usr/bin/env python3
import os, sys, json, random, datetime, time

sys.path.insert(0, os.path.dirname(__file__))
os.chdir(os.path.dirname(__file__))

from video_creator import create_video_from_script, upload_to_youtube

STATE_FILE = os.path.join(os.path.dirname(__file__), "auto_state.json")
LOG_FILE = os.path.join(os.path.dirname(__file__), "upload_log.json")

def generate_all(topic, index):
    titles = [
        f"{topic} - What Nobody Tells You in 2026",
        f"Why {topic} is Changing Everything in 2026",
        f"{topic} Explained Simply - Complete Guide",
        f"Top 5 {topic} Secrets You Must Know",
        f"The Truth About {topic} Nobody Talks About",
        f"How to Master {topic} in 2026",
        f"{topic}: Everything You Need to Know",
        f"{topic} Tips That Actually Work",
        f"Stop Doing This Wrong - {topic} Guide",
        f"The Future of {topic} in 2026 and Beyond"
    ]

    descriptions = [
        f"Complete guide to {topic} in 2026. Learn the best strategies, tips, and tools that are working right now. This video covers everything from basics to advanced techniques.",
        f"Everything you need to know about {topic} explained in simple terms. Updated for 2026 with the latest information and proven strategies that deliver real results.",
        f"Want to master {topic}? This comprehensive guide walks you through everything you need to know. Perfect for beginners and experts alike.",
        f"The ultimate {topic} guide for 2026. We share insider tips, common mistakes to avoid, and best practices that work. Watch now and transform your skills.",
        f"Discover the secrets of {topic} that experts use. This video breaks down complex concepts into easy to understand steps anyone can follow."
    ]

    hashtag_sets = [
        f"#{topic.replace(' ','')} #2026 #Guide #Tips #Learn",
        f"#{topic.replace(' ','')} #Tutorial #HowTo #Beginner #2026",
        f"#{topic.replace(' ','')} #Secrets #Master #Complete #2026",
        f"#{topic.replace(' ','')} #Best #Top #Essential #2026",
        f"#{topic.replace(' ','')} #Future #Innovation #Trends #2026"
    ]

    section_sets = [
        [
            {"text": f"Welcome! Today we explore {topic} in depth"},
            {"text": f"The basics of {topic} everyone should know"},
            {"text": f"Common mistakes people make with {topic}"},
            {"text": f"Best strategies for {topic} that work in 2026"},
            {"text": f"Subscribe and share your experience with {topic}"}
        ],
        [
            {"text": f"Why is {topic} so important in 2026? Lets find out"},
            {"text": f"The evolution of {topic} leading to today"},
            {"text": f"How {topic} is changing industries worldwide"},
            {"text": f"Practical steps to get started with {topic}"},
            {"text": f"Like this video if you found it helpful about {topic}"}
        ],
        [
            {"text": f"Everyone talks about {topic} but what is the truth?"},
            {"text": f"The science behind {topic} explained simply"},
            {"text": f"Real world examples of {topic} success"},
            {"text": f"Step by step guide to {topic} for beginners"},
            {"text": f"Comment your biggest question about {topic}"}
        ],
        [
            {"text": f"What makes {topic} special in 2026?"},
            {"text": f"Expert insights on {topic} you need to hear"},
            {"text": f"The tools and resources for {topic}"},
            {"text": f"Advanced {topic} techniques for pros"},
            {"text": f"Subscribe for more {topic} content weekly"}
        ],
        [
            {"text": f"The future of {topic} looks incredible"},
            {"text": f"Myths about {topic} that need to be broken"},
            {"text": f"Predictions for {topic} in the coming years"},
            {"text": f"How to avoid {topic} pitfalls and mistakes"},
            {"text": f"Share this with someone who needs to learn {topic}"}
        ]
    ]

    i = index % len(titles)
    return {
        "title": titles[i],
        "desc": descriptions[i],
        "hashtags": hashtag_sets[i],
        "sections": section_sets[i]
    }

def load_state():
    if os.path.exists(STATE_FILE):
        with open(STATE_FILE, "r") as f:
            return json.load(f)
    return {"last_index": 0, "total_posted": 0, "last_run": None}

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
    print("=" * 50)
    print("  YouTube Auto Creator")
    print("=" * 50)

    topic = input("\nTema: ").strip()
    if not topic:
        print("Nuk ka teme. Mbyllet.")
        return

    count_str = input("Sa video: ").strip()
    try:
        count = int(count_str)
    except:
        count = 1

    state = load_state()
    start = state["last_index"]

    print(f"\nTema: {topic}")
    print(f"Video: {count}")
    print(f"Fillet nga: #{start + 1}")
    print()

    for i in range(count):
        idx = start + i
        content = generate_all(topic, idx)

        print(f"[{i+1}/{count}] {content['title']}")

        video_path = create_video_from_script(
            title=content["title"],
            sections=content["sections"],
            desc=content["desc"],
            hashtags=content["hashtags"],
            lang="en"
        )

        url = upload_to_youtube(
            video_path, content["title"],
            f"{content['desc']}\n\n{content['hashtags']}",
            content["hashtags"].split(), "public"
        )

        log_upload(topic, content["title"], url)
        state["last_index"] = idx + 1
        state["total_posted"] = state.get("total_posted", 0) + 1
        state["last_run"] = datetime.datetime.now().isoformat()
        state["last_url"] = url
        save_state(state)

        print(f"  -> {url}")

        if i < count - 1:
            next_time = (datetime.datetime.now() + datetime.timedelta(hours=1)).strftime("%H:%M")
            print(f"  -> Prit 1 ore... (radha tjetere: {next_time})")
            time.sleep(3600)

    print(f"\n{'='*50}")
    print(f"  KRYER! {count} video te publikuara!")
    print(f"  Total: {state['total_posted']} video")
    print(f"{'='*50}")

if __name__ == "__main__":
    main()
