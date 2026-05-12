# PRD：Phase 2 — 地圖與事件系統

標籤：`needs-triage`

---

## Problem Statement

Phase 1 完成了完整的單場戰鬥流程（玩家回合 → 敵人回合 → 勝負判定），但每次遊戲都是孤立的一場戰鬥，沒有跨戰鬥的進展感：HP 不保留、牌組不累積、沒有選擇路線的決策，也沒有「通關一場 Run」的終點。玩家無法體驗 roguelike 的核心樂趣。

---

## Solution

實作 Slay the Spire 風格的節點地圖系統。玩家從地圖底部出發，逐層選擇節點（Combat、Rest、Boss），HP 與牌組在節點間持久保留。贏得 Boss 戰或玩家死亡時結束本場 Run，回到可重新開始的狀態。

同步建立四個新 Manager（`RunManager`、`UIManager`、`MapManager`、`EventManager`）並透過 `ServiceLocator` 統一管理，讓各系統職責清晰、可獨立擴充。

---

## User Stories

### 地圖瀏覽

1. 作為玩家，我希望在每場 Run 開始時看到一張節點地圖，以便決定前進路線
2. 作為玩家，我希望地圖每場 Run 都不同，以便遊戲有重玩價值
3. 作為玩家，我希望地圖從底部往上排列，最底層是起始節點、最頂層是 Boss，以便直觀理解進度方向
4. 作為玩家，我希望看到哪些節點可以從當前位置進入（高亮顯示），以便知道我的選擇
5. 作為玩家，我希望看到已拜訪的節點以不同顏色標記，以便追蹤走過的路徑
6. 作為玩家，我希望無法選擇不可到達的節點，以便避免操作錯誤
7. 作為玩家，我希望每層只能選擇一個節點前進，以便保持路線選擇的張力

### Combat 節點

8. 作為玩家，我希望進入 Combat 節點後自動啟動戰鬥，以便無縫銜接地圖與戰鬥
9. 作為玩家，我希望戰鬥結束（Victory）後自動回到地圖，以便繼續選擇下一個節點
10. 作為玩家，我希望戰鬥後的 HP 保留到下一個節點，以便感受到資源管理的壓力
11. 作為玩家，我希望戰鬥後的牌組（含 Buff）保留到下一場戰鬥，以便牌組建構有跨戰鬥意義

### Rest 節點

12. 作為玩家，我希望進入 Rest 節點時看到兩個選擇：「治療」或「升級卡牌」，以便根據當前狀況做出決策
13. 作為玩家，我希望選擇治療時回復 30% 最大 HP，以便在危險時補血
14. 作為玩家，我希望選擇升級時能從整副牌組中選擇一張升級，以便強化關鍵卡牌
15. 作為玩家，我希望看到每張卡牌升級後的效果預覽，以便做出有根據的選擇
16. 作為玩家，我希望升級後的卡牌在後續戰鬥中保持升級狀態，以便升級具有持久意義

### Boss 節點

17. 作為玩家，我希望 Boss 固定在地圖最頂層，以便知道終點在哪裡
18. 作為玩家，我希望擊敗 Boss 後看到 Run 勝利畫面，以便感受到通關的成就感
19. 作為玩家，我希望 Run 結束後可以選擇開始新的一場 Run，以便繼續遊戲

### 玩家死亡

20. 作為玩家，我希望在任何節點死亡時立即顯示失敗畫面並結束本場 Run，以便明確知道 Run 已結束
21. 作為玩家，我希望失敗後可以開始新的一場 Run，以便重新嘗試

### Stub 節點

22. 作為玩家，我希望看到 Elite、Shop、RandomEvent 節點在地圖上存在，以便感受到系統的完整性
23. 作為玩家，進入 Stub 節點時看到「即將推出」提示並自動返回地圖，以便遊戲不崩潰

### 面板管理

24. 作為玩家，我希望在地圖與戰鬥之間切換時，只看到當前對應的 UI，以便介面整潔不混亂

---

## Implementation Decisions

### 新增 Manager 架構

**RunManager**
- 持有 `RunMode` 枚舉（`MapView / Battle / Event / Reward`）與狀態轉換邏輯
- 持有 `RunState`（目前只含 `currentNodeId`）
- 訂閱 `IBattleManager.OnBattleEnd`，在 Victory 時回到 MapView，在 Defeat 時結束 Run
- 訂閱 `MapManager.OnNodeSelected`，更新 `currentNodeId` 並通知 `EventManager`
- HP 與牌組直接從 `LocalPlayer` 讀取，不在 RunState 複製（見 ADR-0003）

**UIManager**
- 職責唯一：依當前 `RunMode` 顯示對應 Panel，隱藏其餘所有 Panel
- 持有所有 Panel 的 GameObject 參照（MapPanel、GamePanel、VictoryPanel、DefeatPanel、RestPanel、StubPanel）
- 對外只暴露 `ShowMode(RunMode)` 介面
- `Bootstrapper` 將 Panel 參照注入 UIManager，不再自行持有

