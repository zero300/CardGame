using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ListCardUI : MonoBehaviour
{
    private CardInstance _cardInstance;
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
}
