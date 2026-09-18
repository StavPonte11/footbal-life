# Match Engine Development Skill

## Philosophy

Football Life is not a traditional FIFA-style football game.

The player controls one footballer.

The simulation controls the rest of the match.

## Match layers

Separate:

### Match simulation

Determines:

- possession
- positioning
- tactical state
- opportunities
- opponent behavior
- teammate behavior
- match events

### Player interaction

Determines the player's decision/action when an opportunity occurs.

### Presentation

Unity displays the situation.

## Match events

Prefer explicit event types:

```text
PassOpportunity
DribbleOpportunity
ShotOpportunity
CrossOpportunity
DefensiveAction
Tackle
Interception
Foul
Offside
Goal
Injury
Substitution
```

## Position-specific gameplay

A striker, winger, midfielder, defender and goalkeeper must not receive identical opportunities.

Position should influence:

- spatial opportunities
- expected actions
- tactical responsibilities
- evaluation criteria
- progression

## Player decisions

Outcomes should consider:

- player attributes
- current state
- spatial context
- tactical context
- opponent attributes
- timing
- controlled randomness

Do not make outcomes purely random.

## Presentation

The simulation produces a situation.

Unity turns it into:

- camera behavior
- player movement
- animation
- UI
- sound
- effects

Do not put match simulation logic into Unity animation scripts.