# CardsGame

A 2D card-based roguelike (Slay the Spire-style) where a player builds a deck, enters turn-based combat against enemies, and progresses through a node map.

## Language

### Combat

**Battle**:
A single combat encounter with one player and one or more enemies. Started explicitly by passing a player and an enemy list to `BattleManager`. Ends when all enemies die (Victory) or the player dies (Defeat).
_Avoid_: Fight, combat session, encounter

**Turn**:
One unit of action belonging to a single side (player or enemy). A player Turn begins with drawing cards and ends when the player clicks "End Turn"; an enemy Turn follows immediately after.
_Avoid_: Round (a round = one PlayerTurn + one EnemyTurn)

**Round**:
One full cycle of PlayerTurn + EnemyTurn.
_Avoid_: Turn (ambiguous — always qualify as PlayerTurn or EnemyTurn)

**PlayerTurn**:
The phase of a Round where the player draws cards, spends Energy, and plays cards. Ends when the player explicitly ends the turn.

**EnemyTurn**:
The phase of a Round where each enemy executes its planned action. Ends automatically once all enemies have acted.

**BattleResult**:
The outcome of a Battle: `Victory` or `Defeat`. Determined after all death effects fully resolve — **the side whose last character died determines the result**: if the last death in the chain was an enemy → Victory; if the last death was the LocalPlayer → Defeat. This means a player can die but still win if their death effect kills the last enemy.

**BattleState**:
The internal state of BattleManager: `Setup → PlayerTurn ⇄ EnemyTurn → End`. `CheckResult` is not a state — it is a private method called after every death and after every EnemyTurn.

**Round**:
One full cycle of PlayerTurn + EnemyTurn.
_Avoid_: Turn (ambiguous — always qualify as PlayerTurn or EnemyTurn)

### Cards

**Hand**:
The set of cards currently playable by the player. Drawn at the start of each PlayerTurn; discarded at the end.

**DrawPile** (DrawCards in code):
The face-down pile a player draws from. Refilled by shuffling the DiscardPile when empty.

**DiscardPile** (DiscardCards in code):
Cards that have been played or discarded at end of turn.

**Exhaust**:
A card removed from the Battle entirely (sent to DeleteCards). Cannot be drawn again this Battle.

### Characters

**LocalPlayer**:
The single human-controlled character. Distinct from enemies. In the future (multiplayer), this will refer to the character controlled by this client specifically.
_Avoid_: Player (too ambiguous in multiplayer context — prefer LocalPlayer)

**Enemy**:
An AI-controlled character that executes a planned action each EnemyTurn.

**EnemyIntent**:
The action an enemy has announced it will take next turn (type + value). Planned at the start of PlayerTurn so the player can see it while deciding; executed at the start of EnemyTurn.

**EnemyController** (`IEnemyController`):
The AI logic attached to an enemy CharacterInstance. Implements `PlanNextAction()` and `ExecuteAction()`. `null` on the LocalPlayer (player-controlled). Phase 1 uses `SimpleEnemyController` (fixed attack every turn).

### Resources

**Energy**:
The resource spent to play cards. Refills to max at the start of each PlayerTurn.

**Block**:
Temporary armor that absorbs incoming damage before HP is reduced. Each character's Block is cleared at the start of **their own side's turn** — the player's Block clears at the start of PlayerTurn; each enemy's Block clears at the start of EnemyTurn.
_Avoid_: Shield, armor

## Relationships

- A **Battle** has exactly one **LocalPlayer** and one or more **Enemies**
- A **Round** consists of one **PlayerTurn** followed by one **EnemyTurn**
- A **Hand** is drawn at the start of each **PlayerTurn** and discarded at the end
- Each **Enemy** announces one **EnemyIntent** per Round

## Key timing rules

- **Block clears at the start of the owning side's turn**: player Block clears when PlayerTurn starts; each enemy's Block clears when EnemyTurn starts (before that enemy acts).
- **EnemyIntent is planned at the start of PlayerTurn**: enemies announce their next action at the top of PlayerTurn so the player can see it while deciding; enemies execute it at the start of EnemyTurn.
- **CardManager binds to LocalPlayer's hand when a Battle starts** (via `BattleManager.StartBattle()`), not at game boot.

## Flagged ambiguities

- "Player" was used in code (`GameManager.Player`) to mean LocalPlayer — to be renamed `LocalPlayer` in Phase 1 refactor.
- "Round" vs "Turn": in this codebase, a Turn belongs to one side; a Round = PlayerTurn + EnemyTurn. Never use "Turn" without qualifying which side.
