using System.Collections.Generic;
using UnityEngine;

public class CharacterManager
{
    private const string ResourcePath = "CharacterDatas/";

    private readonly GameManager _gameManager;
    private readonly GameObject _characterUIPrefab;
    private readonly Transform _playerParent;
    private readonly Transform _enemyParent;

    private readonly List<CharacterInstance> _characters = new List<CharacterInstance>();

    public CharacterManager(GameManager gameManager, GameObject characterUIPrefab, Transform GamePanel)
    {
        _gameManager = gameManager;
        _characterUIPrefab = characterUIPrefab;
        _playerParent = GamePanel.Find("PlayerSpace");
        _enemyParent = GamePanel.Find("EnemySpace");

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
    // TODO : 可以在這裡添加一些管理角色的其他方法，例如移除角色、獲取所有角色列表等
    // TODO : 可以考慮添加一些事件，例如角色死亡事件，讓其他系統能夠訂閱並做出反應
    // TODO : 處理生成角色UI時的排序問題，例如玩家角色固定在左邊，敵人角色依次排列在右邊等
}
