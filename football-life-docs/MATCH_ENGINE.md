# Football Life — Match Engine

## 1. Purpose

The match engine creates an interactive football experience where the player controls one footballer inside a simulated team match.

The goal is not to reproduce FIFA.

The goal is to capture the most exciting decisions of being one player on the pitch.

---

## 2. Core Philosophy

The match is divided into:

```text
Match Simulation
        ↓
Relevant Situation
        ↓
Player Decision
        ↓
Outcome Resolution
        ↓
Presentation
```

The simulation controls the world.

The player controls their footballer when meaningful interaction is available.

---

## 3. Match Participants

The match contains:

- player's team
- opponent team
- player's footballer
- teammates
- opponents
- referees
- optional substitutes

The initial system should not attempt full high-fidelity football simulation for every player.

Use an abstraction level appropriate for the player's current situation.

---

## 4. Match State

Core state includes:

- score
- time
- possession
- team momentum
- tactical state
- player positions
- player stamina
- player form
- cards
- substitutions
- injuries
- match events

---

## 5. Tactical Context

Match situations depend on:

- formation
- possession
- scoreline
- match time
- team mentality
- opponent tactics
- field position

Examples:

```text
Winning 1-0 at 85'
→ lower risk

Losing 0-1 at 85'
→ higher attacking risk

Counterattack
→ space behind defense

Defensive block
→ fewer attacking opportunities
```

---

## 6. Player Position

The player's position determines the situations they commonly encounter.

### Striker

Examples:

- runs behind defense
- finishing
- pressing
- hold-up play
- positioning

### Winger

Examples:

- 1v1
- crossing
- cutting inside
- counterattack

### Midfielder

Examples:

- receiving under pressure
- passing
- through balls
- pressing
- positioning

### Defender

Examples:

- interception
- tackle
- marking
- aerial challenge
- buildup

### Goalkeeper

Examples:

- saves
- positioning
- distribution
- one-on-one situations

---

## 7. Situation Generation

The match simulation generates opportunities based on the current match state.

Example:

```text
Team wins possession
        ↓
Ball enters midfield
        ↓
Player is available
        ↓
Forward movement detected
        ↓
Through-ball opportunity
```

The opportunity should contain structured context.

Example:

```text
Situation:
    type = ThroughBall
    playerPosition = ...
    targetPosition = ...
    opponentPressure = ...
    space = ...
    expectedValue = ...
```

---

## 8. Player Actions

Initial actions:

```text
Pass
Through Ball
Dribble
Shoot
Cross
Clear
Tackle
Press
Move
Hold Position
```

Not every action is available in every situation.

---

## 9. Action Resolution

An action should consider:

```text
Player Ability
+
Player State
+
Situation
+
Opponent Pressure
+
Timing
+
Decision
+
Controlled Randomness
```

Example:

Passing success depends on:

- passing
- vision
- composure
- pressure
- distance
- target movement
- timing

---

## 10. Decision Quality

The player should be rewarded for recognizing situations.

A risky pass may have:

- high potential reward
- high failure probability

A safe pass may have:

- lower reward
- higher success probability

The goal is meaningful decision-making rather than reaction speed alone.

---

## 11. Dribbling

Dribbling should consider:

- dribbling ability
- agility
- pace
- defender ability
- space
- timing
- direction

Possible outcomes:

- beat defender
- retain possession
- lose possession
- draw foul

---

## 12. Shooting

Shooting depends on:

- finishing
- composure
- technique
- position
- angle
- pressure
- body orientation
- goalkeeper
- shot type

Possible outcomes:

- goal
- save
- miss
- block
- post
- deflection

---

## 13. Passing

Passing depends on:

- passing
- vision
- pressure
- distance
- target quality
- timing

Types:

- short pass
- long pass
- through ball
- cross

---

## 14. Defensive Actions

Defensive actions depend on:

- tackling
- positioning
- strength
- pace
- anticipation
- opponent skill
- timing

Possible outcomes:

- clean tackle
- interception
- pressure
- foul
- beaten

---

## 15. Off-Ball Movement

The player should receive contextual prompts for movement.

Examples:

```text
Make Run
Hold Position
Drop Back
Press
Overlap
Attack Space
```

The simulation determines whether the opportunity exists.

---

## 16. Stamina

Stamina affects:

- sprint ability
- pressing
- recovery
- decision quality
- late-match performance

The player's stamina should be visible but not dominate the HUD.

---

## 17. Match Rating

Match rating should consider:

- successful actions
- goals
- assists
- chances created
- defensive contributions
- errors
- tactical behavior
- playing time

Position should influence evaluation.

A defender should not be judged using striker expectations.

---

## 18. Manager Evaluation

The manager should evaluate:

- performance
- tactical discipline
- effort
- decisions
- consistency

The result affects manager trust.

---

## 19. Match Flow

A simplified match flow:

```text
Kickoff
   ↓
Simulate Match
   ↓
Generate Relevant Player Situation
   ↓
Present Situation
   ↓
Player Input
   ↓
Resolve Action
   ↓
Update Match State
   ↓
Continue
   ↓
Full Time
```

The amount of player interaction should depend on match context.

---

## 20. Presentation Boundary

The simulation produces:

```text
What happened
```

Unity decides:

```text
How it looks and feels
```

For example:

```text
Simulation:
Player completed through ball.

Unity:
Camera follows pass.
Animation plays.
Ball travels.
Teammate reacts.
Crowd reacts.
UI displays result.
```

---

## 21. Camera

The camera should prioritize:

- readability
- anticipation
- player context
- action visibility

The camera can dynamically adapt to:

- attacking situations
- defensive situations
- shots
- set pieces
- important moments

---

## 22. HUD

Keep the HUD minimal.

Core elements:

- score
- time
- player indicator
- stamina
- contextual actions

Avoid permanent display of every possible statistic.

---

## 23. Match Events

Important events should be explicit:

```text
MatchStarted
PossessionChanged
SituationCreated
ActionAttempted
ActionResolved
GoalScored
ChanceCreated
CardIssued
InjuryOccurred
SubstitutionMade
MatchEnded
```

---

## 24. Difficulty

Difficulty should primarily modify:

- opponent quality
- situation frequency
- pressure
- AI decision quality
- tolerance for mistakes

Do not simply make the game unfair by manipulating outcomes.

---

## 25. Position-Specific Progression

Match experience should contribute to development relevant to the player's role.

Examples:

Successful through balls:

```text
Passing XP
Vision XP
Decision XP
```

Successful tackles:

```text
Tackling XP
Positioning XP
Defensive Decision XP
```

Goals:

```text
Finishing XP
Composure XP
Confidence
```

---

## 26. Match Engine Development Strategy

Build incrementally.

### Version 1

- one position
- basic passing
- basic shooting
- simple opportunities
- basic opponent pressure

### Version 2

- multiple positions
- dribbling
- defending
- stamina
- tactical context

### Version 3

- richer team behavior
- advanced positioning
- set pieces
- substitutions
- richer AI

Do not build a complete football simulation before the core interaction is fun.

---

## 27. Critical Principle

The match engine should create the feeling:

> "I made that happen."

The player should feel responsible for meaningful outcomes without having to control every player on the pitch.
