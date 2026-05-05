using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CardInstance _cardInstance;
    private CardManager _cardManager;

    private Vector2 _originalPosition;
    private CanvasGroup _canvasGroup;


    #region 方便卡片更新畫面儲存的GameObject 
    // public GameObject 
    private Transform cardTitle;
    private Transform cardBody;
    private Text cardNameText;
    private RawImage cardRarity;
    private Text cardDescriptionText;
    /* 基礎UI結構
     * -- Card
     *   -- CardTitle
     *     -- CardNameImage
     *       -- CardNameText
     *     -- CardRarity
     *   -- CardBody
     *     -- CardImage
     *     -- CardDescriptionImage
     *       -- CardDescriptionText
    */
    #endregion

    public void Awake()
    {
        // CanvasGroup 用於在拖曳時關閉射線阻擋，這樣滑鼠才能點透卡牌，點到後面的敵人
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        cardTitle = transform.GetChild(0);
        cardBody = transform.GetChild(1);
        cardNameText = cardTitle.GetChild(0).GetChild(0).GetComponent<Text>();
        cardRarity = cardTitle.GetChild(1).GetComponent<RawImage>();
        cardDescriptionText = cardBody.GetChild(1).GetChild(0).GetComponent<Text>();
    }
    /// <summary>
    /// 綁定CardManager 
    /// </summary>
    /// <param name="cardManager"></param>
    public void BindCardManager(CardManager cardManager)
    {
        _cardManager = cardManager;
    }
    /// <summary>
    /// 綁定Card數值
    /// </summary>
    /// <param name="cardInstance"></param>
    public void BindCardInstance(CardInstance cardInstance)
    {
        _cardInstance = cardInstance;
        UpdateCardUI();
    }
    public CardInstance GetCardInstance()
    {
        return _cardInstance;
    }
    public CharacterInstance GetCardOwner()
    {
        return _cardInstance.Owner;
    }
    /// <summary>
    /// 更新卡牌的UI顯示
    /// </summary>
    public void UpdateCardUI()
    {
        // 若無_cardInstance，不進行更新
        if (_cardInstance == null)
        {
            Debug.LogWarning("CardInstance is null. Cannot update card UI.");
            return;
        }

        // 如果綁定位置有誤，不進行更新
        if (cardTitle == null || cardBody == null)
        {
            Debug.LogWarning("cardTitle or cardBody is null. Cannot update card UI.");
            return;
        }

        string queryText = string.Empty;
        // 設置卡牌名稱
        // CardTitle / CardNameImage / CardNameText
        queryText = "CardNameText";
        cardNameText.text = _cardInstance.baseCardData.Name;

        // 設置卡牌稀有度
        // CardTitle / CardRarity
        queryText = "CardRarity";
        Color color = Color.black;
        switch (_cardInstance.baseCardData.Rarity)
        {
            case CardRarity.Common:
                color = Color.gray;
                break;
            case CardRarity.Uncommon:
                color = Color.green;
                break;
            case CardRarity.Rare:
                color = Color.blue;
                break;
            case CardRarity.Epic:
                color = Color.magenta;
                break;
            case CardRarity.Legendary:
                color = Color.yellow;
                break;
            default: break;
        }
        cardRarity.color = color;
        // 設置卡牌描述
        queryText = "CardDescriptionText";
        cardDescriptionText.text = _cardInstance.baseCardData.Description;
    }
    public void UpdateCardPosition(Vector2 vector2)
    {
        // todo : 根據卡牌的狀態來更新卡牌的位置，例如在手牌區、場上、墓地等
        Debug.Log("Updating card position ...");
        transform.localPosition = vector2;
        /*
        cardObject.transform.position = position;
        cardObject.transform.localScale = isHandScale ? new Vector3(0.75f, 0.75f, 1) : Vector3.one;
        */
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalPosition = transform.position; // 記錄原始位置
        _canvasGroup.blocksRaycasts = false;    // 讓滑鼠射線穿透這張卡牌
    }
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition; // 卡牌跟著滑鼠走
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true; // 恢復射線阻擋

        // 偵測滑鼠放開時，點到了世界座標的什麼東西
        CharacterUI targetEnemy = DetectEnemyUnderMouse();

        if (targetEnemy != null)
        {
            // 點到了！交給 Manager 去判定能不能打出
            bool playSuccess = false;
            if (_cardManager != null)
            {
                playSuccess = _cardManager.TryPlayCard(this, targetEnemy);
            }
            if (!playSuccess)
            {
                // 如果費用不夠等原因打出失敗，彈回原位
                ReturnToHand();
            }
        }
        else
        {
            // 沒點到敵人，彈回原位
            ReturnToHand();
        }
    }
    private CharacterUI DetectEnemyUnderMouse()
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning("DetectEnemyUnderMouse: EventSystem.current is null.");
            return null;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        GraphicRaycaster raycaster = GetComponentInParent<GraphicRaycaster>();

        if (raycaster == null)
        {
            Debug.LogWarning("DetectEnemyUnderMouse: 找不到 GraphicRaycaster。");
            return null;
        }

        raycaster.Raycast(pointerData, raycastResults);

        // 遍歷所有 raycast 結果，優先找直接的 CharacterUI
        foreach (var result in raycastResults)
        {
            CharacterUI characterUI = result.gameObject.GetComponent<CharacterUI>();
            if (characterUI != null)
            {
                return characterUI;
            }
        }

        // 如果沒有直接找到，查找父物件
        foreach (var result in raycastResults)
        {
            CharacterUI characterUI = result.gameObject.GetComponentInParent<CharacterUI>();
            if (characterUI != null)
            {
                return characterUI;
            }
        }

        return null;
    }
    private void ReturnToHand()
    {
        transform.position = _originalPosition;
        // (如果有寫 UpdateHandLayout，這裡可以呼叫 Manager 重新排版)
    }
}
