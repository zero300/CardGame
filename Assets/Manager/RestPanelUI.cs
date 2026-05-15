using UnityEngine;
using UnityEngine.UI;

public class RestPanelUI : MonoBehaviour
{
    [SerializeField] private Button _healButton;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private CardListPanelUI _cardListPanelUI;

    private EventManager _eventManager;
    private RunManager _runManager;

    private void OnEnable()
    {
        _eventManager ??= ServiceLocator.Instance.Get<EventManager>();
        _runManager ??= ServiceLocator.Instance.Get<RunManager>();

        _healButton?.onClick.AddListener(OnHealClicked);
        _upgradeButton?.onClick.AddListener(OnUpgradeClicked);
    }

    public void OnHealClicked()
    {
        var player = _runManager.LocalPlayer;
        if (player != null)
        {
            int healAmount = Mathf.FloorToInt(player.characterData.MaxHP * 0.3f);
            player.Heal(healAmount);
        }
        _eventManager?.CompleteEvent();
    }

    public void OnUpgradeClicked()
    {
        var player = _runManager.LocalPlayer;
        if (player == null) { _eventManager?.CompleteEvent(); return; }

        var upgradeable = player.DeckController.Deck.FindAll(c => c.baseCardData.CanUpgrade && !c.IsUpgraded);
        if (upgradeable.Count == 0) { _eventManager?.CompleteEvent(); return; }

        _cardListPanelUI?.Open(CardListMode.UpgradeCard, upgradeable, OnCardUpgraded);
    }

    private void OnCardUpgraded(CardInstance card)
    {
        Debug.Log($"Upgrading card: {card.ToString()}");
        card.Upgrade();
        _eventManager?.CompleteEvent();
    }
}
