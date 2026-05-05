# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**CardsGame** is a 2D card-based roguelike game built with Unity (similar to Slay the Spire). Players build decks, draw cards each turn, and use them in turn-based combat against enemies. The core loop involves managing energy resources, playing cards with various effects (damage, healing, armor), and managing deck composition.

### Tech Stack
- **Engine**: Unity (2022+)
- **Language**: C#
- **Graphics**: Universal Render Pipeline (2D)
- **Data Format**: ScriptableObjects for card and character definitions
- **Architecture**: Singleton pattern for managers, Event-driven UI updates

## Development Setup

### Opening the Project
1. Open Unity Hub and add the `CardsGame` folder
2. Open with Unity 2022.3 LTS or later
3. Open the scene at `Assets/Scenes/SampleScene.unity` to start developing

### Building & Running
- Press **Play** in the Unity Editor to test
- Build for platform: File > Build Settings > Select platform > Build
- Default resolution: 1920x1080 (configurable in ProjectSettings)

### Code Navigation
- **Game Logic**: `Assets/Manager/` - Core game systems
- **Card System**: `Assets/Cards/` - Card instances, deck control, UI
- **Character System**: `Assets/Player/` - Character data and UI
- **Data Definitions**: `Assets/ScriptableObject/` and `Assets/Resources/`
- **Utilities**: `Assets/Editor/` - Editor tools and helpers

## Architecture & Key Systems

### Singleton Manager Pattern
Three main manager classes coordinate gameplay:

1. **GameManager** (`Assets/Manager/GameManager.cs`)
   - Singleton entry point for the game
   - Initializes CardManager and CharacterManager
   - Handles UI button bindings (Start, Draw, TurnEnd, EnemyGenerate)
   - Stores references to canvas, card prefab, and UI elements
   - Property: `GameManager.Instance.Player` - the player character

2. **CardManager** (`Assets/Manager/CardManager.cs`)
   - Creates card instances from CardData (loaded from `Resources/CardDatas/`)
   - Manages hand UI layout and positioning
   - Handles card-to-target interactions via `TryPlayCard()`
   - Tracks drawn and discarded cards count for UI display
   - Subscribes to DeckController events to spawn card UI

3. **CharacterManager** (`Assets/Manager/CharacterManager.cs`)
   - Creates character instances from CharacterData (loaded from `Resources/CharacterDatas/`)
   - Spawns character UI prefabs for enemies
   - Maintains list of all characters

### Card System
Cards flow through distinct states managed by `DeckController`:

```
Deck → DrawCards (抽牌堆) → HandCards → [Played/Discarded] → DiscardCards → [Shuffle back when DrawCards empty] → DeleteCards (消耗)
```

**Key Classes:**

- **CardData** (ScriptableObject) - Immutable card definition
  - Properties: ID, Name, Cost, Type, Rarity, Effects, PostUseAction
  - Contains serialized list of ICardEffect implementations
  - Create via Assets > Create > Games > CardData

- **CardInstance** - Runtime card with lifecycle hooks
  - Holds reference to CardData and owner (CharacterInstance)
  - Lifecycle: `OnDrawing()` → `OnDrawed()` → `OnUsing()` → `OnUsed()` → `OnDiscarding()` → `OnDiscarded()`
  - `CanUse()` - validation hook (currently always true, extend for resource checks)
  - `CurrentCost` - can be modified by effects before playing

- **DeckController** - Manages all card piles
  - Public lists: `Deck`, `DrawCards`, `DiscardCards`, `HandCards`, `DeleteCards`
  - `DrawCard(count)` - draws from DrawCards, auto-shuffles DiscardCards if needed
  - `MoveToPostUse(card)` - routes card based on PostUseAction enum
  - `OnCardDrawn` event - fires when card enters hand (CardManager subscribes)
  - Fisher-Yates shuffle when refilling draw pile

- **ICardEffect** Interface - Effect system
  - Implementations: `DamageEffect`, `HealEffect`, `ShieldEffect`
  - All are `[System.Serializable]` for Inspector editing
  - Called via `effect.ExecuteEffect(card, source, target)` in CardManager.TryPlayCard()
  - Extend by creating new `[System.Serializable]` classes implementing ICardEffect

### Character System
Characters manage health, energy (mana), and decks:

**CharacterData** (ScriptableObject) - Immutable character definition
- Properties: CharacterName, MaxHP, BaseEnergy
- Create via Assets > Create > Games > CharacterData

**CharacterInstance** - Runtime character state
- Health system: `CurrentHP`, `TakeDamage()` (deducts armor first, then HP)
- Armor system: `CurrentBlock`, `AddBlock()`, `ClearBlock()` (typically cleared each turn)
- Energy system: `MaxEnergy`, `CurrentEnergy`, `ConsumeEnergy()` (checked before playing cards)
- Deck ownership: Each character has a `DeckController` via `CharacterInstance.DeckController`
- Events: `OnHPChanged`, `OnBlockChanged`, `OnDamageTaken`, `OnEnergyChanged`, `OnDeath`

**CharacterUI** - Binds to CharacterInstance events for visual updates
- Subscribes to character events and updates display

### Enums & Constants

**CardType**: Minion, Spell, Equipment, Field, Curse

