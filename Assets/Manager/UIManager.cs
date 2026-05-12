using UnityEngine;

public class UIManager
{
    /// <summary>
    /// 地圖
    /// </summary>
    private readonly GameObject _mapPanel;
    /// <summary>
    /// 戰鬥
    /// </summary>
    private readonly GameObject _gamePanel;
    /// <summary>
    /// 勝利畫面
    /// </summary>
    private readonly GameObject _victoryPanel;
    /// <summary>
    /// 失敗畫面
    /// </summary>
    private readonly GameObject _defeatPanel;
    /// <summary>
    /// 休息事件
    /// </summary>
    private readonly GameObject _restPanel;
    /// <summary>
    /// 模組事件
    /// </summary>
    private readonly GameObject _stubPanel;

    public UIManager(GameObject mapPanel, GameObject gamePanel, GameObject victoryPanel,
                     GameObject defeatPanel, GameObject restPanel, GameObject stubPanel)
    {
        _mapPanel = mapPanel;
        _gamePanel = gamePanel;
        _victoryPanel = victoryPanel;
        _defeatPanel = defeatPanel;
        _restPanel = restPanel;
        _stubPanel = stubPanel;
    }

    public void ShowMode(RunMode mode)
    {
        _mapPanel?.SetActive(mode == RunMode.MapView);
        _gamePanel?.SetActive(mode == RunMode.Battle);
        _victoryPanel?.SetActive(mode == RunMode.Victory);
        _defeatPanel?.SetActive(mode == RunMode.Defeat);
        _restPanel?.SetActive(mode == RunMode.RestEvent);
        _stubPanel?.SetActive(mode == RunMode.StubEvent);
    }
}
