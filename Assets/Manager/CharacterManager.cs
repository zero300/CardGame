using System.Collections.Generic;
using UnityEngine;

public class CharacterManager
{
    private const string ResourcePath = "CharacterDatas/";

    private readonly GameObject _characterUIPrefab;
    private readonly Transform _playerParent;
    private readonly Transform _enemyParent;

    private readonly List<CharacterInstance> _characters = new List<CharacterInstance>();

    public CharacterManager(GameObject characterUIPrefab, Transform gamePanel)
    {
        _characterUIPrefab = characterUIPrefab;
        _playerParent = gamePanel?.Find("PlayerSpace");
        _enemyParent = gamePanel?.Find("EnemySpace");

        // 警告用
        if (_playerParent == null)
        {
            Debug.LogWarning("CharacterManager: 無法在 GamePanel 下找到 PlayerSpace，請手動指定 PlayerSpace");
        }
        if (_enemyParent == null)
        {
            Debug.LogWarning("CharacterManager: 無法在 GamePanel 下找到 EnemySpace，請手動指定 EnemySpace");
        }
    }
    public CharacterInstance CreateCharacter(string characterName)
    {
        CharacterData characterData = Resources.Load<CharacterData>($"{ResourcePath}{characterName}");
        if (characterData == null)
        {
            Debug.LogWarning($"CharacterManager: 無法找到 CharacterData => {ResourcePath}{characterName}");
            return null;
        }

        return CreateCharacter(characterData);
    }
    public CharacterInstance CreateCharacter(CharacterData characterData)
    {
        if (characterData == null)
        {
            Debug.LogWarning("CharacterManager: 傳入的 CharacterData 為 null");
            return null;
        }

        CharacterInstance characterInstance = new CharacterInstance(characterData);
        _characters.Add(characterInstance);
        return characterInstance;
    }
    public void CreateCharacterUI(CharacterInstance characterInstance, bool isEnemy = true)
    {
        if (characterInstance == null)
        {
            Debug.LogWarning("CharacterManager: 傳入的 CharacterInstance 為 null，無法建立 UI");
            return;
        }

        if (_characterUIPrefab == null)
        {
            Debug.LogWarning("CharacterManager: 尚未指定 CharacterUI Prefab");
            return;
        }

        Transform targetParent = isEnemy ? _enemyParent : _playerParent;
        if (targetParent == null)
        {
            Debug.LogWarning($"CharacterManager: 尚未指定 {(isEnemy ? "敵人" : "玩家")} UI 父物件，無法建立 UI");
            return;
        }

        GameObject uiObject = GameObject.Instantiate(_characterUIPrefab, targetParent);
        CharacterUI characterUI = uiObject.GetComponent<CharacterUI>();
        if (characterUI == null)
        {
            Debug.LogWarning("CharacterManager: CharacterUI Prefab 上找不到 CharacterUI 組件");
            return;
        }

        characterUI.Setup(characterInstance);
    }
    public void RemoveCharacter(CharacterInstance character)
    {
        _characters.Remove(character);
    }

    public void CleanupEnemies()
    {
        _characters.RemoveAll(c => c.EnemyController != null);

        if (_enemyParent != null)
        {
            foreach (Transform child in _enemyParent)
                GameObject.Destroy(child.gameObject);
        }
    }
    public List<CharacterInstance> GetAliveEnemies()
    {
        return _characters.FindAll(c => c.EnemyController != null && c.CurrentHP > 0);
    }
}
