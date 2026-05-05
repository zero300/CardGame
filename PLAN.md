# 優化方向決策：先做可玩流程，過程中做關鍵解耦

## Context

**現況**：專案目前只有測試按鈕（生成卡牌、生成敵人、結束回合），沒有真正的回合系統、勝負判定、地圖或事件流程。雖然 `CharacterInstance` 與 `DeckController` 的抽象做得不錯（每個角色都自帶 deck，卡片也記得 owner），但 `GameManager.Player`、`CardManager._player` 仍綁死「只有一個玩家」的假設，UI 階層也是 hardcode `Find("Hand")`、`Find("PlayerSpace")`。

**使用者目標**：
1. 短期：先做可玩 demo（從地圖選路 → 進戰鬥 → 結束 → 回地圖）。
2. 中期：Slay the Spire 風格節點地圖，含戰鬥/升級/休息等事件。
3. 長期：線上多人連線，多玩家共打同一隻敵人，但只顯示自己的手牌。

## 結論：優化順序建議

**先做可玩流程（方向 2），但在過程中順手做網路多人友善的解耦。** 不要先做純架構重構（方向 1 的多人化）。

## 實作藍圖

### 階段一：戰鬥系統骨架（1～2 週）

目標：能完整跑完一場戰鬥（玩家回合 → 敵人回合 → 勝負判定）。

**新增 `BattleManager`**（`Assets/Manager/BattleManager.cs`）
- 戰鬥狀態機：`Setup` → `PlayerTurn` → `EnemyTurn` → `CheckResult` → `End`
- 進入戰鬥時接收「玩家 + 敵人列表」（為多角色預留）
- 觸發回合事件：`OnTurnStart`、`OnTurnEnd`、`OnBattleEnd`

**改寫 `GameManager`**
- 移除 hardcoded 測試按鈕的卡牌生成邏輯
- `GameManager.Player` 仍保留（短期讓 demo 能跑），但 BattleManager 內部不直接用它
- 拆出 `RunMode` 枚舉：`MapView` / `Battle` / `Event` / `Reward`

**回合邏輯**（補完 `CharacterInstance.TurnEnd()`、`DeckController.TurnEnd()`）
- 回合開始：能量回滿、清除護甲、抽 5 張
- 回合結束：剩餘手牌進棄牌堆（或標記 retain）
- 敵人回合：執行 AI（先寫最簡單的「每回合固定攻擊」）

**敵人 AI 與意圖**
- `EnemyIntent` 結構：下回合行動類型 + 數值
- `EnemyController` 介面：`PlanNextAction()` 與 `ExecuteAction()`
- CharacterUI 顯示意圖圖示（之後做）

**勝負判定**
- `CharacterInstance.OnDeath` 已存在，BattleManager 訂閱
- 全部敵人死亡 → `BattleResult.Victory`
- 玩家死亡 → `BattleResult.Defeat`

**關鍵解耦（為網路多人鋪路）**
- `CardManager.TryPlayCard()` 已經接 `CardUI.GetCardOwner()`，這部分 OK
- 把 `CardManager._player` getter 移除，改為 `BindLocalPlayerHand(CharacterInstance localPlayer)`，只用於「決定要顯示哪個角色的手牌 UI」
- `SetDrawCount()`、`SetDiscardCount()` 改成接收 `DeckController` 參數
- `GameManager.Player` 改名為 `LocalPlayer`，明確表達語意（為線上多人做準備）

### 階段二：地圖與事件系統（1～2 週）

目標：能從地圖選擇節點，進入對應事件，結束後回到地圖。

**新增 `MapManager`**（`Assets/Manager/MapManager.cs`）
- 預先生成節點圖（Slay the Spire 風格，每層 2～4�個節點，玩家從底部往上選路）
- 節點類型：`Combat`、`Elite`、`Rest`、`Shop`、`RandomEvent`、`Boss`
- 維護玩家當前所在節點 + 可前往節點清單
- 用 ScriptableObject 定義「層數結構」（方便未來調整節奏）

**新增 `EventManager`**（`Assets/Manager/EventManager.cs`）
- 派發節點對應的事件處理器
- `Combat` → 啟動 BattleManager
- `Rest` → 顯示休息 UI（回 30% HP 或升級一張卡）
- `Shop` → 顯示商店 UI（買卡/移除卡）
- `RandomEvent` → 從 ScriptableObject 隨機抽事件腳本
- 事件結束後通知 MapManager 玩家可以繼續選路

**地圖 UI**
- 新建 `MapPanel`（與 `GamePanel` 平行）
- 節點按鈕生成：根據 MapManager 的節點資料動態產生
- 玩家位置高亮、可選節點高亮、不可選節點灰階
- 切換 Panel：`GameManager.SetMode(RunMode.MapView)` 顯示 MapPanel、隱藏 GamePanel

