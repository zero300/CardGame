using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum CardListMode
{
    ViewDeck,       // 查看牌組（只讀）
    UpgradeCard     // 升級卡牌（選擇一張）
}

public class CardListPanelUI : MonoBehaviour
{
    [SerializeField] private Text _titleText;
    [SerializeField] private RectTransform _cardListContainer;
    [SerializeField] private GameObject _cardButtonPrefab;
    [SerializeField] private Button _closeButton;
    [SerializeField] private float _cardSpacing = 10f;

    private CardListMode _mode;
    private Action<CardInstance> _onCardSelected;

    private void Awake()
    {
        _closeButton?.onClick.AddListener(Close);
    }

    public void Open(CardListMode mode, List<CardInstance> cards, Action<CardInstance> onCardSelected = null)
    {
        _mode = mode;
        _onCardSelected = onCardSelected;

        if (_titleText != null)
            _titleText.text = mode switch
            {
                CardListMode.ViewDeck => "當前牌組",
                CardListMode.UpgradeCard => "選擇升級卡牌",
                _ => "卡牌列表"
            };

        // 先啟用 Panel，讓 Unity 完成 Layout 計算，Container 的 rect.width 才會有正確值
        gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();

        BuildList(cards);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void BuildList(List<CardInstance> cards)
    {
        if (_cardListContainer == null || _cardButtonPrefab == null) return;

        foreach (Transform child in _cardListContainer)
            Destroy(child.gameObject);

        // 從 Prefab 的 sizeDelta 取得卡片固定尺寸（Prefab 無父物件時 rect 不可靠，sizeDelta 永遠有效）
        var prefabRt = _cardButtonPrefab.GetComponent<RectTransform>();
        if (prefabRt == null) return;
        Vector2 cardSize = prefabRt.sizeDelta;

        // 取得 Container 可用寬度（Panel 啟用後才能讀到正確值）
        float containerW = _cardListContainer.rect.width;

        // 計算一排能放幾欄
        int cols = Mathf.Max(1, Mathf.FloorToInt((containerW + _cardSpacing) / (cardSize.x + _cardSpacing)));

        for (int i = 0; i < cards.Count; i++)
        {
            int col = i % cols;
            int row = i / cols;

            var captured = cards[i];
            var go = Instantiate(_cardButtonPrefab, _cardListContainer);
            var rt = go.GetComponent<RectTransform>();
            var btn = go.GetComponent<Button>();

            var listCardUI = go.GetComponent<ListCardUI>();
            listCardUI.BindCardInstance(captured);
            // 錨點固定在 Container 左上角，向右下排列
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(
                col * (cardSize.x + _cardSpacing),
                -row * (cardSize.y + _cardSpacing)
            );

            // bool selectable = _mode == CardListMode.UpgradeCard && captured.baseCardData.CanUpgrade && !captured.IsUpgraded;
            if (btn != null) btn.onClick.AddListener(() => OnCardClicked(captured));
        }

        // 自動撐高 Content，讓 ScrollRect 能正確捲動
        int totalRows = Mathf.CeilToInt((float)cards.Count / cols);
        float contentH = totalRows * cardSize.y + Mathf.Max(0, totalRows - 1) * _cardSpacing;
        _cardListContainer.sizeDelta = new Vector2(_cardListContainer.sizeDelta.x, contentH);
    }

    private void OnCardClicked(CardInstance card)
    {
        _onCardSelected?.Invoke(card);
        Close();
    }
}
