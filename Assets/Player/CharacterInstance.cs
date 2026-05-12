
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInstance
{
    public CharacterData characterData { get; private set; }
    public int CurrentHP;
    public int CurrentBlock;
    public int MaxEnergy;
    public int CurrentEnergy;
    public int drawCount = 5;
    public int MaxHands = 10;

    public IEnemyController EnemyController { get; set; }
    private DeckController _deckController = new DeckController();
    public DeckController DeckController { get { return _deckController; } }

    // 宣告事件，讓 UI 監聽
    public event Action<int, int> OnHPChanged;    // 傳遞: 當前HP, 最大HP
    public event Action<int> OnBlockChanged;      // 傳遞: 當前護甲
    public event Action<int> OnDamageTaken;       // 傳遞: 實際造成的HP傷害 (用來跳傷害數字)
    public event Action<int> OnEnergyChanged;     // 傳遞: 當前
    public event Action OnDeath;
    public CharacterInstance(CharacterData data)
    {
        if (data == null)
        {
            Debug.LogWarning("CharacterInstance: 傳入的 CharacterData 為 null");
            return;
        }

        characterData = data;
        CurrentHP = data.MaxHP;
        CurrentBlock = 0;
        MaxEnergy = data.BaseEnergy;
        CurrentEnergy = data.BaseEnergy;
    }
    /// <summary>
    /// 初始化卡牌
    /// </summary>
    /// <param name="initialDeck"></param>
    public void InitializeDeck(List<CardInstance> initialDeck)
    {
        DeckController.InitializeDeck(initialDeck);
        // 將這個角色綁定為每張卡的擁有者，方便之後在使用卡牌時能找到來源的DeckController等資訊
        if (initialDeck != null)
        {
            foreach (var card in initialDeck)
            {
                card.BindCharacterInstance(this);
            }
        }
    }
    public void BattleStart()
    {
        DeckController.BattleStart();
    }
    public void TurnStart()
    {
        ClearBlock();
        ResetEnergy();
        DeckController.DrawCard(drawCount);
    }
    public void ResetEnergy()
    {
        CurrentEnergy = MaxEnergy;
        OnEnergyChanged?.Invoke(CurrentEnergy);
    }
    public void TurnEnd()
    {
        DeckController.TurnEnd();
    }
    public void MoveToPostUse(CardInstance card)
    {
        _deckController.MoveToPostUse(card);
    }
    public void ConsumeEnergy(int _consumnEnergy)
    {
        CurrentEnergy -= _consumnEnergy;
        if (CurrentEnergy < 0) CurrentEnergy = 0;
        // TODO : 可以在這裡觸發能量變化的事件，讓UI更新能量顯示等
        OnEnergyChanged?.Invoke(CurrentEnergy);
    }
    /// <summary>
    /// 受傷邏輯
    /// </summary>
    /// <param name="damageAmount"></param>
    public void TakeDamage(int damageAmount)
    {
        Debug.Log($"CharacterInstance: {characterData.CharacterName} 受到傷害 => {damageAmount}");
        if (damageAmount <= 0) return;

        int actualHPDamage = 0;

        // 1. 先扣除護甲 (Block)
        if (CurrentBlock > 0)
        {
            if (CurrentBlock >= damageAmount)
            {
                // 護甲足夠抵擋所有傷害
                CurrentBlock -= damageAmount;
                OnBlockChanged?.Invoke(CurrentBlock);
                return; // 沒有扣到血，直接結束
            }
            else
            {
                // 護甲破裂，計算剩餘傷害
                damageAmount -= CurrentBlock;
                CurrentBlock = 0;
                OnBlockChanged?.Invoke(CurrentBlock);
            }
        }

        // 2. 扣除真實血量 (HP)
        actualHPDamage = damageAmount;
        CurrentHP -= actualHPDamage;

        // 確保血量不低於 0
        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            OnHPChanged?.Invoke(CurrentHP, characterData.MaxHP);
            OnDeath?.Invoke();
        }
        else
        {
            OnHPChanged?.Invoke(CurrentHP, characterData.MaxHP);
            OnDamageTaken?.Invoke(actualHPDamage); // 觸發受傷事件 (可呼叫 UI 播放動畫)
        }
    }
    /// <summary>
    /// 獲得護甲邏輯 (打出防禦卡時呼叫)
    /// </summary>
    /// <param name="amount">獲得護甲數量</param>
    public void AddBlock(int amount)
    {
        CurrentBlock += amount;
        OnBlockChanged?.Invoke(CurrentBlock);
    }
    /// <summary>
    /// 回合開始時通常會清空護甲
    /// </summary>
    public void ClearBlock()
    {
        CurrentBlock = 0;
        OnBlockChanged?.Invoke(CurrentBlock);
    }
}
