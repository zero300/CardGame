using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    [Header("UI 參考")]
    public Text HpText;
    public Text BlockText;
    public Image characterImage;
    public Text NameText;
    public GameObject blockIcon; // 護甲圖標，沒護甲時隱藏

    private CharacterInstance _characterInstance;

    private void Awake()
    {
        // 確保有 CanvasGroup 用於互動控制
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            gameObject.AddComponent<CanvasGroup>();
        }
    }
    public void Setup(CharacterInstance instance)
    {
        _characterInstance = instance;
        // 初始化 UI 顯示，這裡假設 CharacterData 有 CharacterName 和 CharacterSprite 屬性
        if (_characterInstance == null || _characterInstance.characterData == null)
        {
            Debug.LogWarning("CharacterUI: 傳入的 CharacterInstance 或其 CharacterData 為 null，無法初始化 UI");
            return;
        }
        // 初始獲取UI位址資料
        HpText = transform.Find("HpText").GetComponent<Text>();
        NameText = transform.Find("NameText").GetComponent<Text>();
        BlockText = transform.Find("BlockText").GetComponent<Text>();
        characterImage = transform.Find("CharacterImage").GetComponent<Image>();

        // 初始化基本資料  sprite 先不設定
        // characterImage.sprite = _characterInstance.characterData.CharacterSprite;
        UpdateHPUI(_characterInstance.CurrentHP, _characterInstance.characterData.MaxHP);
        UpdateBlockUI(_characterInstance.CurrentBlock);
        NameText.text = _characterInstance.characterData.CharacterName;

        // 訂閱事件
        _characterInstance.OnHPChanged += UpdateHPUI;
        _characterInstance.OnBlockChanged += UpdateBlockUI;
        _characterInstance.OnDamageTaken += PlayHitAnimation;
        _characterInstance.OnDeath += PlayDeathAnimation;
    }
    private void OnDestroy()
    {
        // 記得解除訂閱，避免 Memory Leak
        if (_characterInstance != null)
        {
            _characterInstance.OnHPChanged -= UpdateHPUI;
            _characterInstance.OnBlockChanged -= UpdateBlockUI;
            _characterInstance.OnDamageTaken -= PlayHitAnimation;
            _characterInstance.OnDeath -= PlayDeathAnimation;
        }
    }
    public CharacterInstance GetCharacterInstance()
    {
        return _characterInstance;
    }
    private void UpdateHPUI(int currentHP, int maxHP)
    {
        HpText.text = $"{currentHP} / {maxHP}";
    }
    private void UpdateBlockUI(int currentBlock)
    {
        BlockText.text = currentBlock.ToString();
        // blockIcon.SetActive(currentBlock > 0); // 只有數值 > 0 時才顯示護甲圖示
    }
    private void PlayHitAnimation(int damageTaken)
    {
        Debug.Log($"CharacterUI : {_characterInstance.characterData.CharacterName} 受到了 {damageTaken} 點傷害！");
        // 這裡可以加入：
        // 1. 角色閃爍紅光
        // 2. 螢幕震動
        // 3. 生成浮動傷害數字 (Floating Text)
    }
    private void PlayDeathAnimation()
    {
        Debug.Log($"CharacterUI : {_characterInstance.characterData.CharacterName} 死亡！");
        // 處理死亡動畫或隱藏物件
        Destroy(gameObject);
    }
}
