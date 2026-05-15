using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestPanelUI : MonoBehaviour
{
    [SerializeField] private Button _healButton;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private GameObject _mainChoicePanel;
    [SerializeField] private GameObject _cardListPanel;
    [SerializeField] private Transform _cardListContainer;
    [SerializeField] private GameObject _cardButtonPrefab;

    private EventManager _eventManager;

    private void OnEnable()
    {
        _eventManager ??= ServiceLocator.Instance.Get<EventManager>();
        ShowMainChoice();

        var player = ServiceLocator.Instance.Get<RunManager>()?.LocalPlayer;
        bool hasUpgradeable = player?.DeckController.Deck.Exists(c => c.baseCardData.CanUpgrade && !c.IsUpgraded) ?? false;
        if (_upgradeButton != null) _upgradeButton.interactable = hasUpgradeable;
    }

    private void ShowMainChoice()
    {
        if (_mainChoicePanel != null) _mainChoicePanel.SetActive(true);
        if (_cardListPanel != null) _cardListPanel.SetActive(false);
    }

    public void OnHealClicked()
    {
        var player = ServiceLocator.Instance.Get<RunManager>()?.LocalPlayer;
        if (player != null)
        {
            int healAmount = Mathf.FloorToInt(player.characterData.MaxHP * 0.3f);
            player.Heal(healAmount);
        }
        _eventManager?.CompleteEvent();
    }

    public void OnUpgradeClicked()
    {
        var player = ServiceLocator.Instance.Get<RunManager>()?.LocalPlayer;
        if (player == null) { _eventManager?.CompleteEvent(); return; }

        var upgradeable = player.DeckController.Deck.FindAll(c => c.baseCardData.CanUpgrade && !c.IsUpgraded);
        if (upgradeable.Count == 0) { _eventManager?.CompleteEvent(); return; }

        if (_mainChoicePanel != null) _mainChoicePanel.SetActive(false);
        if (_cardListPanel != null) _cardListPanel.SetActive(true);
        BuildCardList(upgradeable);
    }

    private void BuildCardList(List<CardInstance> cards)
    {
        if (_cardListContainer == null || _cardButtonPrefab == null) return;

        foreach (Transform child in _cardListContainer)
            Destroy(child.gameObject);

        foreach (var card in cards)
        {
            var captured = card;
            var go = Instantiate(_cardButtonPrefab, _cardListContainer);
            var btn = go.GetComponent<Button>();
            var label = go.GetComponentInChildren<Text>();
            if (label != null) label.text = $"{card.baseCardData.Name}（升級）";
            btn?.onClick.AddListener(() => OnCardSelected(captured));
        }
    }

    private void OnCardSelected(CardInstance card)
    {
        card.Upgrade();
        _eventManager?.CompleteEvent();
    }
}
