---
name: narrative-engine
description: Use when authoring branching dialogues, narrative quest graphs, story state machines, and integrating dialogue tools like Ink, Yarn Spinner, or ScriptableObject dialogue trees.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# Game Narrative & Dialogue Tooling

Patterns and tools for authoring branching storylines, player choices, press conferences, manager interviews, and dynamic career events in Unity.

## Narrative Tooling Options

### 1. Inkle Ink (`ink-unity-integration`)
- Industry standard for complex branching narratives with state variables, knots, stitches, and divert logic.
- In Unity: Reads compiled `.ink.json` files and evaluates story steps:
  ```csharp
  var story = new Ink.Runtime.Story(inkJsonAsset.text);
  string text = story.Continue();
  List<Choice> choices = story.currentChoices;
  ```

### 2. Yarn Spinner
- Friendly node-based dialogue syntax designed specifically for games.
- Integrates with Unity's TextMeshPro, audio cues, and custom Yarn commands (`<<trigger_stadium_cheer>>`).

### 3. Lightweight ScriptableObject Dialogue Graph
- Zero external package dependency.
- Define nodes with choices, condition checks (e.g. `reputation >= 70`), and event callbacks:
  ```csharp
  [CreateAssetMenu(fileName = "NewDialogueNode", menuName = "Narrative/Dialogue Node")]
  public class DialogueNode : ScriptableObject
  {
      [TextArea(3, 5)] public string speakerText;
      public string speakerName;
      public Sprite speakerPortrait;
      public List<DialogueChoice> choices;
  }

  [System.Serializable]
  public class DialogueChoice
  {
      public string choiceLabel;
      public DialogueNode nextNode;
      public int moralityDelta;
      public int budgetDelta;
  }
  ```

## Integration with Sports / Football Career Flow

1. **Pre-Match / Post-Match Press Conferences**:
   - Player questions driven by recent match events, player ratings, or rivalry status.
   - Choices influence Board Confidence, Fan Approval, and Squad Morale.
2. **Transfer Contract Negotiations**:
   - Multi-round dialogue where agent/player demands are weighed against available wage budget and squad role promises.
3. **Locker Room Talks**:
   - Half-time talks adjusting team mentality and tactical commitment.
