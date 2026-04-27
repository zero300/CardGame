using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Games/CharacterData")]
public class CharacterData: ScriptableObject
{
    public string CharacterName;
    public int MaxHP;
    public int BaseEnergy; // 每回合基礎費用
    public Sprite CharacterSprite; // 尚且無需使用
}