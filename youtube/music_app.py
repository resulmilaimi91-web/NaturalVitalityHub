#!/usr/bin/env python3
import os, sys, json, random, datetime, time, threading
import tkinter as tk
from tkinter import ttk, scrolledtext

sys.path.insert(0, os.path.dirname(__file__))
os.chdir(os.path.dirname(__file__))

from video_creator import create_music_video, upload_to_youtube

LOG_FILE = os.path.join(os.path.dirname(__file__), "upload_log.json")

class YouTubeApp:
    def __init__(self):
        self.root = tk.Tk()
        self.root.title("YouTube Music Video Creator")
        self.root.geometry("800x600")
        self.root.configure(bg="#0a0a1a")
        self.running = False
        self.create_ui()

    def create_ui(self):
        title_frame = tk.Frame(self.root, bg="#0f1a30", height=60)
        title_frame.pack(fill="x")
        title_frame.pack_propagate(False)
        tk.Label(title_frame, text="YouTube Music Video Creator", font=("Arial", 20, "bold"),
                fg="#00d4ff", bg="#0f1a30").pack(pady=15)

        main_frame = tk.Frame(self.root, bg="#0a0a1a")
        main_frame.pack(fill="both", expand=True, padx=15, pady=10)

        left_frame = tk.Frame(main_frame, bg="#0a0a1a", width=350)
        left_frame.pack(side="left", fill="both", expand=True, padx=(0, 10))

        tk.Label(left_frame, text="Video Settings", font=("Arial", 14, "bold"),
                fg="#ffffff", bg="#0a0a1a").pack(anchor="w", pady=(0, 10))

        tk.Label(left_frame, text="Video Title:", fg="#aaaaaa", bg="#0a0a1a").pack(anchor="w")
        self.title_var = tk.StringVar(value="Relaxing Music Mix")
        tk.Entry(left_frame, textvariable=self.title_var, width=40, font=("Arial", 11)).pack(fill="x", pady=(0, 10))

        tk.Label(left_frame, text="Duration (minutes):", fg="#aaaaaa", bg="#0a0a1a").pack(anchor="w")
        self.duration_var = tk.StringVar(value="1")
        tk.Spinbox(left_frame, from_=1, to=60, textvariable=self.duration_var, width=10).pack(anchor="w", pady=(0, 10))

        tk.Label(left_frame, text="Number of Videos:", fg="#aaaaaa", bg="#0a0a1a").pack(anchor="w")
        self.count_var = tk.StringVar(value="1")
        tk.Spinbox(left_frame, from_=1, to=10, textvariable=self.count_var, width=10).pack(anchor="w", pady=(0, 15))

        btn_frame = tk.Frame(left_frame, bg="#0a0a1a")
        btn_frame.pack(fill="x")

        self.start_btn = tk.Button(btn_frame, text="CREATE VIDEO", command=self.start_create,
                                  bg="#00d4ff", fg="#000000", font=("Arial", 12, "bold"),
                                  width=15, cursor="hand2")
        self.start_btn.pack(side="left")

        self.stop_btn = tk.Button(btn_frame, text="STOP", command=self.stop_create,
                                 bg="#ff4444", fg="#ffffff", font=("Arial", 12, "bold"),
                                 width=8, state="disabled", cursor="hand2")
        self.stop_btn.pack(side="left", padx=10)

        right_frame = tk.Frame(main_frame, bg="#0a0a1a", width=400)
        right_frame.pack(side="right", fill="both", expand=True)

        tk.Label(right_frame, text="Activity Log", font=("Arial", 14, "bold"),
                fg="#ffffff", bg="#0a0a1a").pack(anchor="w", pady=(0, 10))

        self.log_text = scrolledtext.ScrolledText(right_frame, height=22, width=50,
                                                  bg="#0a1628", fg="#00ff88",
                                                  font=("Consolas", 10))
        self.log_text.pack(fill="both", expand=True)

        status_frame = tk.Frame(self.root, bg="#0f1a30", height=40)
        status_frame.pack(fill="x", side="bottom")
        status_frame.pack_propagate(False)
        self.status_label = tk.Label(status_frame, text="Ready", fg="#00d4ff",
                                    bg="#0f1a30", font=("Arial", 10))
        self.status_label.pack(pady=10)

    def log(self, msg):
        timestamp = datetime.datetime.now().strftime("%H:%M:%S")
        self.log_text.insert("end", f"[{timestamp}] {msg}\n")
        self.log_text.see("end")
        self.root.update_idletasks()

    def start_create(self):
        try:
            count = int(self.count_var.get())
            duration = int(self.duration_var.get())
        except:
            count = 1
            duration = 1

        title = self.title_var.get()

        self.running = True
        self.start_btn.config(state="disabled")
        self.stop_btn.config(state="normal")

        thread = threading.Thread(target=self.create_videos, args=(title, duration, count))
        thread.daemon = True
        thread.start()

    def stop_create(self):
        self.running = False
        self.start_btn.config(state="normal")
        self.stop_btn.config(state="disabled")
        self.log("STOPPED")

    def create_videos(self, title, duration, count):
        for i in range(count):
            if not self.running:
                break

            video_title = f"{title} {i+1}" if count > 1 else title
            self.log(f"[{i+1}/{count}] Creating: {video_title} ({duration}min)")

            try:
                self.status_label.config(text=f"Creating video {i+1}/{count}...")
                path = create_music_video(video_title, duration_minutes=duration)
                self.log(f"  Created: {os.path.basename(path)}")

            except Exception as e:
                self.log(f"  ERROR: {str(e)}")

        self.running = False
        self.start_btn.config(state="normal")
        self.stop_btn.config(state="disabled")
        self.status_label.config(text="Completed!")
        self.log(f"DONE! {count} videos created")

    def run(self):
        self.root.mainloop()

if __name__ == "__main__":
    app = YouTubeApp()
    app.run()
