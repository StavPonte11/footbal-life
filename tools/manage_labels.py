import subprocess
import urllib.request
import json

proc = subprocess.run(
    ["git", "credential", "fill"],
    input="protocol=https\nhost=github.com\n",
    text=True,
    capture_output=True,
    check=True
)
token = None
for line in proc.stdout.splitlines():
    if line.startswith("password="):
        token = line.split("=", 1)[1].strip()

headers = {
    "Authorization": f"Bearer {token}",
    "User-Agent": "FootballLife-IssueCreator",
    "Accept": "application/vnd.github.v3+json"
}

req = urllib.request.Request(
    "https://api.github.com/repos/StavPonte11/footbal-life/labels",
    headers=headers
)
with urllib.request.urlopen(req) as resp:
    data = json.loads(resp.read().decode("utf-8"))
    for l in data:
        print(f"{l['name']}: color={l['color']}, desc={l.get('description', '')}")
