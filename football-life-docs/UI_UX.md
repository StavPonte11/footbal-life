# Football Life — UI/UX Design

## 1. UX Vision

The interface should communicate:

> This is my footballer's life.

Not:

> I am managing a football database.

The UI should create context, anticipation and decisions.

---

## 2. Platform

Primary:

- iOS
- Android
- portrait-first

Gameplay may switch to landscape if appropriate.

Reference design frame:

```text
390 × 844
```

---

## 3. Navigation

Initial navigation:

```text
Home
Career
Football
Life
Profile
```

The navigation can evolve as the product becomes clearer.

---

## 4. Home

Home is the daily hub.

Show:

- date
- player
- club
- position
- overall
- money
- form
- fatigue
- next match
- current events
- primary actions

The player should immediately know:

> What is happening today?

---

## 5. Career

Show:

- current club
- squad status
- manager trust
- contract
- salary
- market value
- career statistics
- trophies
- career timeline

Career should feel like a story.

---

## 6. Football

Football area includes:

- next match
- match preview
- team context
- objectives
- training
- match history
- statistics

---

## 7. Life

Life area includes:

- home
- relationships
- finances
- lifestyle
- phone
- social activities
- life events

---

## 8. Profile

Profile shows:

- player identity
- attributes
- current state
- position
- potential
- development
- career identity

Separate:

```text
ABILITY
```

from:

```text
CURRENT STATE
```

---

## 9. Match Preview

Show:

- opponent
- competition
- stadium
- starting status
- expected role
- manager expectations
- team form
- match objective

Primary CTA:

```text
PLAY MATCH
```

---

## 10. Training

Show:

- current attributes
- training categories
- expected improvement
- energy cost
- fatigue
- weekly schedule

Training should make the tradeoff visible.

---

## 11. Life Hub

Life should feel visually distinct from football without becoming a separate game.

Sections:

```text
Home
Relationships
Finances
Lifestyle
Phone
Social
```

---

## 12. Relationships

Show people as people.

Each relationship can show:

- avatar
- name
- relationship context
- recent interaction
- status
- available interaction

Avoid exposing every relationship as a raw numeric value.

---

## 13. Life Events

Life events should be immersive.

Structure:

```text
Character
 ↓
Situation
 ↓
Context
 ↓
Choices
```

Choices should communicate personality and intent.

Do not expose every hidden consequence.

---

## 14. Finances

Show:

- balance
- income
- expenses
- recent transactions
- assets
- lifestyle

Keep it game-like.

Avoid excessive accounting complexity.

---

## 15. Transfer Market

Show opportunities in a decision-oriented way.

Each offer should communicate:

- club
- role
- salary
- contract
- competition
- club ambition
- location
- career upside

Do not reduce the decision to salary.

---

## 16. Phone

Phone is an information and narrative hub.

Sections:

- Messages
- News
- Social
- Agent
- Manager
- Teammates
- Partner

Phone notifications should create anticipation.

---

## 17. Home

Home is a visual lifestyle progression system.

Possible interactions:

```text
Bed → Rest
Gym → Train
Phone → Social
Wardrobe → Customize
TV → Relax
Door → Go out
```

Home quality should visually reflect career progression.

---

## 18. Season Summary

End-of-season screen should feel celebratory and reflective.

Show:

- appearances
- goals
- assists
- trophies
- overall progression
- market value
- manager trust
- major moments

Include a timeline of memorable events.

---

## 19. Visual Design

Direction:

- dark premium base
- modern typography
- restrained accent color
- subtle gradients
- strong imagery
- large numbers
- clean cards
- strong hierarchy

Avoid:

- spreadsheet aesthetics
- tiny text
- excessive badges
- unnecessary decoration

---

## 20. Interaction

Every screen should have:

- clear primary action
- logical secondary actions
- obvious current context

Touch targets must be comfortable.

---

## 21. Reusable Components

Create a consistent component library.

Examples:

```text
PlayerHeader
PlayerAvatar
RatingBadge
AttributeBar
StatCard
MatchCard
EventCard
RelationshipCard
ClubCard
FinanceCard
TimelineItem
PrimaryButton
SecondaryButton
BottomNavigation
Notification
```

---

## 22. Design Principle

The UI should always answer:

```text
Where am I?
What is happening?
What can I do?
What happens next?
```

If a screen cannot answer these questions clearly, simplify it.
