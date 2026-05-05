using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager
{
    private const string ResourcePath = "CardDatas/";
    private GameManager _gameManager;
    private GameObject _baseCanvas; 
    private GameObject _cardPrefab;

    private float cardSpacing = 70f;
    private List<CardUI> _currentHandUI = new List<CardUI>(); // 用於追蹤手牌的UI物件，方便更新和管理
    private CharacterInstance _player => _gameManager.Player; // 方便訪問玩家角色
    #region 方便卡片更新畫面儲存的GameObject 
    public GameObject GamePanel ; 
    public Text DrawCount ; 
    public Text DiscardCount ;
    public GameObject HandGameObject;
    #endregion
    public CardManager(GameManager gameManager, GameObject cardPrefab)
    {
        _gameManager = gameManager;
        _baseCanvas = _gameManager.baseCanvas;
        _cardPrefab = cardPrefab;

        cardSpacing = _gameManager.handCardSpacing; // 從GameManager獲取手牌間距設定

        GamePanel = _baseCanvas.transform.Find("GamePanel").gameObject; // 假設GamePanel是baseCanvas的子物件
        DrawCount = GamePanel.transform.Find("Draw").Find("DrawCount").GetComponent<Text>(); // 假設DrawCount是Draw物件的子物件
        DiscardCount = GamePanel.transform.Find("Discard").Find("DiscardCount").GetComponent<Text>(); // 假設DiscardCount是Draw物件的子物件
        HandGameObject = GamePanel.transform.Find("Hand").gameObject; // 假設DiscardCount是Draw物件的子物件
        // 這裡可以初始化一些卡牌相關的資料或設定

    }
    public CardInstance GenerateCardByString(string cardName)
    {
        // TODO : 緩存卡牌資料以提高性能，避免每次生成卡牌都從資源中加載
        // 感覺可以改成根據ID判斷，不過目前先用名稱來測試
        CardData cardData = (CardData)Resources.Load($"{ResourcePath}{cardName}");
        return GenerateCard(cardData);
    }
    public CardInstance GenerateCard(CardData cardData)
    {
        // TODO : 緩存卡牌資料以提高性能，避免每次生成卡牌都從資源中加載
        CardInstance cardInstance = new CardInstance(cardData);
        return cardInstance;
    }
    public void GenerateCardUI(List<CardInstance> cards)
    {
        foreach (CardInstance card in cards)
        {
            GenerateCardUI(card);
        }
    }
    public void GenerateCardUI(CardInstance card)
    {
        GameObject cardObject = GameObject.Instantiate(_cardPrefab, HandGameObject.transform);
        CardUI cardUI = cardObject.GetComponent<CardUI>();
        cardUI.BindCardInstance(card);
        // 綁定 CardManager 以便 CardUI 能呼叫管理器的行為 (例如 TryPlayCard)
        cardUI.BindCardManager(this);
        _currentHandUI.Add(cardUI); // 將生成的CardUI加入追蹤列表
    }
    public void RemoveCardUI(CardUI cardUI)
    {
        if (_currentHandUI.Contains(cardUI))
        {
            _currentHandUI.Remove(cardUI);
            
            GameObject.Destroy(cardUI.gameObject); // 或者丟進物件池 (Object Pool)

            // 卡牌變少了，重新排版剩下的卡牌
            UpdateHandLayout();
        }
    }
    public void BindDeckControllerOnDraw(DeckController deckController)
    {
        deckController.OnCardDrawn += SpawnCardUI;
    }
    public void UnbindDeckControllerOnDraw(DeckController deckController)
    {
        deckController.OnCardDrawn -= SpawnCardUI;
    }
    private void SpawnCardUI(CardInstance cardInstance)
    {
        GenerateCardUI(cardInstance);
        UpdateHandLayout();
    }
    /// <summary>
    /// 重新排版手牌UI，確保在手牌數量變化時能夠適當地調整卡牌的位置和間距，使其保持整齊和美觀
    /// </summary>
    public void UpdateHandLayout()
    {
        int cardCount = _currentHandUI.Count;
        if (cardCount == 0) return;

        // 計算最左邊第一張卡的起始 X 座標，確保整體置中
        float startX = -(cardCount - (cardCount / 2) ) * cardSpacing / 2f ;

        // 這裡可以根據需要更新手牌的佈局，例如重新排列卡牌位置等
        for (int i = 0; i < _currentHandUI.Count; i++)
        {
            // 根據索引計算每張卡牌的位置，這裡只是示例，你可以根據實際需求調整位置計算方式
            Vector3 position = new Vector3(startX + (i * cardSpacing), 0, 0);
            _currentHandUI[i].UpdateCardPosition(position);
            // TODO : 動畫
        }
    }
    // 玩家嘗試打出卡牌
    public bool TryPlayCard(CardUI cardUI, CharacterUI targetUI)
    {
        CardInstance cardLogic = cardUI.GetCardInstance();
        CharacterInstance cardOwner = cardUI.GetCardOwner();
        CharacterInstance targetLogic = targetUI.GetCharacterInstance();

        // 1. 條件檢查：判斷費用夠不夠？(這裡假設你有個玩家的 PlayerInstance 記錄費用)
        if (cardOwner.CurrentEnergy < cardLogic.CurrentCost) return false;

        // 2. 扣除費用
        cardOwner.ConsumeEnergy(cardLogic.CurrentCost);

        // 3. 執行卡牌效果！ (⭐ 這裡就是觸發攻擊的地方)
        foreach (var effect in cardLogic.baseCardData.Effects)
        {
            // 裡面會觸發卡牌邏輯
            effect.ExecuteEffect(cardLogic, cardOwner, targetLogic);
        }

        // 4. 處理卡牌去向 (從手牌移除，放入棄牌堆或消耗區)
        // 由 DeckController 根據 CardData.PostUse 來決定並執行移動
        cardOwner.MoveToPostUse(cardLogic);

        // 5. 銷毀 UI
        RemoveCardUI(cardUI);

        return true; // 打出成功
    }
    public void SetDrawCount()
    {
        DrawCount.text = _player.DeckController.DrawCards.Count.ToString();
    }
    public void SetDiscardCount()
    {
        DiscardCount.text = _player.DeckController.DiscardCards.Count.ToString();
    }
}