**MapManager**
- 在 Run 開始時依 `MapLayoutData` ScriptableObject 的規則程序生成節點圖
- 維護所有節點的狀態（未訪問 / 當前 / 已訪問 / 不可達）
- 計算並暴露可從當前節點到達的節點集合
- 發出 `OnNodeSelected(nodeId)` 事件；`RunManager` 訂閱
- 不追蹤玩家位置——玩家位置由 `RunState.currentNodeId` 持有，MapManager 接受外部傳入

**EventManager**
- 持有各 NodeType 對應的處理邏輯入口
- `Combat` / `Boss`：呼叫 `IBattleManager.StartBattle()` 並傳入對應敵人
- `Rest`：通知 UIManager 顯示 RestPanel，等待玩家選擇後執行治療或升卡
- `Elite` / `Shop` / `RandomEvent`：顯示 StubPanel 並在短暫延遲後返回地圖
- 完成後發出 `OnEventCompleted` 事件；`RunManager` 接收並轉換 RunMode

### 新增 ScriptableObject

**MapLayoutData**
- `floorCount`：地圖總層數
- `nodesPerFloor`：每層節點數範圍（min / max）
- `nodeTypeWeightsByFloor`：每層各 NodeType 的生成權重（最後一層強制為 Boss）

**CardData 新增 UpgradedVersion 欄位**
- 型別為 `CardData`（指向升級後版本的 ScriptableObject）
- `null` 表示此卡不可升級
- Rest 節點升級時：將 `DeckController.Deck` 中選中的 `CardInstance.baseCardData` 替換為 `UpgradedVersion`

### LocalPlayer 生命週期

`LocalPlayer`（`CharacterInstance`）整場 Run 是同一個物件，不在節點間重建。戰鬥開始前呼叫 `DeckController.BattleStart()` 重置戰鬥堆（DrawCards / HandCards / DiscardCards），但 `Deck` 不變。

### Run 開始與結束

- **Run 開始**：`RunManager.StartRun()` 呼叫 `MapManager.GenerateMap()`，重置 `RunState`，重置 `LocalPlayer` HP 至 MaxHP，清空並重新初始化牌組（由 Bootstrapper 或選單系統提供初始牌組），切換 RunMode 為 `MapView`
- **Run 結束**：顯示 VictoryPanel 或 DefeatPanel；提供「再玩一次」按鈕重新呼叫 `RunManager.StartRun()`

### Bootstrapper 調整

- 新增對 `RunManager`、`UIManager`、`MapManager`、`EventManager` 的初始化與 ServiceLocator 註冊
- 移除 `victoryPanel`、`defeatPanel` 的直接參照，改由 UIManager 管理
- 保留 `LocalPlayer` 的建立（整場 Run 生命週期不變）

---

## Testing Decisions

- 測試原則：只驗證對外行為（public 介面與事件），不測試私有方法或具體實作細節
- 優先測試深模組（MapManager 的生成邏輯、RunManager 的狀態轉換）

**MapManager**（優先，邏輯可脫離 Unity 測試）
- 生成的地圖 Floor 數量符合 MapLayoutData 設定
- 每層節點數在 min/max 範圍內
- 最後一層只有 Boss 節點
- 從起始節點出發，每個節點都可達（連通性）
- `GetReachableNodes(nodeId)` 只回傳直接相連的上層節點

**RunManager 狀態機**
- 初始狀態為 `MapView`
- OnNodeSelected → 正確轉換到對應 RunMode
- OnBattleEnd(Victory) → 回到 MapView
- OnBattleEnd(Defeat) → 顯示 DefeatPanel

**相關既有 Pattern**：參照 Phase 1 `BattleManager` 的狀態機邏輯；目前專案無自動化測試，Phase 2 如有需要以 Unity Test Runner 建立 EditMode 測試。

---

## Out of Scope

- 商店系統（金錢、購買卡牌、移除卡牌）
- Elite 與 RandomEvent 節點的完整實作
- 卡牌動畫、音效、特效
- 存檔 / 讀檔（Save / Load）
- 網路多人（Phase 3）
- 多角色同時出戰
- Boss 以外的特殊敵人 AI
- 地圖節點的詳細圖示美術資源

---

## Further Notes

- **ADR-0002**：EventManager 從 Phase 2 起獨立存在，為 Phase 3 的 RandomEvent 腳本系統預留擴充點
- **ADR-0003**：RunState 不持有 HP / 牌組，直接從 LocalPlayer 讀取，避免雙重真相
- **地圖 UI 需要 Unity Editor 手動建立**：MapPanel 需要一個 ScrollRect 容器、動態生成的節點按鈕與連線，程式碼只提供資料與事件；具體 Prefab 由開發者在 Editor 中設計
- **CardData.UpgradedVersion** 為 null 時代表不可升級，Rest UI 應過濾不可升級的卡牌