**CardRarity**: Common, Uncommon, Rare, Epic, Legendary

**PostUseAction**: 
- `Discard` - returns to DiscardCards (standard)
- `Exhaust` - moved to DeleteCards (unavailable this battle)
- `RemoveFromGame` - removed entirely (rare)

## Common Development Tasks

### Adding a New Card
1. Create CardData: Right-click in `Assets/Resources/CardDatas/` > Create > Games > CardData
2. Set properties (Name, Cost, Type, Rarity, etc.)
3. Add effects by clicking "+" in Effects list and selecting effect type
4. Test by calling `GameManager.Instance.cardManager.GenerateCardByString("YourCardName")`

### Adding a New Effect Type
1. Create a new `[System.Serializable]` class in `Assets/Cards/ICardEffect.cs`
2. Implement `ICardEffect` interface with `ExecuteEffect(card, source, target)` method
3. Add public fields for parameters (damage, heal amount, etc.)
4. Effects automatically appear in CardData Inspector once class is defined

### Adding a New Character
1. Create CharacterData: Right-click in `Assets/Resources/CharacterDatas/` > Create > Games > CharacterData
2. Set CharacterName, MaxHP, BaseEnergy
3. Create corresponding CharacterUI prefab (duplicate existing or create new)
4. Assign UI prefab to GameManager's `enemyCharacterUIPrefab` field

### Playing a Card (Existing Flow)
CardUI.OnClick → CardManager.TryPlayCard() → Check energy → Execute effects → MoveToPostUse() → RemoveCardUI()

### Testing Gameplay
- Click "Start" button to initialize player deck with BaseAttack and BaseShield cards
- Click "Draw" to draw more cards (currently a placeholder)
- Click "Enemy Generate" to create an enemy character with UI
- Click cards to play them (targeting UI coming soon)
- Click on target to execute card effects

## Important Patterns & Conventions

### Binding & References
- Cards bind to CharacterInstance via `BindCharacterInstance()`
- CardUI binds to CardInstance and CardManager for callbacks
- CharacterUI binds to CharacterInstance for event subscriptions
- Pattern: Manager creates instance, prefab's component gets Setup/Bind() calls

### Resource Loading
- Cards: `Resources.Load<CardData>("CardDatas/BaseAttack")`
- Characters: `Resources.Load<CharacterData>("CharacterDatas/Player")`
- Always check for null after load (logs warning if not found)

### Event-Driven UI
- CharacterInstance fires events (OnHPChanged, OnEnergyChanged, etc.)
- CharacterUI subscribes in `Setup()` method
- CardManager subscribes to DeckController.OnCardDrawn to spawn card UI

### Serialization
- All game data uses ScriptableObjects (not JSON/serialization)
- Effects are `[SerializeReference]` to support polymorphic serialization in CardData
- Meta files (.meta) are auto-generated; don't commit changes

## Common Gotchas

1. **Hand Size Limit**: DeckController enforces 10-card hand max; excess cards auto-discard
2. **PostUseAction**: Default is Discard; Exhaust moves to DeleteCards (consumed this battle)
3. **Energy Reset**: Currently not reset between turns; implement in TurnEnd() if needed
4. **Block Clears**: Currently not auto-cleared between turns; call `ClearBlock()` in TurnEnd()
5. **Card Cost Modification**: Modify `CardInstance.CurrentCost` before calling `TryPlayCard()` to implement cost reductions
6. **Effect Order**: Effects execute in list order in CardData; order matters for dependency effects

## Asset Organization

```
Assets/
├── Manager/           # Singleton managers (GameManager, CardManager, CharacterManager)
├── Cards/            # Card logic (CardInstance, DeckController, CardUI, ICardEffect, CardData)
├── Player/           # Character logic (CharacterInstance, CharacterUI, CharacterData)
├── ScriptableObject/ # Data definitions (CardData, CharacterData)
├── Resources/        # Runtime-loaded assets
│   ├── CardDatas/           # Card definitions (BaseAttack.asset, BaseShield.asset)
│   └── CharacterDatas/      # Character definitions (Player.asset, Enemy1.asset)
├── Scenes/           # Game scenes (SampleScene.unity)
└── Editor/           # Editor tools (CardDataEditor)
```

## Future Work (TODO Items)

- Card cost caching in CardManager for performance
- Turn-end energy reset and block clearing
- Targeting system for cards (currently no validation)
- Card and enemy animation system
- Pause/resume game state
- Game over condition (character death)
- Additional card types and effects
- AI for enemy turn logic
- Save/load game state
- Audio system integration
- Balancing and playtesting

## Code Style Notes

- Classes use PascalCase (GameManager, CardInstance)
- Private fields prefixed with `_` (underscore)
- Use `=> null` pattern for singleton instantiation fallback
- Comments in Chinese (game was developed in Chinese-speaking environment)
- Include region markers (#region) for logical code grouping
- XML docs on public methods and important parameters

## Agent skills

### Issue tracker

Issues live in GitHub Issues for `zero300/CardGame`. See `docs/agents/issue-tracker.md`.

### Triage labels

Uses the five canonical label strings without modification. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context repo — one `CONTEXT.md` + `docs/adr/` at the repo root. See `docs/agents/domain.md`.
