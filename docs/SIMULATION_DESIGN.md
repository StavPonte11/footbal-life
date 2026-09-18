# Football Life — Simulation Design

## 1. Simulation Philosophy

The simulation is the hidden machine that makes the player's career believable.

The goal is not perfect realism.

The goal is:

> believable, understandable, varied and fun outcomes.

The simulation should create emergent stories.

---

## 2. Time

The world advances through:

```text
Day
 ↓
Week
 ↓
Month
 ↓
Season
 ↓
Career
```

The player should usually interact at the day/week level.

The simulation may operate at finer resolution internally where useful.

---

## 3. Player Model

The player has four major categories of state.

### Ability

Long-term capability.

Examples:

- finishing
- passing
- dribbling
- pace
- stamina
- vision

### Potential

A probabilistic development ceiling rather than a guaranteed final rating.

### Current State

Temporary condition.

Examples:

- fatigue
- form
- confidence
- happiness
- motivation
- morale

### Career State

Examples:

- club
- position
- squad status
- manager trust
- reputation
- salary
- contract
- market value

---

## 4. Age and Development

Player development should be nonlinear.

A young player generally has:

- higher development potential
- greater room for improvement
- less experience

An older player generally has:

- more experience
- less growth potential
- possible physical decline

Development should depend on:

- age
- potential
- training
- playing time
- coaching
- match experience
- fatigue
- motivation

---

## 5. Training

Training is a tradeoff.

Inputs:

```text
training type
duration
intensity
player state
```

Outputs:

```text
attribute XP
fatigue
fitness
injury risk
confidence
```

Training categories:

```text
Technical
Physical
Mental
Position
Recovery
```

Training should have diminishing returns.

Repeatedly selecting one training type should not create infinite specialization without tradeoffs.

---

## 6. Fatigue

Fatigue represents short-term physical and mental load.

Sources:

- training
- matches
- travel
- social activities
- insufficient rest

Effects:

- performance
- recovery
- injury risk
- training efficiency
- mood

Fatigue should recover through:

- sleep
- rest
- recovery activities
- time

---

## 7. Confidence

Confidence affects performance but should not create runaway positive feedback.

Sources:

- goals
- assists
- good performances
- manager praise
- winning
- personal success

Negative sources:

- poor performance
- mistakes
- losing
- criticism
- lack of playing time

Confidence should have inertia and bounded effects.

---

## 8. Form

Form represents recent football performance.

It should respond faster than permanent ability.

Form can affect:

- manager selection
- performance
- transfer interest
- media attention

Form should decay toward a neutral state over time.

---

## 9. Happiness and Wellbeing

Happiness is influenced by:

- relationships
- career satisfaction
- money
- playing time
- lifestyle
- recent events

Low wellbeing should influence motivation and possibly performance.

Do not make happiness a simple linear score that dictates everything.

---

## 10. Position

Position is a core career identity.

Initial positions:

```text
GK
CB
FB
DM
CM
AM
LW/RW
ST
```

Position affects:

- relevant attributes
- training
- match opportunities
- tactical responsibilities
- manager expectations
- career evaluation

Position changes should be possible but costly enough to be meaningful.

---

## 11. Club Model

Clubs have:

- reputation
- financial strength
- squad quality
- league
- facilities
- manager
- tactical identity
- expectations
- competition level

Club quality should influence:

- opportunities
- pressure
- salary
- development environment
- transfer attractiveness

---

## 12. Manager Model

Managers have:

- tactical preferences
- trust in player
- preferred formations
- player preferences
- tolerance
- expectations

Manager trust should evolve over time.

---

## 13. Playing Time

Playing time depends on:

- manager trust
- player ability
- form
- position competition
- tactical fit
- fitness
- team situation

Possible statuses:

```text
Academy
Reserve
Bench
Rotation
Starter
Key Player
```

---

## 14. Match Performance

Performance should be a function of:

```text
Ability
+
Current State
+
Tactical Context
+
Situation
+
Opponent
+
Decision Quality
+
Controlled Randomness
```

Avoid making performance purely attribute-based.

The same player should produce different performances across matches.

