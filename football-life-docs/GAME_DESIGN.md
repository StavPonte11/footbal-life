# Football Life — Game Design Document

## 1. Vision

Football Life is a modern footballer career and life simulation game inspired by the core fantasy of New Star Soccer and expanded into a deeper, more visual, more systemic experience.

The player does not manage a football club.

The player lives the career of one footballer.

The central question is:

> What would it feel like to live the life of a professional footballer?

The game combines:

- football career progression
- interactive match gameplay
- training and development
- transfers and contracts
- money and lifestyle
- relationships
- home and personal life
- reputation and media
- meaningful decisions
- emergent stories

The objective is not to simulate every aspect of professional football with maximum complexity. The objective is to create a compelling fantasy of being a footballer.

---

## 2. Design Pillars

### 2.1 Live a career, don't manage a spreadsheet

The player should feel like a person whose career is unfolding.

The game should prioritize:

- anticipation
- decisions
- consequences
- relationships
- milestones
- setbacks
- personal identity

Raw statistics are useful, but they should support the experience rather than dominate it.

### 2.2 Football is the core career engine

Football performance remains the primary driver of progression.

Matches, training, playing time, manager trust, reputation and transfers should matter.

Personal life complements football rather than replacing it.

### 2.3 Personal life has real consequences

Personal life is not a cosmetic minigame.

Examples:

- poor sleep can reduce recovery
- social activities can increase happiness but consume energy
- relationships can affect wellbeing
- moving clubs can affect relationships
- fame can create opportunities and pressure
- lifestyle spending affects finances
- financial success unlocks new lifestyle choices

### 2.4 Player agency creates stories

The game should create situations and let the player decide.

The simulation should not dictate one correct path.

A player may choose:

- loyalty versus ambition
- rest versus social life
- money versus playing time
- stability versus a new country
- short-term success versus long-term development

### 2.5 Failure is part of the fantasy

Careers should not always go upward.

Possible outcomes include:

- becoming a superstar
- becoming a reliable professional
- being a journeyman
- becoming a late bloomer
- suffering a career setback
- failing to reach a predicted potential
- becoming a one-club player
- rebuilding a career after a poor period

Failure should create new decisions rather than simply ending the game.

---

## 3. Core Player Fantasy

The player begins as a young footballer and experiences:

```text
Youth
  ↓
First professional opportunity
  ↓
First-team football
  ↓
Development
  ↓
Recognition
  ↓
Career decisions
  ↓
Transfers / trophies / setbacks
  ↓
Peak career
  ↓
Late career
  ↓
Retirement
```

The game should make the player care about the journey between these milestones.

---

## 4. Primary Gameplay Loop

The fundamental loop is:

```text
PLAN
  ↓
TRAIN / LIVE
  ↓
MATCH
  ↓
PERFORM
  ↓
RECEIVE CONSEQUENCES
  ↓
MAKE DECISIONS
  ↓
PROGRESS
  ↓
NEXT WEEK
```

At any point, the player should understand:

1. What is happening?
2. What can I do?
3. What are the likely tradeoffs?
4. What happens next?

---

## 5. Time Structure

The game uses multiple time scales.

### Day

Used for:

- training
- matches
- social activities
- rest
- life events
- travel
- interactions

### Week

Used for:

- training planning
- match cycles
- recovery
- manager expectations
- recurring expenses

### Season

Used for:

- league progression
- trophies
- player statistics
- contract changes
- transfers
- awards

### Career

Used for:

- long-term development
- reputation
- wealth
- relationships
- legacy
- retirement

---

## 6. Player Creation

The player should be able to define:

- name
- nationality
- appearance
- preferred foot
- position
- basic personality traits
- starting club/path where appropriate

Position selection is a core feature.

Supported positions should initially include:

- GK
- CB
- FB
- DM
- CM
- AM
- LW/RW
- ST

The exact position taxonomy may evolve.

Position should affect:

- match opportunities
- tactical responsibilities
- attributes
- training priorities
- manager expectations
- career paths
- transfer opportunities

---

## 7. Player Attributes

Attributes represent relatively stable football ability.

Initial categories:

### Physical

- Pace
- Acceleration
- Stamina
- Strength
- Agility

### Technical

- Passing
- Shooting
- Dribbling
- Crossing
- First Touch
- Tackling

### Mental

- Vision
- Composure
- Positioning
- Decision Making
- Concentration

Attributes should be position-aware.

Not every attribute should have equal importance for every position.

---

## 8. Player State

Current state must be separate from permanent ability.

Examples:

- Form
- Confidence
- Fatigue
- Motivation
- Happiness
- Morale
- Fitness
- Manager Trust
- Reputation

A highly talented player can perform poorly because of state.

A less talented player can temporarily overperform because of excellent state.

---

## 9. Career Progression

Progression should have several dimensions.

### Football ability

Improves through:

- training
- match experience
- coaching
- age-related development
- specialized development

### Career status

Examples:

```text
Academy
Reserve
Squad Player
Rotation
Starter
Star
Captain
Legend
```

### Reputation

Represents how widely the player is recognized.

Reputation should affect:

- transfer interest
- sponsorships
- media attention
- social opportunities
- expectations

### Wealth

Represents financial success.

Wealth unlocks:

- housing
- vehicles
- lifestyle
- investments
- experiences

---

## 10. Match Experience

The player participates as one footballer.

The rest of the match is simulated.

The player should encounter contextual situations such as:

