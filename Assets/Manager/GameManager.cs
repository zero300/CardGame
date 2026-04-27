using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // 因為只是一個簡單的遊戲管理器，所以暫時不需要任何變數或方法
    private static GameManager _instance;
    public static GameManager Instance
    {
        get 
        {
            if(_instance == null)
            {
                CreateInstanceObject();
            }   
            return _instance; 
        }
    }

    public GameObject baseCanvas;
    /// <summary>
    /// 卡牌Prefab
    /// </summary> 
    public  GameObject cardPrefab; // 這裡假設你有一個卡牌的預製體

    private CardManager cardManager;
    /// <summary>
    /// 手牌之間的間距，單位為像素，可以根據需要調整
    /// </summary>
    public float handCardSpacing = 70f;
    public Button StartButton; // 這裡假設你有一個開始遊戲的按鈕
    public Button DrawButton; 
    public Button TurnEndButton;

    public CharacterInstance Player ;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // 重複判斷，防止生成多個實例
        if (_instance != null)
        {
            Debug.LogWarning("GameManager instance already exists. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        _instance = this ;
        DontDestroyOnLoad(gameObject);

        // 初始化Manager 
        cardManager = new CardManager(Instance, cardPrefab);

        // TEST:簡單賦予點擊事件
        StartButton?.onClick.AddListener(StartButtonOnClick) ; // 為按鈕添加點擊事件
        DrawButton?.onClick.AddListener(DrawButtonOnClick); // 為按鈕添加點擊事件
        TurnEndButton?.onClick.AddListener(TurnEndButtonOnClick); // 為按鈕添加點擊事件

        Player = new CharacterInstance(); // 假設你有一個CharacterInstance類別，這裡創建一個玩家角色
        cardManager.BindDeckControllerOnDraw(Player.DeckController); // 綁定抽牌事件
    }
    void OnDestroy()
    {
        cardManager.UnbindDeckControllerOnDraw(Player.DeckController); // 綁定抽牌事件
        // 當物件被銷毀時，清除實例引用
        if (_instance == this)
        {
            _instance = null;
        }
    }
    private static void CreateInstanceObject()
    {
        GameObject instanceObject = new GameObject("GameManager");
        _instance = instanceObject.AddComponent<GameManager>();
        DontDestroyOnLoad(instanceObject);
    }
    private void StartButtonOnClick()
    {
        Debug.Log("生成卡牌");

        List<CardInstance> list = new List<CardInstance>
        {
            cardManager.GenerateCardByString("BaseAttack"),
            cardManager.GenerateCardByString("BaseAttack"),
            cardManager.GenerateCardByString("BaseShield"),
            cardManager.GenerateCardByString("BaseShield")
        };
        Player.InitializeDeck(list);
        Player.GameStart();
    }
    private void DrawButtonOnClick()
    {
        Debug.Log("抽牌");
    }
    private void TurnEndButtonOnClick()
    {
        Debug.Log("結束回合");
    }
}
