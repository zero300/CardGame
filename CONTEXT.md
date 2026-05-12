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

### Run & Map

**Run**:
One complete playthrough from the starting node to Boss death (Victory) or LocalPlayer death (Defeat), spanning multiple Nodes. HP and deck composition persist across Battles within a Run.
_Avoid_: Game, session

**Floor**:
One horizontal layer of the node map. Each Floor contains 2–4 Nodes. The player progresses upward Floor by Floor, choosing one Node per Floor to enter.

**Node**:
A single event point on the map. Has a type that determines what happens when the player enters it. Phase 2 implements: `Combat`, `Rest`, `Boss` (fully); `Elite`, `Shop`, `RandomEvent` (stub).
_Avoid_: Room, tile, cell

**NodeType**:
The category of a Node: `Combat`, `Elite`, `Rest`, `Shop`, `RandomEvent`, `Boss`.

**RunMode**:
The current screen/phase of a Run: `MapView`, `Battle`, `Event`, `Reward`. Owned and transitioned by `RunManager`. Drives which Panel UIManager shows.
_Avoid_: GameMode, GameState

**RunState**:
The run-scoped persistent data for the current Run. In Phase 2 contains only `currentNodeId`. HP and deck are read directly from `LocalPlayer` (which persists for the whole Run via `DontDestroyOnLoad`). Gold and other fields added in future phases.

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

- A **Run** consists of multiple **Floors**, each containing multiple **Nodes**
- A **Battle** has exactly one **LocalPlayer** and one or more **Enemies**
- A **Round** consists of one **PlayerTurn** followed by one **EnemyTurn**
- A **Hand** is drawn at the start of each **PlayerTurn** and discarded at the end
- Each **Enemy** announces one **EnemyIntent** per Round
- **RunState** tracks the LocalPlayer's position (`currentNodeId`) across Nodes; HP and deck are owned by `LocalPlayer` directly

## Manager responsibilities (Phase 2)

| Manager | Owns | Does NOT own |
|---|---|---|
| `RunManager` | `RunMode`, `RunState` | Map structure, UI visibility |
| `MapManager` | Node graph, Floor layout, reachable nodes | Player position (stored in RunState) |
| `EventManager` | Event handler dispatch per NodeType | Run state, UI |
| `UIManager` | Panel show/hide per RunMode | RunMode transitions |
| `BattleManager` | Battle state machine | Run persistence |

## Key timing rules

- **Block clears at the start of the owning side's turn**: player Block clears when PlayerTurn starts; each enemy's Block clears when EnemyTurn starts (before that enemy acts).
- **EnemyIntent is planned at the start of PlayerTurn**: enemies announce their next action at the top of PlayerTurn so the player can see it while deciding; enemies execute it at the start of EnemyTurn.
- **CardManager binds to LocalPlayer's hand when a Battle starts** (via `BattleManager.StartBattle()`), not at game boot.
- **LocalPlayer persists for the whole Run**: `CharacterInstance` is not recreated between Battles. HP and deck on `LocalPlayer` are the live run state; RunState reads from them, does not duplicate.
- **Map is generated once per Run**: `MapManager` generates the node graph at Run start using `MapLayoutData` rules; the same map instance is used for the entire Run.

## Flagged ambiguities

- "Player" was used in code (`GameManager.Player`) to mean LocalPlayer — renamed `LocalPlayer` in Phase 1 ✅
- "Round" vs "Turn": in this codebase, a Turn belongs to one side; a Round = PlayerTurn + EnemyTurn. Never use "Turn" without qualifying which side.
