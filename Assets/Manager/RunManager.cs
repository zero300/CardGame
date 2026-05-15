public class RunManager
{
    public RunMode CurrentMode { get; private set; }
    public RunState RunState { get; private set; } = new();
    public CharacterInstance LocalPlayer { get; private set; }

    private MapManager _mapManager;
    public MapManager MapManager => _mapManager;
    private UIManager _uiManager;
    public UIManager UIManager => _uiManager;

    public void Initialize(IBattleManager battleManager, MapManager mapManager, CharacterInstance localPlayer, EventManager eventManager)
    {
        battleManager.OnBattleEnd += HandleBattleEnd;
        mapManager.OnNodeSelected += HandleNodeSelected;
        eventManager.OnEventCompleted += HandleEventCompleted;
        _mapManager = mapManager;
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
        LocalPlayer = localPlayer;
    }

    public void StartRun()
    {
        RunState = new RunState();
        _mapManager?.GenerateMap();
        SetMode(RunMode.MapView);
    }

    public void SetMode(RunMode mode)
    {
        CurrentMode = mode;
        _uiManager?.ShowMode(mode);
    }

    private void HandleBattleEnd(BattleResult result)
    {
        ServiceLocator.Instance.Get<CharacterManager>()?.CleanupEnemies();

        if (result == BattleResult.Victory)
        {
            _mapManager?.CompleteCurrentNode(RunState.CurrentNodeId);
            SetMode(RunMode.MapView);
        }
        else
        {
            SetMode(RunMode.Defeat);
        }
    }

    private void HandleEventCompleted()
    {
        _mapManager?.CompleteCurrentNode(RunState.CurrentNodeId);
        SetMode(RunMode.MapView);
    }

    private void HandleNodeSelected(string nodeId)
    {
        RunState.CurrentNodeId = nodeId;
        ServiceLocator.Instance.Get<EventManager>()?.HandleNodeEntered(nodeId);
    }
}
