using UnityEngine;

public interface ICardEffect
{
    public void ExecuteEffect(CardInstance card, CharacterInstance source, CharacterInstance target);
}
[System.Serializable]
public class DamageEffect : ICardEffect
{
    // 傷害數值 
    public int damage;
    public void ExecuteEffect(CardInstance card, CharacterInstance source, CharacterInstance target)
    {
        // 這裡可以根據卡牌的屬性來計算傷害，例如基礎傷害、玩家狀態等
        target.TakeDamage(damage);
        // TODO : 呼叫TARGET的受傷方法，並傳入傷害值
        Debug.Log($"{source.characterData.CharacterName} 對 {target.characterData.CharacterName} 造成了 {damage} 點傷害！");
    }
}
[System.Serializable]
public class HealEffect : ICardEffect
{
    public int heal;
    public void ExecuteEffect(CardInstance card, CharacterInstance source, CharacterInstance target)
    {
        // 這裡可以根據卡牌的屬性來計算治療量，例如基礎治療、玩家狀態等
        // TODO : 呼叫TARGET的治療方法，並傳入治療量
        Debug.Log($"{source.characterData.CharacterName}對{target}恢復了{heal}點生命！");
    }
}
[System.Serializable]
public class ShieldEffect : ICardEffect
{
    public int shield;
    public void ExecuteEffect(CardInstance card, CharacterInstance source, CharacterInstance target)
    {
        // 這裡可以根據卡牌的屬性來計算護甲值，例如基礎護甲、玩家狀態等
        source.AddBlock(shield);
        // TODO : 呼叫TARGET的增加護甲方法，並傳入護甲值
        Debug.Log($"{source.characterData.CharacterName}增加了{shield}點護甲！");
    }
}