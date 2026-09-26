import os
import sys
import json
import urllib.request
import urllib.error
import subprocess

def get_token():
    token = os.environ.get("GITHUB_TOKEN") or os.environ.get("GITHUB_PERSONAL_ACCESS_TOKEN")
    if token:
        return token
    try:
        proc = subprocess.run(
            ["gh", "auth", "token"],
            text=True,
            capture_output=True,
            check=True
        )
        t = proc.stdout.strip()
        if t:
            return t
    except Exception:
        pass
    try:
        proc = subprocess.run(
            ["git", "credential", "fill"],
            input="protocol=https\nhost=github.com\n",
            text=True,
            capture_output=True,
            check=True
        )
        for line in proc.stdout.splitlines():
            if line.startswith("password="):
                return line.split("=", 1)[1].strip()
    except Exception:
        pass
    return None

def get_repo():
    try:
        proc = subprocess.run(
            ["git", "remote", "get-url", "origin"],
            text=True,
            capture_output=True,
            check=True
        )
        url = proc.stdout.strip()
        if "github.com" in url:
            parts = url.replace(":", "/").split("github.com/")[-1].replace(".git", "").split("/")
            if len(parts) >= 2:
                return f"{parts[0]}/{parts[1]}"
    except Exception:
        pass
    return "StavPonte11/footbal-life"

issues_to_create = [
    {
        "title": "#P7-301: Audio Architecture, Buses & Volume Settings (Master/SFX/Ambience/UI/Music)",
        "body": """### User Story
As a mobile player, I want an AudioManager architecture with independent volume sliders and mute toggles for Master, SFX, Ambience, UI, and Music buses so that I can control sound levels and play comfortably with or without headphones.

### Acceptance Criteria
1. `AudioManager` persistent singleton initialized in `[MANAGERS]` category across scenes.
2. Volume conversion with proper logarithmic decibel scaling (0.0 to 1.0 mapped to -80dB to 0dB).
3. Persistent settings integration: Master, SFX, Ambience, UI, and Music volume saved and loaded via PlayerPrefs / settings service.
4. Clean decoupling via event subscriptions or service locator; zero GC allocation in hot paths.
""",
        "labels": ["audio", "phase-7", "unity"]
    },
    {
        "title": "#P7-302: Match SFX Integration (Kick Impact, Post Clang, Net Ripple, Whistle)",
        "body": """### User Story
As a player taking shots in 3D match situations, I want tactile, punchy match audio effects (kick impact scaled by shot power, metallic goal post/crossbar clangs, net rustle, and referee whistles) so that every strike feels physical and responsive.

### Acceptance Criteria
1. Kick sound triggered in `ShootingInteraction.cs` with volume and pitch modulation based on swipe velocity/kick power.
2. Goal post / crossbar collision sound (sharp metallic chime/ping) triggered when ball hits the woodwork.
3. Goal net ripple sound triggered in `GoalTrigger.cs` on scoring.
4. Referee whistle sound (start of situation, goal scored, final whistle).
5. Procedural high-fidelity PCM audio clip synthesis fallback so sounds play seamlessly without external asset dependencies.
""",
        "labels": ["audio", "gameplay", "phase-7"]
    },
    {
        "title": "#P7-303: Crowd Atmosphere & Reactive Dynamic Chants",
        "body": """### User Story
As a player in a stadium match situation, I want dynamic crowd atmosphere (continuous ambient stadium murmur, explosive goal celebration roar, and collective gasp on near-misses) so that match situations feel like real stadium events.

### Acceptance Criteria
1. Continuous ambient stadium loop playing through Ambience bus during 3D match scenes.
2. Reactive crowd roar triggered instantly upon goal confirmation.
3. Near-miss / goalkeeper save gasp triggered when a shot misses or is saved.
4. Volume/intensity scaled smoothly with fade transitions.
""",
        "labels": ["audio", "stadium", "phase-7"]
    },
    {
        "title": "#P7-304: UI Audio Feedback (Tap Clicks, Screen Transitions & Reward Chimes)",
        "body": """### User Story
As a player navigating menus and managing career finances, I want tactile UI audio feedback on button clicks, tab switches, and a satisfying cash/coin chime on weekly wage deposits.

### Acceptance Criteria
1. Subtle, modern UI click sound on primary/secondary button taps across UI Toolkit screens.
2. Satisfying coin/wage chime on week advance / wage credit / shop purchase.
3. Alert/chime on dilemma prompt or contract offer.
4. Routed to dedicated UI audio bus with mute respect.
""",
        "labels": ["audio", "ui-ux", "phase-7"]
    },
    {
        "title": "#P7-305: Background Music Management & Dynamic Ducking",
        "body": """### User Story
As a player in the hub and menus, I want ambient background music with smooth crossfading, loop handling, and automatic ducking when 3D match situations or high-priority dialogue start.

### Acceptance Criteria
1. Background music playback routed through Music bus.
2. Smooth fade-in, fade-out, and crossfade between menu and match states.
3. Music ducking support (-12dB duck during whistle/commentary/match play).
4. Full rights-safe procedural melodic ambient synth track generated in-engine.
""",
        "labels": ["audio", "music", "phase-7"]
    }
]

def main():
    token = get_token()
    repo = get_repo()

    if not token:
        print("Error: No GitHub token found.")
        sys.exit(1)

    headers = {
        "Authorization": f"Bearer {token}",
        "Accept": "application/vnd.github.v3+json",
        "Content-Type": "application/json"
    }

    created_issues = {}

    for item in issues_to_create:
        print(f"Creating issue: {item['title']}...")
        payload = {
            "title": item["title"],
            "body": item["body"],
            "labels": item["labels"]
        }
        req = urllib.request.Request(
            f"https://api.github.com/repos/{repo}/issues",
            data=json.dumps(payload).encode("utf-8"),
            headers=headers,
            method="POST"
        )
        try:
            with urllib.request.urlopen(req) as resp:
                data = json.loads(resp.read().decode("utf-8"))
                num = data["number"]
                created_issues[item['title']] = num
                print(f"Created #{num}: {data['html_url']}")
        except urllib.error.HTTPError as e:
            print(f"HTTPError: {e.code} {e.read().decode('utf-8')}")

    out_file = os.path.join(os.path.dirname(__file__), "milestone_73_issues.json")
    with open(out_file, "w") as f:
        json.dump(created_issues, f, indent=2)
    print(f"Saved created issue numbers to {out_file}")

if __name__ == "__main__":
    main()
