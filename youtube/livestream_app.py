#!/usr/bin/env python3
import os, sys, threading, time
import tkinter as tk
from tkinter import ttk, scrolledtext

sys.path.insert(0, os.path.dirname(__file__))
os.chdir(os.path.dirname(__file__))

from livestream_creator import create_livestream_video

class LivestreamApp:
    def __init__(self):
        self.root = tk.Tk()
        self.root.title("YouTube 24/7 Livestream Creator")
        self.root.geometry("800x550")
        self.root.configure(bg="#0a0a1a")
        self.running = False
        self.create_ui()

    def create_ui(self):
        title_frame = tk.Frame(self.root, bg="#1a0a0a", height=60)
        title_frame.pack(fill="x")
        title_frame.pack_propagate(False)
        tk.Label(title_frame, text="YouTube 24/7 Livestream Creator", font=("Arial", 20, "bold"),
                fg="#ff4444", bg="#1a0a0a").pack(pady=15)

        main_frame = tk.Frame(self.root, bg="#0a0a1a")
        main_frame.pack(fill="both", expand=True, padx=15, pady=10)

        left_frame = tk.Frame(main_frame, bg="#0a0a1a", width=350)
        left_frame.pack(side="left", fill="both", expand=True, padx=(0, 10))

        tk.Label(left_frame, text="Stream Settings", font=("Arial", 14, "bold"),
                fg="#ffffff", bg="#0a0a1a").pack(anchor="w", pady=(0, 10))

        tk.Label(left_frame, text="Stream Title:", fg="#aaaaaa", bg="#0a0a1a").pack(anchor="w")
        self.title_var = tk.StringVar(value="24/7 Music Live")
        tk.Entry(left_frame, textvariable=self.title_var, width=40, font=("Arial", 11)).pack(fill="x", pady=(0, 10))

        tk.Label(left_frame, text="Duration (hours):", fg="#aaaaaa", bg="#0a0a1a").pack(anchor="w")
        self.hours_var = tk.StringVar(value="1")
        tk.Spinbox(left_frame, from_=1, to=24, textvariable=self.hours_var, width=10).pack(anchor="w", pady=(0, 15))

        btn_frame = tk.Frame(left_frame, bg="#0a0a1a")
        btn_frame.pack(fill="x")

        self.start_btn = tk.Button(btn_frame, text="CREATE VIDEO", command=self.start_create,
                                  bg="#ff4444", fg="#ffffff", font=("Arial", 12, "bold"),
                                  width=15, cursor="hand2")
        self.start_btn.pack(side="left")

        self.stop_btn = tk.Button(btn_frame, text="STOP", command=self.stop_create,
                                 bg="#666666", fg="#ffffff", font=("Arial", 12, "bold"),
                                 width=8, state="disabled", cursor="hand2")
        self.stop_btn.pack(side="left", padx=10)

        info_frame = tk.Frame(left_frame, bg="#1a0a0a", relief="raised", bd=1)
        info_frame.pack(fill="x", pady=10)
        tk.Label(info_frame, text="How to stream:", font=("Arial", 10, "bold"),
                fg="#ff4444", bg="#1a0a0a").pack(anchor="w", padx=10, pady=(5,0))
        info_text = "1. Create video here\n2. Open OBS Studio\n3. Add Media Source (loop)\n4. Stream to YouTube"
        tk.Label(info_frame, text=info_text, fg="#aaaaaa", bg="#1a0a0a",
                justify="left", font=("Arial", 9)).pack(anchor="w", padx=10, pady=5)

        right_frame = tk.Frame(main_frame, bg="#0a0a1a", width=400)
        right_frame.pack(side="right", fill="both", expand=True)

        tk.Label(right_frame, text="Activity Log", font=("Arial", 14, "bold"),
                fg="#ffffff", bg="#0a0a1a").pack(anchor="w", pady=(0, 10))

        self.log_text = scrolledtext.ScrolledText(right_frame, height=20, width=50,
                                                  bg="#0a1628", fg="#ff8888",
                                                  font=("Consolas", 10))
        self.log_text.pack(fill="both", expand=True)

        status_frame = tk.Frame(self.root, bg="#1a0a0a", height=40)
        status_frame.pack(fill="x", side="bottom")
        status_frame.pack_propagate(False)
        self.status_label = tk.Label(status_frame, text="Ready", fg="#ff4444",
                                    bg="#1a0a0a", font=("Arial", 10))
        self.status_label.pack(pady=10)

    def log(self, msg):
        timestamp = time.strftime("%H:%M:%S")
        self.log_text.insert("end", f"[{timestamp}] {msg}\n")
        self.log_text.see("end")
        self.root.update_idletasks()

    def start_create(self):
        try:
            hours = int(self.hours_var.get())
        except:
            hours = 1
        title = self.title_var.get()

        self.running = True
        self.start_btn.config(state="disabled")
        self.stop_btn.config(state="normal")

        thread = threading.Thread(target=self.create_video, args=(title, hours))
        thread.daemon = True
        thread.start()

    def stop_create(self):
        self.running = False
        self.start_btn.config(state="normal")
        self.stop_btn.config(state="disabled")
        self.log("STOPPED")

    def create_video(self, title, hours):
        self.log(f"Creating {hours}h livestream video...")
        try:
            self.status_label.config(text=f"Creating {hours}h video...")
            path = create_livestream_video(title, hours)
            self.log(f"Created: {os.path.basename(path)}")
            self.log("Ready to stream with OBS!")
        except Exception as e:
            self.log(f"ERROR: {str(e)}")

        self.running = False
        self.start_btn.config(state="normal")
        self.stop_btn.config(state="disabled")
        self.status_label.config(text="Completed!")

    def run(self):
        self.root.mainloop()

if __name__ == "__main__":
    app = LivestreamApp()
    app.run()
