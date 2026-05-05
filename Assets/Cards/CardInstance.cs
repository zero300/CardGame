using System;
using UnityEngine;




public class CardInstance
{
    // 卡片基礎屬性
    public CardData baseCardData ;
    /// <summary>
    /// 當前所需費用，因可能因為其他效果改變
    /// </summary>
    public int CurrentCost;
    public CharacterInstance Owner;
    public GameObject cardObject;
    public CardInstance(CardData cardData) 
    {
        baseCardData = cardData;
    }
    public void BindCardObject(GameObject cardObject)
    {
        this.cardObject = cardObject;
    }
    public void BindCharacterInstance(CharacterInstance Character)
    {
        this.Owner = Character;
    }
    /// <summary>
    /// 判斷是否可以將此卡打出
    /// </summary>
    /// <returns></returns>
    public virtual bool CanUse()
    {
        // todo : 可以傳入玩家狀態、場上狀態等參數來判斷是否可以打出這張卡
        // 例子：檢查玩家是否有足夠的資源來打出這張卡
        return true;
    }
    /// <summary>
    /// 當此卡被從卡堆中抽出時，執行的函式
    /// </summary>
    public virtual void OnDrawing()
    {
        Debug.Log($"抽到卡牌: {baseCardData.Name}");
    }
    /// <summary>
    /// 當此卡被從卡堆中抽出後，所執行的函式
    /// </summary>
    public virtual void OnDrawed() 
    {
        Debug.Log($"已經抽到卡牌: {baseCardData.Name}");
    }
    /// <summary>
    /// 當此卡牌從手牌中被打出時，所執行的函式
    /// </summary>
    public virtual void OnUsing()
    {
        Debug.Log($"打出卡牌: {baseCardData.Name}");
    }
    /// <summary>
    /// 當此卡牌打出後，所執行的函式
    /// </summary>
    public virtual void OnUsed()
    {
        Debug.Log($"打出卡牌後: {baseCardData.Name}");
    }
    /// <summary>
    /// 當捨棄卡牌時，所執行的函式
    /// </summary>
    public virtual void OnDiscarding()
    {
        Debug.Log($"捨棄卡牌: {baseCardData.Name}");
    }
    /// <summary>
    /// 當捨棄卡牌後，所執行的函式
    /// </summary>
    public virtual void OnDiscarded()
    {
        Debug.Log($"捨棄卡牌後: {baseCardData.Name}");
    }
    
    #region Override Function 
    public override string ToString()
    {
        return $"卡牌名稱: {baseCardData.Name}, 卡牌描述: {baseCardData.Description}, 卡牌費用: {baseCardData.Cost}, 卡牌類型: {baseCardData.Type}, 卡牌稀有度: {baseCardData.Rarity}";
    }
    #endregion

}
