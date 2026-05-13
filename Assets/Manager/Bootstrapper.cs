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
        // 1. BattleManager
        GameObject bmGo = new GameObject("BattleManager");
        IBattleManager battleManager = bmGo.AddComponent<BattleManager>();
        DontDestroyOnLoad(bmGo);
        ServiceLocator.Instance.Register<IBattleManager>(battleManager);

        // 2. UIManager
        var uiManager = new UIManager(mapPanel, GamePanel?.gameObject, victoryPanel, defeatPanel, restPanel, stubPanel);
        ServiceLocator.Instance.Register(uiManager);

        // 3. MapManager
        var mapManager = new MapManager(mapLayoutData);
        ServiceLocator.Instance.Register(mapManager);

        // 4. CardManager
        var cardManager = new CardManager(cardPrefab, handCardSpacing, baseCanvas);
        ServiceLocator.Instance.Register(cardManager);

        // 5. CharacterManager + LocalPlayer (must be before RunManager.Initialize)
        var characterManager = new CharacterManager(enemyCharacterUIPrefab, GamePanel);
        ServiceLocator.Instance.Register(characterManager);
        LocalPlayer = characterManager.CreateCharacter("Player");
        characterManager.CreateCharacterUI(LocalPlayer, isEnemy: false);

        // 6. RunManager — subscribes to battle/map events, holds LocalPlayer reference
        var runManager = new RunManager();
        runManager.Initialize(battleManager, mapManager, LocalPlayer);
        ServiceLocator.Instance.Register(runManager);

        // 7. EventManager
        var eventManager = new EventManager();
        ServiceLocator.Instance.Register(eventManager);
    }

    void OnDestroy()
    {
        if (_instance != this) return;
        _instance = null;
        ServiceLocator.Instance.Clear();
    }

    private void OnStartRunButtonClicked()
    {
        var cardManager = ServiceLocator.Instance.Get<CardManager>();
        var runManager = ServiceLocator.Instance.Get<RunManager>();

        // Build the starting deck for this Run
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
        LocalPlayer.ResetHP();

        runManager?.StartRun();
    }

    // Kept for direct battle testing without the map flow
    private void OnStartButtonClicked()
    {
        var cardManager = ServiceLocator.Instance.Get<CardManager>();
        var characterManager = ServiceLocator.Instance.Get<CharacterManager>();
        var battleManager = ServiceLocator.Instance.Get<IBattleManager>();
        var runManager = ServiceLocator.Instance.Get<RunManager>();

        runManager?.SetMode(RunMode.Battle);

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