**升級/休息事件**
- `CardInstance` 增加 `Upgraded` 屬性，CardData 增加 `UpgradedVersion` 欄位（指向另一張 CardData）
- 休息事件：選擇「治療」或「升級一張手中卡」
- 升級時把 `DeckController.Deck` 中的 CardInstance 替換成升級版

**Run 結構**
- 新增 `RunState` 類別：保存玩家當前 deck、當前 HP、當前金錢、當前地圖位置
- 戰鬥結束後 HP/deck 回寫到 RunState
- 新一場 Run 開始時重置 RunState

### 階段三（不在本計畫，預留）：網路多人

階段一、二做好後，網路化主要工作會落在：
- 引入 Mirror 或 Unity Netcode for GameObjects
- `CharacterInstance` 加 `NetworkBehaviour` 包裝、狀態 SyncVar
- BattleManager 改為伺服器權威（卡牌效果在伺服器執行）
- CardManager 只渲染 `IsLocalPlayer` 的手牌，其他玩家只顯示角色狀態列
- 因為階段一已經做了解耦，這時改的是「橫切面」（哪個流程在 server / client 跑），而不是「翻新地基」。

## 關鍵檔案

**會修改**：
- [Assets/Manager/GameManager.cs](Assets/Manager/GameManager.cs) — 拆出 RunMode、改名 Player → LocalPlayer
- [Assets/Manager/CardManager.cs](Assets/Manager/CardManager.cs) — 移除 `_player` getter，改為 `BindLocalPlayerHand()`
- [Assets/Manager/CharacterManager.cs](Assets/Manager/CharacterManager.cs) — 補上 `RemoveCharacter()`、`GetAliveEnemies()`
- [Assets/Player/CharacterInstance.cs](Assets/Player/CharacterInstance.cs) — 補完 `TurnEnd()`（能量回復、清護甲）
- [Assets/Cards/DeckController.cs](Assets/Cards/DeckController.cs) — 補完 `TurnEnd()`（手牌進棄牌堆）

**會新增**：
- [Assets/Manager/BattleManager.cs](Assets/Manager/BattleManager.cs) — 戰鬥狀態機
- [Assets/Manager/MapManager.cs](Assets/Manager/MapManager.cs) — 節點地圖
- [Assets/Manager/EventManager.cs](Assets/Manager/EventManager.cs) — 節點事件派發
- [Assets/AI/EnemyIntent.cs](Assets/AI/EnemyIntent.cs) — 敵人下回合意圖
- [Assets/AI/EnemyController.cs](Assets/AI/EnemyController.cs) — 敵人 AI 介面
- [Assets/Run/RunState.cs](Assets/Run/RunState.cs) — 一場 Run 的玩家狀態
- [Assets/ScriptableObject/MapLayoutData.cs](Assets/ScriptableObject/MapLayoutData.cs) — 地圖層數設定
- [Assets/ScriptableObject/RandomEventData.cs](Assets/ScriptableObject/RandomEventData.cs) — 隨機事件設定

## 可重用的既有設計

不要重寫，繼續沿用：
- `CharacterInstance.OnDeath` 事件 — BattleManager 訂閱即可
- `CharacterInstance.OnHPChanged`、`OnEnergyChanged` 事件 — UI 已經訂閱
- `DeckController.OnCardDrawn` — CardManager 已訂閱
- `ICardEffect.ExecuteEffect(card, source, target)` — 已經接收 source/target，多目標只需擴成 list
- `PostUseAction` 路由（Discard/Exhaust/Remove）— 已實作
- `[SerializeReference]` 多型效果序列化 — 已驗證可行

## 驗證方式

### 階段一完成判準
1. 在 Unity 中按 Play → 進入戰鬥 → 出 5 張手牌 → 玩家回合可打牌、消耗能量
2. 點「結束回合」→ 敵人執行行動 → 玩家受到傷害
3. 連續打牌直到敵人死亡 → 跳出勝利 UI
4. 故意被打死 → 跳出失敗 UI

### 階段二完成判準
1. 開始遊戲 → 顯示節點地圖 → 玩家從底部選擇第一個節點
2. 進入 Combat 節點 → 完成戰鬥 → 回到地圖 → 上層節點變成可選
3. 進入 Rest 節點 → 顯示治療/升級選擇 → 套用後回地圖
4. 走到 Boss 節點 → 通關後顯示 Run 結束畫面

### 不在範圍
- 卡牌動畫、音效、特效
- 平衡性調整
- 多人連線（階段三另開計畫）
- 存檔/讀檔
