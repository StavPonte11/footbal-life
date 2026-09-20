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

req = urllib.request.Request(
    "https://api.github.com/repos/StavPonte11/footbal-life/issues?state=all&per_page=100",
    headers={
        "Authorization": f"Bearer {token}",
        "User-Agent": "FootballLife-IssueCreator",
        "Accept": "application/vnd.github.v3+json"
    }
)
with urllib.request.urlopen(req) as resp:
    issues = json.loads(resp.read().decode("utf-8"))
    print(f"Total issues fetched: {len(issues)}")
    for iss in sorted(issues, key=lambda x: x["number"]):
        labels = [l["name"] for l in iss.get("labels", [])]
        print(f"#{iss['number']}: {iss['title']} ({iss['state']}) - labels: {labels}")
