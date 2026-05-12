public class RunManager
{
    public RunMode CurrentMode { get; private set; }
    public RunState RunState { get; private set; } = new();

    private MapManager _mapManager;
    public MapManager MapManager { get { return _mapManager; } }
    private UIManager _uiManager;
    public UIManager UIManager { get { return _uiManager; } }

    public void Initialize(IBattleManager battleManager, MapManager mapManager)
    {
        battleManager.OnBattleEnd += HandleBattleEnd;
        mapManager.OnNodeSelected += HandleNodeSelected;
        _mapManager = mapManager;
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
    }

    public void StartRun()
    {
        RunState ??= new RunState();
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

    private void HandleNodeSelected(string nodeId)
    {
        RunState.CurrentNodeId = nodeId;
        ServiceLocator.Instance.Get<EventManager>()?.HandleNodeEntered(nodeId);
    }
}
