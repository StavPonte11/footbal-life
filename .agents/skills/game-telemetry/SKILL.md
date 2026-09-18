---
name: game-telemetry
description: Use when designing game telemetry, analytics schemas, event tracking, session management, performance profiling metrics, and telemetry pipelines (UGS Analytics, PostHog, GameAnalytics).
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# Game Analytics & Telemetry Architecture

Guidelines for instrumenting event taxonomy, performance telemetry, session analytics, and user progression in Unity games.

## Event Taxonomy Standard

Structure events with consistent naming (`object_verb`) and parameters:

| Event Name | Key Parameters | When to Dispatch |
|---|---|---|
| `session_start` | `session_id`, `app_version`, `device_model`, `os_version` | Game launched / foregrounded |
| `session_end` | `session_id`, `duration_seconds`, `exit_reason` | Game closed / backgrounded |
| `match_started` | `match_id`, `game_mode`, `team_home`, `team_away`, `difficulty` | Kickoff whistle |
| `match_ended` | `match_id`, `score_home`, `score_away`, `duration_seconds`, `winner` | Final whistle |
| `goal_scored` | `match_id`, `minute`, `player_id`, `shot_distance`, `is_penalty` | Ball crosses goal line |
| `progression_level_up` | `career_season`, `player_id`, `old_rating`, `new_rating` | Skill upgrade / milestone |
| `economy_transaction` | `item_id`, `item_type`, `currency`, `amount`, `balance_after` | In-game store / transfer fee |
| `performance_metrics` | `avg_fps`, `min_fps`, `memory_used_mb`, `draw_calls_peak` | Periodic snapshot / end of match |

## Telemetry Manager Implementation Pattern

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

public class TelemetryManager : MonoBehaviour
{
    public static TelemetryManager Instance { get; private set; }
    private readonly Queue<TelemetryEvent> eventBuffer = new();
    private string sessionId;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        sessionId = Guid.NewGuid().ToString();
    }

    public void TrackEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        parameters ??= new Dictionary<string, object>();
        parameters["session_id"] = sessionId;
        parameters["timestamp_utc"] = DateTime.UtcNow.ToString("o");

        var evt = new TelemetryEvent(eventName, parameters);
        eventBuffer.Enqueue(evt);

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[Telemetry] {eventName} => {JsonUtility.ToJson(evt)}");
        #endif

        // Trigger batch flush if buffer threshold exceeded
        if (eventBuffer.Count >= 20) FlushEvents();
    }

    public void FlushEvents()
    {
        // Dispatch batch asynchronously to telemetry backend (UGS / PostHog / REST)
    }
}

[Serializable]
public record TelemetryEvent(string EventName, Dictionary<string, object> Parameters);
```

## Privacy & Compliance
- Anonymize user identifiers (do not collect raw hardware IDs or PII).
- Implement player opt-out toggle in Settings.
- Support GDPR right-to-erasure workflows via player ID mapping.
