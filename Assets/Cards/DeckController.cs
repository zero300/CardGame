using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckController
{
    /// <summary>
    /// 總牌組，包含所有的牌，這裡可以用來初始化 DrawCards、DiscardCards等
    /// </summary>
    public List<CardInstance> Deck = new List<CardInstance>();
    public List<CardInstance> DrawCards = new List<CardInstance>();
    public List<CardInstance> DiscardCards = new List<CardInstance>();
    public List<CardInstance> HandCards = new List<CardInstance>();
    public List<CardInstance> DeleteCards = new List<CardInstance>();

    // 宣告一個事件：當卡牌被抽到時觸發，並傳遞那張卡的實體
    public event Action<CardInstance> OnCardDrawn;

    public void InitializeDeck(List<CardInstance> initialDeck)
    {
        Deck.AddRange(initialDeck);
    }
    public void GameStart(int initialCount)
    {
        DrawCards.AddRange(Deck);
        DrawCard(initialCount); // 假設遊戲開始時抽取初始張數
        // TODO : 可以在這裡添加一些遊戲開始時的邏輯，例如抽取初始手牌、觸發遊戲開始事件等
    }
    public void DeckAddCard(CardInstance card)
    {
        Deck.Add(card);
    }
    public void DeckRemoveCard(CardInstance card)
    {
        Deck.Remove(card);
    }  
    /// <summary>
    /// 抽卡
    /// </summary>
    /// <param name="drawCount">要抽取的牌數量</param> 
    public void DrawCard(int drawCount)
    {
        for (int i = 0; i < drawCount; i++)
        {
            if (DrawCards.Count <= 0)
            {
                // TODO : 將DiscardCards 洗牌後放回DrawCards
                ShuffleDiscardIntoDraw();
            }
            // 洗牌後，如果抽牌堆沒有牌了，則表示整個牌組都用完了，無牌可抽
            if (DrawCards.Count <= 0)
            {
                Debug.Log("抽牌堆已空，無牌可抽。");
                return;
            }
            CardInstance drawCard = DrawCards[0];
            DrawCards.RemoveAt(0);
            if (HandCards.Count >= 10)
            {
                // 超過手牌上限，將牌丟棄
                DiscardCards.Add(drawCard);
                continue;
            }
            drawCard.OnDrawing();
            HandCards.Add(drawCard);
            OnCardDrawn?.Invoke(drawCard);
            drawCard.OnDrawed();
        }
    }
    public void GameEnd()
    {
        // TODO : 執行遊戲結束的相關邏輯，例如顯示遊戲結束畫面、重置遊戲狀態等
        HandCards.Clear();
        DrawCards.Clear();
        DiscardCards.Clear();
        DeleteCards.Clear();
    }
    /// <summary> 
    /// 回合結束
    /// </summary>
    public void TurnEnd()
    {
        // TODO : 執行回合結束的相關邏輯，例如重置玩家狀態、觸發回合結束事件等
        for (int i = HandCards.Count - 1; i >= 0; i--)
        {
            CardInstance discard = HandCards[i];
            // TODO : 虛無效果，當回合結束時，手牌中的牌會被丟棄

            // TODO : 保留效果，當回合結束時，手牌中的牌會保留到下一回合

            // 正常效果，當回合結束時，手牌中的牌會被丟棄
            discard.OnDiscarding();
            HandCards.RemoveAt(i);
            DiscardCards.Add(discard);
            discard.OnDiscarded();
        }
    }

    /// <summary>
    /// 洗牌，將棄牌堆洗回抽牌堆
    /// </summary>
    private void ShuffleDiscardIntoDraw()
    {
        // 若棄牌堆為空直接返回
        if (DiscardCards == null || DiscardCards.Count == 0)
        {
            Debug.Log("棄牌堆為空，無牌可回填到抽牌堆。");
            return;
        }

        // Fisher-Yates 隨機洗牌（使用 UnityEngine.Random）
        for (int i = DiscardCards.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1); // 0..i (包含 i)
            var tmp = DiscardCards[i];
            DiscardCards[i] = DiscardCards[j];
            DiscardCards[j] = tmp;
        }

        // 將洗好的棄牌堆全部放回抽牌堆（追加），然後清空棄牌堆
        DrawCards.AddRange(DiscardCards);
        int movedCount = DiscardCards.Count;
        DiscardCards.Clear();

        Debug.Log($"已將棄牌堆洗回抽牌堆，共回填 {movedCount} 張牌。");
    }

    #region Debug 使用相關Methods 
    public void DebugDeck()
    {
        Debug.Log("Deck:");
        foreach (var card in Deck)
        {
            Debug.Log($"- {card.baseCardData.Name} (Type: {card.baseCardData.Type}, Rarity: {card.baseCardData.Rarity})");
        }
    }
    public void DebugDrawCards()
    {
        Debug.Log("Draw Cards:");
        foreach (var card in DrawCards)
        {
            Debug.Log($"- {card.baseCardData.Name} (Type: {card.baseCardData.Type}, Rarity: {card.baseCardData.Rarity})");
        }
    }
    public void DebugHandCards()
    {
        Debug.Log("Hand Cards:");
        foreach (var card in HandCards)
        {
            Debug.Log($"- {card.baseCardData.Name} (Type: {card.baseCardData.Type}, Rarity: {card.baseCardData.Rarity})");
        }
    }
    #endregion
}
