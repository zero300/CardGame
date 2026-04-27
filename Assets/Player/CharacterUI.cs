using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    [Header("UI 參考")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI blockText;
    public GameObject blockIcon; // 護甲圖標，沒護甲時隱藏
    public Image characterImage;

    private CharacterInstance _characterInstance;

    public void Setup(CharacterInstance instance)
    {
        _characterInstance = instance;

        // 初始化基本資料  sprite 先不設定
        // characterImage.sprite = _characterInstance.characterData.CharacterSprite;
        UpdateHPUI(_characterInstance.CurrentHP, _characterInstance.characterData.MaxHP);
        UpdateBlockUI(_characterInstance.CurrentBlock);

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
        hpText.text = $"{currentHP} / {maxHP}";
    }

    private void UpdateBlockUI(int currentBlock)
    {
        blockText.text = currentBlock.ToString();
        blockIcon.SetActive(currentBlock > 0); // 只有數值 > 0 時才顯示護甲圖示
    }

    private void PlayHitAnimation(int damageTaken)
    {
        Debug.Log($"{_characterInstance.characterData.CharacterName} 受到了 {damageTaken} 點傷害！");
        // 這裡可以加入：
        // 1. 角色閃爍紅光
        // 2. 螢幕震動
        // 3. 生成浮動傷害數字 (Floating Text)
    }

    private void PlayDeathAnimation()
    {
        Debug.Log($"{_characterInstance.characterData.CharacterName} 死亡！");
        // 處理死亡動畫或隱藏物件
    }
}
