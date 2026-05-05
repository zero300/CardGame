using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 定義卡牌的稀有度
/// </summary>
public enum CardRarity
{
    Common,     // 普通
    Uncommon,   // 罕見
    Rare,       // 稀有
    Epic,       // 史詩
    Legendary   // 傳說
}
/// <summary>
/// 定義卡牌的類型
/// </summary>
public enum CardType
{
    Minion,     // 生物/手下
    Spell,      // 法術
    Equipment,  // 裝備/武器
    Field,      // 場地
    Curse       // 詛咒
}
/// <summary>
/// 打出後卡片去向（用於 Data 驅動）
/// </summary>
public enum PostUseAction
{
    Discard,        // 放入棄牌堆（預設）
    Exhaust,        // 消耗/移出遊戲（放入 Delete/Exhaust 區）
    RemoveFromGame  // 直接移除（行為上與 Exhaust 類似，但語意不同）
}

[CreateAssetMenu(fileName = "CardData", menuName = "Games/CardData")]
public class CardData : ScriptableObject
{
    public string ID; // 卡牌的唯一識別碼，可以用來區分不同的卡牌
    public string Name; // 卡牌名稱
    [TextArea] 
    public string Description; // 卡牌描述
    public int Cost; // 卡牌消耗
    public CardType Type; // 卡牌類型
    public CardRarity Rarity; // 卡牌稀有度
    public PostUseAction PostUseAction; // 打出後卡片去向
    // 存放效果列表
    [SerializeReference]
    public List<ICardEffect> Effects = new List<ICardEffect>();
}
