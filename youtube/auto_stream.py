#!/usr/bin/env python3
import os, sys, json, time, subprocess, threading
import tkinter as tk
from tkinter import ttk, scrolledtext, messagebox

sys.path.insert(0, os.path.dirname(__file__))
os.chdir(os.path.dirname(__file__))

from livestream_creator import create_livestream_video

OBS_CONFIG = {
    "host": "localhost",
    "port": 4455,
    "password": ""
}

class AutoStreamApp:
    def __init__(self):
        self.root = tk.Tk()
        self.root.title("YouTube Auto Stream - OBS Integration")
        self.root.geometry("900x650")
        self.root.configure(bg="#0a0a0a")
        self.running = False
        self.obs_process = None
        self.create_ui()

    def create_ui(self):
        title_frame = tk.Frame(self.root, bg="#1a0a0a", height=60)
        title_frame.pack(fill="x")
        title_frame.pack_propagate(False)
        tk.Label(title_frame, text="YouTube Auto Stream", font=("Arial", 20, "bold"),
                fg="#ff0000", bg="#1a0a0a").pack(pady=15)

        main_frame = tk.Frame(self.root, bg="#0a0a0a")
        main_frame.pack(fill="both", expand=True, padx=15, pady=10)

        left_frame = tk.Frame(main_frame, bg="#0a0a0a", width=400)
        left_frame.pack(side="left", fill="both", expand=True, padx=(0, 10))

        tk.Label(left_frame, text="Stream Settings", font=("Arial", 14, "bold"),
                fg="#ffffff", bg="#0a0a0a").pack(anchor="w", pady=(0, 10))

        tk.Label(left_frame, text="Stream Title:", fg="#aaaaaa", bg="#0a0a0a").pack(anchor="w")
        self.title_var = tk.StringVar(value="24/7 Music Live")
        tk.Entry(left_frame, textvariable=self.title_var, width=42, font=("Arial", 11)).pack(fill="x", pady=(0, 10))

        tk.Label(left_frame, text="Duration (hours):", fg="#aaaaaa", bg="#0a0a0a").pack(anchor="w")
        self.hours_var = tk.StringVar(value="1")
        tk.Spinbox(left_frame, from_=1, to=24, textvariable=self.hours_var, width=10).pack(anchor="w", pady=(0, 10))

        tk.Label(left_frame, text="YouTube Stream Key:", fg="#aaaaaa", bg="#0a0a0a").pack(anchor="w")
        self.key_var = tk.StringVar(value="")
        tk.Entry(left_frame, textvariable=self.key_var, width=42, font=("Arial", 11), show="*").pack(fill="x", pady=(0, 10))

        tk.Label(left_frame, text="OBS Path:", fg="#aaaaaa", bg="#0a0a0a").pack(anchor="w")
        self.obs_path_var = tk.StringVar(value=r"C:\Program Files\obs-studio\bin\64bit\obs64.exe")
        tk.Entry(left_frame, textvariable=self.obs_path_var, width=42, font=("Arial", 10)).pack(fill="x", pady=(0, 15))

        btn_frame = tk.Frame(left_frame, bg="#0a0a0a")
        btn_frame.pack(fill="x")

        self.start_btn = tk.Button(btn_frame, text="START STREAM", command=self.start_stream,
                                  bg="#ff0000", fg="#ffffff", font=("Arial", 12, "bold"),
                                  width=15, cursor="hand2")
        self.start_btn.pack(side="left")

        self.stop_btn = tk.Button(btn_frame, text="STOP", command=self.stop_stream,
                                 bg="#666666", fg="#ffffff", font=("Arial", 12, "bold"),
                                 width=8, state="disabled", cursor="hand2")
        self.stop_btn.pack(side="left", padx=10)

        info_frame = tk.Frame(left_frame, bg="#1a0a0a", relief="raised", bd=1)
        info_frame.pack(fill="x", pady=10)
        tk.Label(info_frame, text="Auto Stream Process:", font=("Arial", 10, "bold"),
                fg="#ff0000", bg="#1a0a0a").pack(anchor="w", padx=10, pady=(5,0))
        info_text = "1. Creates video automatically\n2. Opens OBS Studio\n3. Configures stream settings\n4. Starts streaming to YouTube"
        tk.Label(info_frame, text=info_text, fg="#aaaaaa", bg="#1a0a0a",
                justify="left", font=("Arial", 9)).pack(anchor="w", padx=10, pady=5)

        right_frame = tk.Frame(main_frame, bg="#0a0a0a", width=450)
        right_frame.pack(side="right", fill="both", expand=True)

        tk.Label(right_frame, text="Stream Log", font=("Arial", 14, "bold"),
                fg="#ffffff", bg="#0a0a0a").pack(anchor="w", pady=(0, 10))

        self.log_text = scrolledtext.ScrolledText(right_frame, height=25, width=55,
                                                  bg="#0a0a0a", fg="#ff4444",
                                                  font=("Consolas", 10))
        self.log_text.pack(fill="both", expand=True)

        status_frame = tk.Frame(self.root, bg="#1a0a0a", height=40)
        status_frame.pack(fill="x", side="bottom")
        status_frame.pack_propagate(False)
        self.status_label = tk.Label(status_frame, text="Ready - Enter stream key and click START",
                                    fg="#ff0000", bg="#1a0a0a", font=("Arial", 10))
        self.status_label.pack(pady=10)

    def log(self, msg):
        timestamp = time.strftime("%H:%M:%S")
        self.log_text.insert("end", f"[{timestamp}] {msg}\n")
        self.log_text.see("end")
        self.root.update_idletasks()

    def start_stream(self):
        stream_key = self.key_var.get().strip()
        if not stream_key:
            messagebox.showerror("Error", "Please enter YouTube Stream Key!")
            return

        self.running = True
        self.start_btn.config(state="disabled")
        self.stop_btn.config(state="normal")

        thread = threading.Thread(target=self.run_stream)
        thread.daemon = True
        thread.start()

    def stop_stream(self):
        self.running = False
        self.start_btn.config(state="normal")
        self.stop_btn.config(state="disabled")
        self.status_label.config(text="Stopping...")

        if self.obs_process:
            try:
                self.obs_process.terminate()
                self.log("OBS stopped")
            except:
                pass

        self.log("STREAM STOPPED")
        self.status_label.config(text="Stopped")

    def run_stream(self):
        try:
            title = self.title_var.get()
            hours = int(self.hours_var.get())
            stream_key = self.key_var.get().strip()
            obs_path = self.obs_path_var.get()

            self.log("=== AUTO STREAM STARTING ===")

            # Step 1: Create video
            self.log("[1/4] Creating livestream video...")
            self.status_label.config(text="Creating video...")
            video_path = create_livestream_video(title, hours)
            self.log(f"  Video created: {os.path.basename(video_path)}")

            # Step 2: Check OBS
            self.log("[2/4] Checking OBS Studio...")
            self.status_label.config(text="Checking OBS...")
            if not os.path.exists(obs_path):
                self.log("  ERROR: OBS not found at: " + obs_path)
                self.log("  Please install OBS or set correct path")
                return
            self.log("  OBS found!")

            # Step 3: Create OBS scene config
            self.log("[3/4] Configuring OBS...")
            self.status_label.config(text="Configuring OBS...")
            self.create_obs_config(video_path, stream_key)
            self.log("  OBS configured!")

            # Step 4: Launch OBS
            self.log("[4/4] Launching OBS...")
            self.status_label.config(text="Launching OBS...")
            self.obs_process = subprocess.Popen([obs_path])
            time.sleep(5)
            self.log("  OBS launched!")
            self.log("")
            self.log("=== STREAM ACTIVE ===")
            self.log("Video is now streaming to YouTube!")
            self.log("To start: Click 'Start Streaming' in OBS")
            self.log("To stop: Click 'Stop Streaming' in OBS")
            self.status_label.config(text="STREAM ACTIVE - Check OBS")

        except Exception as e:
            self.log(f"ERROR: {str(e)}")
            self.status_label.config(text="Error occurred")

    def create_obs_config(self, video_path, stream_key):
        config_dir = os.path.join(os.path.dirname(__file__), "obs_config")
        os.makedirs(config_dir, exist_ok=True)

        basic_scene = {
            "current_program_scene": "LiveScene",
            "scenes": [
                {
                    "name": "LiveScene",
                    "sources": [
                        {
                            "name": "VideoSource",
                            "type": "ffmpeg_source",
                            "settings": {
                                "local_file": video_path,
                                "looping": True,
                                "is_local_file": True
                            }
                        }
                    ]
                }
            ]
        }

        config_path = os.path.join(config_dir, "scene_config.json")
        with open(config_path, "w") as f:
            json.dump(basic_scene, f, indent=2)

        self.log(f"  Config saved: {config_path}")

    def run(self):
        self.root.mainloop()

if __name__ == "__main__":
    app = AutoStreamApp()
    app.run()
