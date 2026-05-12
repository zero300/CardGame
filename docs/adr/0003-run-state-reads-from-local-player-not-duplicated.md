# RunState 不持有 HP 與牌組，改由 LocalPlayer 直接持有

## 狀態

已採用（Phase 2）

## 背景

設計 `RunState` 時，有兩種方案：

**方案 A（複製）**：RunState 獨立儲存 `currentHP`、`maxHP`、`deck`，每場 Battle 結束後由 BattleManager / RunManager 將 LocalPlayer 的狀態回寫進 RunState；下一場 Battle 開始時再從 RunState 讀入並重建 LocalPlayer 狀態。

**方案 B（不複製）**：LocalPlayer 的 `CharacterInstance` 整場 Run 是同一個物件（`DontDestroyOnLoad`），HP 和 `DeckController.Deck` 天然持久。RunState 只存導航資料（`currentNodeId`）；其他狀態直接從 `LocalPlayer` 讀取。

## 決定

採用方案 B。Phase 2 的 `RunState` 只含 `currentNodeId`，HP 與牌組的真相來源是 `LocalPlayer.CurrentHP` 與 `LocalPlayer.DeckController.Deck`。

## 理由

`CharacterInstance` 已是正確的持久物件——它在 `Bootstrapper.Awake()` 建立並隨 `DontDestroyOnLoad` 的 GameObject 存活整場 Run。在 RunState 再存一份 HP 和 Deck 會造成：

1. **雙重真相**：兩處狀態需同步，任何一處更新遺漏都是 Bug。
2. **不必要的複雜度**：Round-trip 寫回 / 讀出的邏輯換來的是已經可以直接讀取的資料。

## 取捨

- **代價**：若未來需要存檔（Save / Load），RunState 必須補充 HP 和 Deck 的序列化欄位，屆時需要重構讀取方式。
- **收益**：Phase 2 無重複狀態、無同步 Bug 風險、程式碼更簡單。
- **升級路徑**：加存檔時，只需在 RunState 新增欄位，並在 `RunManager.SaveRun()` 讀取 `LocalPlayer` 的當前值寫入；不影響 Battle 期間的邏輯。
