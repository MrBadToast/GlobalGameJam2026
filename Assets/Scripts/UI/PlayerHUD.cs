using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 플레이어 HUD - HP, 물약, 골드 표시
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image playerSpriteImage;              // 플레이어 스프라이트 표시용 이미지
    [SerializeField] private Sprite[] playerSprites = new Sprite[4];  // 4종류 플레이어 스프라이트 텍스쳐
    [SerializeField] private Slider hpBar;
    [SerializeField] private TextMeshProUGUI currencyText;

    [Header("Potion Slot")]
    [SerializeField] private Image potionIcon;
    [SerializeField] private TextMeshProUGUI potionCountText;

    private Player_Topdown player;

    private void Start()
    {
        // 로컬 플레이어 찾기
        FindLocalPlayer();
    }

    private void Update()
    {
        if (player == null)
        {
            FindLocalPlayer();
            return;
        }

        UpdateHUD();
    }

    private void FindLocalPlayer()
    {
        player = Player_Topdown.Local;

        if (player != null)
        {
            // 플레이어 이벤트 구독
            player.OnPotionChanged += UpdatePotionSlot;
            player.OnExpressionChanged += OnExpressionChanged;

            // 초기 업데이트
            UpdateHUD();
            UpdatePotionSlot();
            SetPlayerSprite((int)player.Expression);
        }
    }

    private void OnExpressionChanged(int expressionIndex)
    {
        SetPlayerSprite(expressionIndex);
    }

    private void UpdateHUD()
    {
        // HP 바 업데이트
        if (hpBar != null)
        {
            hpBar.maxValue = player.MaxHealth;
            hpBar.value = player.CurrentHealth;
        }

        // 골드 업데이트
        if (currencyText != null)
        {
            currencyText.text = "$ " + player.Money.ToString();
        }
    }

    private void UpdatePotionSlot()
    {
        if (player == null) return;

        int selectedIndex = player.SelectedPotionIndex;
        ItemData selectedPotion = player.SelectedPotionData;
        int count = player.SelectedPotionCount;

        // 아이콘 업데이트
        if (potionIcon != null)
        {
            if (selectedPotion != null && selectedPotion.icon != null)
            {
                potionIcon.sprite = selectedPotion.icon;
                potionIcon.enabled = true;
            }
            else
            {
                potionIcon.enabled = false;
            }
        }

        // 개수 업데이트
        if (potionCountText != null)
        {
            potionCountText.text = count.ToString();
        }
    }

    /// <summary>
    /// 플레이어 스프라이트 설정 (0~3)
    /// </summary>
    public void SetPlayerSprite(int index)
    {
        if (playerSpriteImage == null) return;
        if (index < 0 || index >= playerSprites.Length) return;

        if (playerSprites[index] != null)
        {
            playerSpriteImage.sprite = playerSprites[index];
            playerSpriteImage.preserveAspect = true;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (player != null)
        {
            player.OnPotionChanged -= UpdatePotionSlot;
            player.OnExpressionChanged -= OnExpressionChanged;
        }
    }
}