- receiving the ball
- passing
- through balls
- dribbling
- shooting
- crossing
- defending
- pressing
- positioning
- set pieces

The interaction should be fast and readable.

The player should not need to micromanage the entire team.

---

## 11. Training

Training is a strategic choice rather than a button that simply increases stats.

Training options include:

- Technical
- Physical
- Mental
- Position-specific
- Recovery

Training trades off:

- development
- fatigue
- match readiness
- injury risk
- time

The player should sometimes choose recovery over development.

---

## 12. Manager Relationship

Manager trust is a major progression system.

It should affect:

- starting opportunities
- substitutions
- playing time
- tactical role
- contract opportunities
- tolerance for poor performances

Manager trust should be influenced by:

- performance
- training
- professionalism
- decisions
- consistency
- position competition

---

## 13. Transfers

Transfers should feel like major career decisions.

A transfer opportunity should communicate:

- club
- league
- role
- expected playing time
- salary
- contract length
- club ambition
- competition
- location
- lifestyle implications

The player should consider:

> Is this actually better for my career?

not simply:

> Which club has the highest salary?

---

## 14. Contracts

Contracts define:

- salary
- length
- role
- bonuses
- release clauses where applicable
- club expectations

Contract negotiations can become narrative moments.

---

## 15. Personal Life

The personal-life layer is a major differentiator.

Main areas:

- Home
- Relationships
- Social life
- Finances
- Lifestyle
- Phone
- Media

Personal life should evolve with career success.

A young player might live in a small apartment.

A successful player may eventually have:

- luxury housing
- vehicles
- expensive lifestyle
- greater social attention
- more complex relationships

---

## 16. Relationships

Relationships can include:

- partner
- family
- friends
- teammates
- manager
- agent

Relationships should have:

- relationship strength
- recent interactions
- context
- needs
- events
- consequences

Avoid reducing relationships to a single visible number.

Numbers may exist internally, but the player should experience relationships through interactions and narrative.

---

## 17. Life Events

Life events create decisions.

Examples:

- teammate invites you out
- family asks for help
- agent presents an opportunity
- partner wants to travel
- media asks for an interview
- sponsor makes an offer
- manager gives advice
- teammate conflict
- unexpected financial expense

Each event should have:

- conditions
- choices
- effects
- narrative

Events should be context-sensitive.

---

## 18. Economy

The player's economy includes:

### Income

- salary
- match bonuses
- performance bonuses
- sponsorships
- other opportunities

### Expenses

- housing
- transportation
- lifestyle
- social activities
- optional purchases

Money should influence choices without becoming tedious accounting.

---

## 19. Home

Home is a visual representation of lifestyle progression.

Possible progression:

```text
Small Apartment
    ↓
Modern Apartment
    ↓
Luxury Apartment
    ↓
House
```

Home can contain interactions:

- sleep
- recover
- train
- relax
- phone
- wardrobe
- leave home

The home should make career progression visually tangible.

---

## 20. Phone

The phone is a narrative and information hub.

Potential sections:

- Messages
- News
- Social Feed
- Agent
- Manager
- Teammates
- Partner
- Transfer Rumors

The phone can deliver events without interrupting the core game loop.

---

## 21. Media and Reputation

Media should react to the player's career.

Possible content:

- match reports
- transfer rumors
- interviews
- fan reactions
- social posts
- milestone stories

Later, AI-generated narrative may be used to present simulation events.

The simulation remains authoritative.

---

## 22. Season Structure

At the end of each season, provide a strong summary.

Show:

- appearances
- goals
- assists
- trophies
- average rating
- attribute progression
- market value
- manager trust
- major events

The season should feel like a completed chapter.

---

## 23. Long-Term Career

The game should support careers lasting many seasons.

Age should affect:

- development
- physical attributes
- experience
- opportunities
- retirement probability

The world should continue evolving around the player.

New players emerge.

Players retire.

Managers change.

Clubs change.

Competitions change.

---

## 24. Game Modes

Initial priority:

### Career Mode

The main experience.

Later possibilities:

### Challenge Mode

Special scenarios with predefined conditions.

### Sandbox

Custom starting conditions.

### Career Replay

Potentially replay a career from a deterministic seed.

Do not implement future modes until the core career loop is compelling.

---

## 25. UX Philosophy

The player should always feel:

> Something is happening.

Avoid making the game primarily menus.

Menus should lead to decisions, events, matches or progression.

The UI should prioritize:

- current context
- next action
- meaningful consequences

---

## 26. Visual Direction

Target:

- premium mobile sports game
- modern football aesthetics
- cinematic match presentation
- clean information architecture
- strong player identity
- visually evolving lifestyle

Avoid:

- spreadsheet-heavy interfaces
- generic management-game appearance
- excessive UI clutter
- visual complexity without gameplay value

---

## 27. AI / Narrative

AI can eventually generate:

- news
- dialogue
- social posts
- interview responses
- contextual descriptions

AI must not directly mutate authoritative game state.

Correct architecture:

```text
Simulation
    ↓
Structured Event
    ↓
Narrative Generator
    ↓
Presentation
```

Incorrect:

```text
LLM
 ↓
Game State
```

---

## 28. Product Success Criteria

The core experience succeeds if a player regularly thinks:

- "I need to make this decision."
- "I want to see what happens next."
- "I can't believe I got that transfer."
- "I need to recover before the next match."
- "I should probably stay at this club."
- "That was the turning point in my career."

The game should create stories players remember.
