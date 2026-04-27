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
    
    // 造成的基礎數值
    public int BaseDamage; // 基礎傷害
    public int BaseHealth; // 基礎回復
    public int BaseShield; // 基礎護甲值 

    // 存放效果列表
    [SerializeReference]
    public List<ICardEffect> Effects = new List<ICardEffect>();
}
