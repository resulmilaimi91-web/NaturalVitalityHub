#!/usr/bin/env python3
import os, sys, json, random, datetime, time, threading
import tkinter as tk
from tkinter import ttk, messagebox, scrolledtext

sys.path.insert(0, os.path.dirname(__file__))
os.chdir(os.path.dirname(__file__))

from video_creator import create_video_from_script, create_music_video, upload_to_youtube

STATE_FILE = os.path.join(os.path.dirname(__file__), "app_state.json")
LOG_FILE = os.path.join(os.path.dirname(__file__), "upload_log.json")
CONFIG_FILE = os.path.join(os.path.dirname(__file__), "config.json")

SCRIPTS = {
    "AI Tools": [
        {"text": "Welcome to the best AI tools of 2026"},
        {"text": "ChatGPT leads the AI revolution"},
        {"text": "Runway creates stunning videos from text"},
        {"text": "GitHub Copilot makes coding faster"},
        {"text": "Subscribe for more AI content"}
    ],
    "Make Money Online": [
        {"text": "Want to make money online in 2026?"},
        {"text": "Freelancing on Upwork is booming"},
        {"text": "YouTube offers passive income streams"},
        {"text": "Digital products sell while you sleep"},
        {"text": "Start your online income journey today"}
    ],
    "Coding Tutorial": [
        {"text": "Learn coding 10x faster with AI"},
        {"text": "Python is the best language for beginners"},
        {"text": "GitHub Copilot writes code while you learn"},
        {"text": "Build projects instead of watching tutorials"},
        {"text": "Start coding and build your career today"}
    ],
    "Tech Gadgets": [
        {"text": "These gadgets will change your life"},
        {"text": "Smart rings track your health 24/7"},
        {"text": "AI glasses translate languages instantly"},
        {"text": "Solar chargers give you unlimited power"},
        {"text": "Get these must have gadgets now"}
    ],
    "Productivity Tips": [
        {"text": "Boost your productivity 10x with these tips"},
        {"text": "Notion organizes your entire life for free"},
        {"text": "Todoist keeps you on track with reminders"},
        {"text": "Canva AI creates professional designs"},
        {"text": "Transform your workflow today"}
    ]
}

