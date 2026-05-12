# EventManager 從 Phase 2 起作為獨立 Manager 存在

## 狀態

已採用（Phase 2）

## 背景

玩家進入 Node 後，系統需要依 NodeType 決定要做什麼（啟動 Battle、顯示 Rest UI、進入 Shop 等）。最直接的做法是讓 `RunManager` 在 `HandleNodeEntered(NodeType)` 裡用 switch 直接處理，不需要額外的 Manager。

Phase 2 實作的節點類型只有三種（`Combat`、`Rest`、`Boss`），邏輯本身並不複雜。

## 決定

從 Phase 2 開始即建立獨立的 `EventManager`，`RunManager` 只負責 RunMode 的切換，具體的節點事件邏輯委派給 `EventManager` 處理。

## 理由

Phase 3 計畫引入 `RandomEvent`，其邏輯為腳本化的選項樹（ScriptableObject 驅動，每個事件有多個選項、條件判斷、分支結果）。若日後才從 `RunManager` 中抽出 `EventManager`，需要同時重構呼叫介面與事件腳本的掛載點。

提前建立獨立的 `EventManager` 能讓 `RandomEvent` 的擴充有明確的落點，避免 `RunManager` 在加入腳本事件後變成上帝類別。

## 取捨

- **代價**：Phase 2 多維護一個 Manager，且部分邏輯（如 Combat 節點直接呼叫 `BattleManager.StartBattle()`）若放在 `RunManager` 也夠簡單。
- **收益**：`RunManager` 保持薄（只管狀態轉換）；`EventManager` 成為未來所有節點事件腳本的統一擴充點。
