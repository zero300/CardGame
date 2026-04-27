using UnityEngine;

public interface ICardEffect
{
    public void ExecuteEffect(CardInstance card, CharacterInstance source, CharacterInstance target);
}
[System.Serializable]
public class DamageEffect : ICardEffect
{
    public void ExecuteEffect(CardInstance card, CharacterInstance source, CharacterInstance target)
    {
        // 這裡可以根據卡牌的屬性來計算傷害，例如基礎傷害、玩家狀態等
        int damage = card.currentCost; // 這裡假設傷害等於卡牌的費用，實際情況可以更複雜
        // TODO : 呼叫TARGET的受傷方法，並傳入傷害值
        Debug.Log($"{source}對{target}造成了{damage}點傷害！");
    }
}
[System.Serializable]
public class HealEffect : ICardEffect
{
    public void ExecuteEffect(CardInstance card, CharacterInstance source, CharacterInstance target)
    {
        // 這裡可以根據卡牌的屬性來計算治療量，例如基礎治療、玩家狀態等
        int healAmount = card.currentCost; // 這裡假設治療量等於卡牌的費用，實際情況可以更複雜
        // TODO : 呼叫TARGET的治療方法，並傳入治療量
        Debug.Log($"{source}對{target}恢復了{healAmount}點生命！");
    }
}
[System.Serializable]
public class ShieldEffect : ICardEffect
{
    public void ExecuteEffect(CardInstance card, CharacterInstance source, CharacterInstance target)
    {
        // 這裡可以根據卡牌的屬性來計算護甲值，例如基礎護甲、玩家狀態等
        int shieldAmount = card.currentCost; // 這裡假設護甲值等於卡牌的費用，實際情況可以更複雜
        // TODO : 呼叫TARGET的增加護甲方法，並傳入護甲值
        Debug.Log($"{source}對{target}增加了{shieldAmount}點護甲！");
    }
}