---

## 15. Manager Trust

Manager trust is affected by:

Positive:

- good performances
- good training
- professionalism
- tactical discipline
- consistency

Negative:

- poor performances
- poor training
- disciplinary problems
- repeated bad decisions
- conflicts

Manager trust affects playing time and opportunities.

---

## 16. Contracts

Contract state includes:

- salary
- length
- role
- bonuses
- clauses
- expectations

Contract negotiation should consider:

- player ability
- reputation
- club finances
- playing time
- age
- market demand

---

## 17. Transfers

Transfer opportunities emerge from:

- player performance
- reputation
- position needs
- club finances
- contract situation
- market conditions

A transfer offer should contain:

```text
club
league
role
salary
contract
competition
location
club ambition
```

Transfer choices should have tradeoffs.

---

## 18. Economy

Money is a long-term progression system.

Income:

- salary
- bonuses
- sponsorships
- special opportunities

Expenses:

- home
- transport
- lifestyle
- social activities
- optional purchases

Money should unlock choices rather than simply increase a number.

---

## 19. Relationships

Relationships should be modeled as entities with context.

Potential relationships:

- partner
- parent
- sibling
- friend
- teammate
- manager
- agent

Relationships should have:

- affinity
- trust
- recent interaction
- unresolved issues
- shared history

---

## 20. Relationship Dynamics

Relationships can change due to:

- interaction
- neglect
- career moves
- money
- fame
- conflicts
- important events

Moving clubs or countries should have social consequences.

---

## 21. Life Events

Life events are data-driven.

Example schema:

```text
id
conditions
weight
cooldown
choices
effects
narrative
```

Conditions may include:

```text
age
club
salary
relationship state
fatigue
happiness
manager trust
reputation
```

---

## 22. Event Resolution

A life event follows:

```text
Trigger
 ↓
Present Situation
 ↓
Player Choice
 ↓
Resolve Effects
 ↓
Emit Event
 ↓
Narrative / UI
```

The narrative describes the outcome.

The simulation determines the outcome.

---

## 23. Reputation

Reputation should have multiple dimensions where useful:

- local recognition
- club reputation
- league reputation
- global reputation
- media visibility

Reputation affects:

- transfer opportunities
- sponsorships
- media
- social opportunities

---

## 24. Injuries

Injuries should initially remain simple.

Risk should depend on:

- fatigue
- workload
- match intensity
- player attributes
- randomness

Injuries affect:

- availability
- training
- development
- manager trust
- form

Do not build a complex medical simulator in the first version.

---

## 25. World Simulation

The world should continue without the player.

Over time:

- players develop
- players decline
- players transfer
- managers change
- clubs change
- young players emerge
- veterans retire

The player should feel like one participant in a living football world.

---

## 26. Progression Philosophy

Progression should have friction.

A player should not automatically become world class.

Factors limiting progression:

- potential
- playing time
- training
- fatigue
- injuries
- competition
- decisions
- age

---

## 27. Emergent Career Archetypes

The simulation should support:

- superstar
- solid professional
- late bloomer
- journeyman
- wonderkid who stalls
- injury-disrupted career
- loyal one-club player
- international star
- lower-league career
- comeback story

---

## 28. Simulation Invariants

Examples:

```text
Age never decreases.
Money changes only through explicit transactions.
Attributes remain within valid bounds.
Unavailable players cannot play.
Contracts have valid dates.
Relationships reference valid entities.
```

---

## 29. Multi-Career Validation

A single career cannot validate the simulation.

Use large batches.

Example:

```bash
career-simulator --careers 10000 --seasons 20
```

Inspect:

- average peak rating
- distribution of peak rating
- career length
- transfer count
- salary
- injuries
- goals
- assists
- retirement age

Look for unrealistic clusters and runaway progression.

---

## 30. Simulation Authority

The simulation is always authoritative.

Correct:

```text
Simulation
  ↓
Structured Result
  ↓
Unity / Narrative
```

Incorrect:

```text
UI
  ↓
directly changes Player.money
```

Incorrect:

```text
LLM
  ↓
changes Player.overall
```
