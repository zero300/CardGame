using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CardInstance _cardInstance;
    private CardManager _cardManager;

    private Vector2 _originalPosition;
    private CanvasGroup _canvasGroup;

    #region 方便卡片更新畫面儲存的GameObject 
    // public GameObject 
    private Transform cardTitle ;
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
        cardNameText = cardTitle.GetChild(0).GetChild(0).GetComponent<Text>() ;
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

        string queryText = string.Empty ;
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
    // 偵測滑鼠下方的敵人 (從 UI 螢幕座標轉換到 2D 世界座標)
    private CharacterUI DetectEnemyUnderMouse()
    {
        // 將滑鼠螢幕座標轉為世界座標
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 發射一條 2D 射線
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            // 看看撞到的物件身上有沒有 CharacterUI
            return hit.collider.GetComponent<CharacterUI>();
        }
        return null;
    }
    private void ReturnToHand()
    {
        transform.position = _originalPosition;
        // (如果有寫 UpdateHandLayout，這裡可以呼叫 Manager 重新排版)
    }
}
