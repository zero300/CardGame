using UnityEngine;

public interface ICardEffect
{
    void ExecuteEffect(CardInstance card, CharacterInstance source, CharacterInstance target);
}

[System.Serializable]
public class DamageEffect : ICardEffect
{
    public int damage;
    public int upgradedDamage;

    public void ExecuteEffect(CardInstance card, CharacterInstance source, CharacterInstance target)
    {
        int actual = card.IsUpgraded ? upgradedDamage : damage;
        target.TakeDamage(actual);
        Debug.Log($"{source.characterData.CharacterName} 對 {target.characterData.CharacterName} 造成了 {actual} 點傷害！");
    }
}

[System.Serializable]
public class HealEffect : ICardEffect
{
    public int heal;
    public int upgradedHeal;

    public void ExecuteEffect(CardInstance card, CharacterInstance source, CharacterInstance target)
    {
        int actual = card.IsUpgraded ? upgradedHeal : heal;
        source.Heal(actual);
        Debug.Log($"{source.characterData.CharacterName} 恢復了 {actual} 點生命！");
    }
}

[System.Serializable]
public class ShieldEffect : ICardEffect
{
    public int shield;
    public int upgradedShield;

    public void ExecuteEffect(CardInstance card, CharacterInstance source, CharacterInstance target)
    {
        int actual = card.IsUpgraded ? upgradedShield : shield;
        source.AddBlock(actual);
        Debug.Log($"{source.characterData.CharacterName} 增加了 {actual} 點護甲！");
    }
}