class YouTubeApp:
    def __init__(self):
        self.root = tk.Tk()
        self.root.title("YouTube Auto Creator")
        self.root.geometry("900x680")
        self.root.configure(bg="#0a0a1a")
        self.running = False
        self.config = self.load_config()
        self.create_ui()

    def load_config(self):
        if os.path.exists(CONFIG_FILE):
            with open(CONFIG_FILE, "r") as f:
                return json.load(f)
        return {"default_lang": "en", "default_privacy": "public"}

    def save_config(self):
        with open(CONFIG_FILE, "w") as f:
            json.dump(self.config, f, indent=2)

    def create_ui(self):
        title_frame = tk.Frame(self.root, bg="#0f1a30", height=60)
        title_frame.pack(fill="x")
        title_frame.pack_propagate(False)
        tk.Label(title_frame, text="YouTube Auto Creator", font=("Arial", 22, "bold"),
                fg="#00d4ff", bg="#0f1a30").pack(pady=15)

        main_frame = tk.Frame(self.root, bg="#0a0a1a")
        main_frame.pack(fill="both", expand=True, padx=15, pady=10)

        left_frame = tk.Frame(main_frame, bg="#0a0a1a", width=400)
        left_frame.pack(side="left", fill="both", expand=True, padx=(0, 10))

        tk.Label(left_frame, text="Video Settings", font=("Arial", 14, "bold"),
                fg="#ffffff", bg="#0a0a1a").pack(anchor="w", pady=(0, 10))

        tk.Label(left_frame, text="Topic:", fg="#aaaaaa", bg="#0a0a1a").pack(anchor="w")
        self.topic_var = tk.StringVar(value="AI Tools")
        topic_menu = ttk.Combobox(left_frame, textvariable=self.topic_var,
                                 values=list(SCRIPTS.keys()), width=40)
        topic_menu.pack(fill="x", pady=(0, 10))

        tk.Label(left_frame, text="Custom Topic:", fg="#aaaaaa", bg="#0a0a1a").pack(anchor="w")
        self.custom_topic = tk.Entry(left_frame, width=42, font=("Arial", 11))
        self.custom_topic.pack(fill="x", pady=(0, 10))

        tk.Label(left_frame, text="Number of Videos:", fg="#aaaaaa", bg="#0a0a1a").pack(anchor="w")
        self.count_var = tk.StringVar(value="1")
        count_spin = tk.Spinbox(left_frame, from_=1, to=100, textvariable=self.count_var, width=10)
        count_spin.pack(anchor="w", pady=(0, 10))

        tk.Label(left_frame, text="Language:", fg="#aaaaaa", bg="#0a0a1a").pack(anchor="w")
        self.lang_var = tk.StringVar(value="en")
        lang_frame = tk.Frame(left_frame, bg="#0a0a1a")
        lang_frame.pack(anchor="w", pady=(0, 10))
        tk.Radiobutton(lang_frame, text="English", variable=self.lang_var, value="en",
                       bg="#0a0a1a", fg="#ffffff", selectcolor="#16213e").pack(side="left")
        tk.Radiobutton(lang_frame, text="Shqip", variable=self.lang_var, value="sq",
                       bg="#0a0a1a", fg="#ffffff", selectcolor="#16213e").pack(side="left", padx=10)

        tk.Label(left_frame, text="Video Mode:", fg="#aaaaaa", bg="#0a0a1a").pack(anchor="w")
        self.mode_var = tk.StringVar(value="normal")
        mode_frame = tk.Frame(left_frame, bg="#0a0a1a")
        mode_frame.pack(anchor="w", pady=(0, 10))
        tk.Radiobutton(mode_frame, text="Normal (TTS + Music)", variable=self.mode_var, value="normal",
                       bg="#0a0a1a", fg="#ffffff", selectcolor="#16213e").pack(side="left")
        tk.Radiobutton(mode_frame, text="Music Only (40min)", variable=self.mode_var, value="music_only",
                       bg="#0a0a1a", fg="#00ff88", selectcolor="#16213e").pack(side="left", padx=10)

        btn_frame = tk.Frame(left_frame, bg="#0a0a1a")
        btn_frame.pack(fill="x", pady=15)

        self.start_btn = tk.Button(btn_frame, text="START POSTING", command=self.start_posting,
                                  bg="#00d4ff", fg="#000000", font=("Arial", 12, "bold"),
                                  width=18, cursor="hand2")
        self.start_btn.pack(side="left")

        self.stop_btn = tk.Button(btn_frame, text="STOP", command=self.stop_posting,
                                 bg="#ff4444", fg="#ffffff", font=("Arial", 12, "bold"),
                                 width=10, state="disabled", cursor="hand2")
        self.stop_btn.pack(side="left", padx=10)

        info_frame = tk.Frame(left_frame, bg="#0f1a30", relief="raised", bd=1)
        info_frame.pack(fill="x", pady=10)
        tk.Label(info_frame, text="How it works:", font=("Arial", 10, "bold"),
                fg="#00d4ff", bg="#0f1a30").pack(anchor="w", padx=10, pady=(5,0))
        info_text = "1. Select topic or write custom\n2. Enable background music\n3. Click START - auto creates + uploads!"
        tk.Label(info_frame, text=info_text, fg="#aaaaaa", bg="#0f1a30",
                justify="left", font=("Arial", 9)).pack(anchor="w", padx=10, pady=5)

        right_frame = tk.Frame(main_frame, bg="#0a0a1a", width=450)
        right_frame.pack(side="right", fill="both", expand=True)

        tk.Label(right_frame, text="Activity Log", font=("Arial", 14, "bold"),
                fg="#ffffff", bg="#0a0a1a").pack(anchor="w", pady=(0, 10))

        self.log_text = scrolledtext.ScrolledText(right_frame, height=28, width=55,
                                                  bg="#0a1628", fg="#00ff88",
                                                  font=("Consolas", 10))
        self.log_text.pack(fill="both", expand=True)

        status_frame = tk.Frame(self.root, bg="#0f1a30", height=40)
        status_frame.pack(fill="x", side="bottom")
        status_frame.pack_propagate(False)
        self.status_label = tk.Label(status_frame, text="Ready - Select topic and click START",
                                    fg="#00d4ff", bg="#0f1a30", font=("Arial", 10))
        self.status_label.pack(pady=10)

    def log(self, msg):
        timestamp = datetime.datetime.now().strftime("%H:%M:%S")
        self.log_text.insert("end", f"[{timestamp}] {msg}\n")
        self.log_text.see("end")
        self.root.update_idletasks()

    def start_posting(self):
        topic = self.custom_topic.get().strip() or self.topic_var.get()
        try:
            count = int(self.count_var.get())
        except:
            count = 1
        lang = self.lang_var.get()
        mode = self.mode_var.get()

        self.running = True
        self.start_btn.config(state="disabled")
        self.stop_btn.config(state="normal")
        self.status_label.config(text=f"Running: {count} videos...")

        thread = threading.Thread(target=self.post_videos, args=(topic, count, lang, mode))
        thread.daemon = True
        thread.start()

    def stop_posting(self):
        self.running = False
        self.start_btn.config(state="normal")
        self.stop_btn.config(state="disabled")
        self.status_label.config(text="Stopped")
        self.log("STOPPED by user")

    def post_videos(self, topic, count, lang, mode):
        sections = SCRIPTS.get(topic, [
            {"text": f"Welcome to our video about {topic}"},
            {"text": f"The most important things about {topic} in 2026"},
            {"text": f"Why {topic} matters more than ever"},
            {"text": f"Practical tips you can use with {topic} today"},
            {"text": f"Subscribe for more content about {topic}"}
        ])

        for i in range(count):
            if not self.running:
                break

            title_templates = [
                f"{topic} - What Nobody Tells You",
                f"Why {topic} is Changing Everything",
                f"{topic} Explained Simply",
                f"Top Tips for {topic} in 2026",
                f"The Truth About {topic}"
            ]
            title = random.choice(title_templates)
            desc = f"Complete guide to {topic} in 2026"
            hashtags = f"#{topic.replace(' ','')} #2026 #Guide #Tips"

            self.log(f"[{i+1}/{count}] Creating: {title}")

            try:
                self.status_label.config(text=f"Creating video {i+1}/{count}...")

                if mode == "music_only":
                    video_path = create_music_video(title, duration_minutes=40)
                else:
                    video_path = create_video_from_script(title, sections, desc, hashtags, lang=lang, use_music=True)

                self.log(f"  Video created!")

                self.status_label.config(text=f"Uploading {i+1}/{count}...")
                url = upload_to_youtube(video_path, title, f"{desc}\n\n{hashtags}",
                                       hashtags.split(), "public")

                self.log(f"  PUBLISHED: {url}")
                self.log_upload(topic, title, url)

            except Exception as e:
                self.log(f"  ERROR: {str(e)}")

            if i < count - 1 and self.running:
                self.log(f"  Waiting 1 hour...")
                self.status_label.config(text=f"Waiting 1 hour... Next: {i+2}/{count}")
                for _ in range(3600):
                    if not self.running:
                        break
                    time.sleep(1)

        self.running = False
        self.start_btn.config(state="normal")
        self.stop_btn.config(state="disabled")
        self.status_label.config(text="Completed!")
        self.log(f"DONE! {count} videos processed")

    def log_upload(self, topic, title, url):
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

    def run(self):
        self.root.mainloop()

if __name__ == "__main__":
    app = YouTubeApp()
    app.run()
