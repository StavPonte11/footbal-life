---
name: llm-game-integration
description: Use when integrating LLMs (OpenAI, Gemini) into Unity for in-game dynamic commentary, NPC dialogue, dynamic match event generation, and tool/function calling.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# In-Game LLM & OpenAI Integration

Architecture patterns and best practices for integrating OpenAI / Gemini into Unity for dynamic storytelling, NPC AI, procedural match events, and commentary.

## Architecture Guidelines

1. **Decouple AI from Game Loop**: Never make synchronous HTTP calls on the main Unity thread. Use `UnityWebRequest` with `async/await` (via `UniTask` or `Task`) or streaming Server-Sent Events (SSE).
2. **Structured Outputs (JSON Schema)**: Always enforce JSON Schema or strict mode so responses deserialize directly into strongly-typed C# models without fragile regex parsing.
3. **Graceful Fallbacks**: Games must never freeze or crash if an LLM API call times out or encounters network limits. Always provide pre-authored fallback dialogue or local procedural generation.
4. **Token & Rate Limiting**: Cache common prompts, batch requests where possible, and run client-side rate limiters.

## Strongly-Typed C# Event Generator Pattern

```csharp
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

[Serializable]
public class MatchCommentaryResponse
{
    public string commentatorQuote;
    public string excitementLevel; // "Low", "Medium", "High", "Goal"
    public string tacticalInsight;
}

public class LLMCommentaryService : MonoBehaviour
{
    [SerializeField] private string apiKey;
    private const string ApiEndpoint = "https://api.openai.com/v1/chat/completions";

    public async Task<MatchCommentaryResponse> GenerateEventCommentaryAsync(string matchContext)
    {
        string prompt = $"You are a world-class football commentator. Describe this moment: {matchContext}. Respond ONLY with JSON matching the schema.";
        // Dispatch async UnityWebRequest with Bearer token
        // Deserialize response into MatchCommentaryResponse
        return await Task.FromResult(new MatchCommentaryResponse { commentatorQuote = "What a strike from distance!" });
    }
}
```

## Game Function Calling & Tools

Expose C# game state modifiers to LLM tool calls:
- `SubPlayer(string playerOut, string playerIn)`
- `AdjustTactics(string formation, string mentality)`
- `TriggerCrowdChant(string chantName, float intensity)`
