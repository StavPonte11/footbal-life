import urllib.request
import urllib.error
import json
import sys

MCP_URL = "http://127.0.0.1:8080/mcp"

class UnityMcpClient:
    def __init__(self, base_url=MCP_URL):
        self.base_url = base_url
        self.session_id = None
        self._req_id = 1
        self.initialize()

    def _post(self, payload):
        data = json.dumps(payload).encode("utf-8")
        headers = {
            "Content-Type": "application/json",
            "Accept": "application/json, text/event-stream"
        }
        if self.session_id:
            headers["mcp-session-id"] = self.session_id

        req = urllib.request.Request(self.base_url, data=data, headers=headers)
        with urllib.request.urlopen(req, timeout=15) as resp:
            new_session = resp.headers.get("mcp-session-id")
            if new_session:
                self.session_id = new_session

            raw_body = resp.read().decode("utf-8")
            # Parse SSE format: event: message\ndata: {...}
            final_data = None
            for line in raw_body.splitlines():
                if line.startswith("data: "):
                    msg = json.loads(line[6:])
                    if "result" in msg or "error" in msg:
                        return msg
                    final_data = msg
            return final_data or {"raw": raw_body}

    def initialize(self):
        payload = {
            "jsonrpc": "2.0",
            "id": self._req_id,
            "method": "initialize",
            "params": {
                "protocolVersion": "2024-11-05",
                "capabilities": {},
                "clientInfo": {"name": "FootballLifeClient", "version": "1.0"}
            }
        }
        self._req_id += 1
        res = self._post(payload)
        return res

    def call_tool(self, name, arguments=None):
        if arguments is None:
            arguments = {}
        payload = {
            "jsonrpc": "2.0",
            "id": self._req_id,
            "method": "tools/call",
            "params": {
                "name": name,
                "arguments": arguments
            }
        }
        self._req_id += 1
        return self._post(payload)

if __name__ == "__main__":
    tool_name = sys.argv[1] if len(sys.argv) > 1 else "read_console"
    args = json.loads(sys.argv[2]) if len(sys.argv) > 2 else {}
    client = UnityMcpClient()
    res = client.call_tool(tool_name, args)
    print(json.dumps(res, indent=2))
