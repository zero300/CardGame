using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bootstrapper : MonoBehaviour
{
    private static Bootstrapper _instance;

    public GameObject baseCanvas;
    public Transform GamePanel;

    [Header("卡牌相關")]
    public GameObject cardPrefab;
    public float handCardSpacing = 70f;

    [Header("測試使用按鈕")]
    public Button StartRunButton;
    public Button StartButton;
    public Button TurnEndButton;
    public Button EnemyGenerateButton;

    [Header("角色UI相關")]
    public GameObject enemyCharacterUIPrefab;

    [Header("Panels")]
    public GameObject mapPanel;
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public GameObject restPanel;
    public GameObject stubPanel;

    [Header("地圖設定")]
    public MapLayoutData mapLayoutData;

    public CharacterInstance LocalPlayer { get; private set; }

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (GamePanel == null && baseCanvas != null)
        {
            GamePanel = baseCanvas.transform.Find("GamePanel");
            if (GamePanel == null)
                Debug.LogWarning("Bootstrapper: 無法在 baseCanvas 下找到 GamePanel");
        }

        InitialManager();

        StartRunButton?.onClick.AddListener(OnStartRunButtonClicked);
        StartButton?.onClick.AddListener(OnStartButtonClicked);
        TurnEndButton?.onClick.AddListener(OnTurnEndButtonClicked);
        EnemyGenerateButton?.onClick.AddListener(OnEnemyGenerateButtonClicked);
    }

    private void InitialManager()
    {
        // BattleManager
        GameObject bmGo = new GameObject("BattleManager");
        IBattleManager battleManager = bmGo.AddComponent<BattleManager>();
        DontDestroyOnLoad(bmGo);
        ServiceLocator.Instance.Register<IBattleManager>(battleManager);

        // UIManager — receives all panel refs from Inspector
        var uiManager = new UIManager(mapPanel, GamePanel?.gameObject, victoryPanel, defeatPanel, restPanel, stubPanel);
        ServiceLocator.Instance.Register(uiManager);

        // MapManager — must be created before RunManager so Initialize can subscribe
        var mapManager = new MapManager(mapLayoutData);
        ServiceLocator.Instance.Register(mapManager);

        // RunManager — subscribes to BattleManager.OnBattleEnd and MapManager.OnNodeSelected
        var runManager = new RunManager();
        runManager.Initialize(battleManager, mapManager);
        ServiceLocator.Instance.Register(runManager);

        // EventManager (Slice 2 stub; Slices 3-5 fill in real dispatch)
        var eventManager = new EventManager();
        ServiceLocator.Instance.Register(eventManager);

        // CardManager
        var cardManager = new CardManager(cardPrefab, handCardSpacing, baseCanvas);
        ServiceLocator.Instance.Register(cardManager);

        // CharacterManager
        var characterManager = new CharacterManager(enemyCharacterUIPrefab, GamePanel);
        ServiceLocator.Instance.Register(characterManager);

        LocalPlayer = characterManager.CreateCharacter("Player");
        characterManager.CreateCharacterUI(LocalPlayer, isEnemy: false);
    }

    void OnDestroy()
    {
        if (_instance != this) return;
        _instance = null;
        ServiceLocator.Instance.Clear();
    }

    private void OnStartRunButtonClicked()
    {
        ServiceLocator.Instance.Get<RunManager>()?.StartRun();
    }

    private void OnStartButtonClicked()
    {
        var cardManager = ServiceLocator.Instance.Get<CardManager>();
        var characterManager = ServiceLocator.Instance.Get<CharacterManager>();
        var battleManager = ServiceLocator.Instance.Get<IBattleManager>();
        var runManager = ServiceLocator.Instance.Get<RunManager>();

        runManager?.SetMode(RunMode.Battle);

        // TODO: 等到有選單系統後，應改成根據玩家選擇的角色來生成卡組和敵人
        var deck = new List<CardInstance>
        {
            cardManager.GenerateCardByString("BaseAttack"),
            cardManager.GenerateCardByString("BaseAttack"),
            cardManager.GenerateCardByString("BaseAttack"),
            cardManager.GenerateCardByString("BaseAttack"),
            cardManager.GenerateCardByString("BaseShield"),
            cardManager.GenerateCardByString("BaseShield"),
            cardManager.GenerateCardByString("BaseShield"),
            cardManager.GenerateCardByString("BaseShield"),
        };
        LocalPlayer.InitializeDeck(deck);
        LocalPlayer.BattleStart();

        CharacterInstance enemy = characterManager.CreateCharacter("Enemy1");
        if (enemy != null)
        {
            enemy.EnemyController = new SimpleEnemyController();
            characterManager.CreateCharacterUI(enemy, isEnemy: true);
        }

        battleManager.StartBattle(LocalPlayer, new List<CharacterInstance> { enemy });
    }

    private void OnTurnEndButtonClicked()
    {
        ServiceLocator.Instance.Get<IBattleManager>()?.EndPlayerTurn();
    }

    public void OnEnemyGenerateButtonClicked() { }
}
