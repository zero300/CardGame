using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager
{
    private const string ResourcePath = "CardDatas/";
    private GameObject _cardPrefab;

    private float cardSpacing = 70f;
    private readonly Dictionary<CardInstance, CardUI> _handUI = new();
    private DeckController _boundDeckController;
    private IBattleManager _battleManager;
    #region 方便卡片更新畫面儲存的GameObject
    public GameObject GamePanel;
    public Text DrawCount;
    public Text DiscardCount;
    public GameObject HandGameObject;
    #endregion
    public CardManager(GameObject cardPrefab, float handCardSpacing, GameObject baseCanvas)
    {
        _cardPrefab = cardPrefab;
        cardSpacing = handCardSpacing;

        GamePanel = baseCanvas.transform.Find("GamePanel").gameObject;
        DrawCount = GamePanel.transform.Find("Draw").Find("DrawCount").GetComponent<Text>();
        DiscardCount = GamePanel.transform.Find("Discard").Find("DiscardCount").GetComponent<Text>();
        HandGameObject = GamePanel.transform.Find("Hand").gameObject;
        // 初始化 BattleManager 引用
        _battleManager = ServiceLocator.Instance.Get<IBattleManager>();
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
        _handUI[card] = cardUI;
    }
    public void RemoveCardUI(CardInstance card)
    {
        if (_handUI.TryGetValue(card, out var cardUI))
        {
            _handUI.Remove(card);
            GameObject.Destroy(cardUI.gameObject);
            UpdateHandLayout();
        }
    }
    public void BindLocalPlayerHand(CharacterInstance localPlayer)
    {
        // 如先前已經進行綁定
        if (_boundDeckController != null)
        {
            _boundDeckController.OnCardDrawn -= SpawnCardUI;
            _boundDeckController.OnCardDiscard -= RemoveCardUI;
        }

        _boundDeckController = localPlayer.DeckController;
        _boundDeckController.OnCardDrawn += SpawnCardUI;
        _boundDeckController.OnCardDiscard += RemoveCardUI;

        SetDrawCount();
        SetDiscardCount();
    }
    public void BindDeckControllerOnDraw(DeckController deckController)
    {
        deckController.OnCardDrawn += SpawnCardUI;
    }
    public void UnbindDeckControllerOnDraw(DeckController deckController)
    {
        deckController.OnCardDrawn -= SpawnCardUI;
    }
    public void BindDeckControllerOnDiscard(DeckController deckController)
    {
        deckController.OnCardDiscard += RemoveCardUI;
    }
    public void UnbindDeckControllerOnDiscard(DeckController deckController)
    {
        deckController.OnCardDiscard -= RemoveCardUI;
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
        int cardCount = _handUI.Count;
        if (cardCount == 0) return;

        float startX = -(cardCount - (cardCount / 2)) * cardSpacing / 2f;

        int i = 0;
        foreach (var cardUI in _handUI.Values)
        {
            cardUI.UpdateCardPosition(new Vector3(startX + (i * cardSpacing), 0, 0));
            i++;
        }
    }
    // 玩家嘗試打出卡牌
    public bool TryPlayCard(CardInstance card, CharacterUI targetUI)
    {
        if (_battleManager != null && !_battleManager.IsPlayerTurn) return false;

        CharacterInstance cardOwner = card.Owner;
        CharacterInstance targetLogic = targetUI.GetCharacterInstance();

        if (cardOwner.CurrentEnergy < card.CurrentCost)
        {
            Debug.LogWarning("CardManager: 能量不足，無法打出卡牌");
            return false;
        }

        cardOwner.ConsumeEnergy(card.CurrentCost);

        foreach (var effect in card.baseCardData.Effects)
            effect.ExecuteEffect(card, cardOwner, targetLogic);

        cardOwner.MoveToPostUse(card);

        return true;
    }
    public void SetDrawCount()
    {
        if (_boundDeckController == null) return;
        DrawCount.text = _boundDeckController.DrawCards.Count.ToString();
    }
    public void SetDiscardCount()
    {
        if (_boundDeckController == null) return;
        DiscardCount.text = _boundDeckController.DiscardCards.Count.ToString();
    }
}
