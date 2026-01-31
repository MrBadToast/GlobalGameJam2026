using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 물약 UI - 선택된 물약 아이콘 및 개수 표시
/// HPPotionSlot > Background, Icon, Count 구조
/// </summary>
public class PotionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image potionIcon;           // Icon (물약 이미지)
    [SerializeField] private TextMeshProUGUI countText;  // Count (남은 개수)

    [Header("Settings")]
    [SerializeField] private Sprite emptySlotSprite;

    private void Start()
    {
        if (Player_Topdown.Local != null)
        {
            Player_Topdown.Local.OnPotionChanged += UpdateUI;
            UpdateUI();
        }
    }

    private void OnDestroy()
    {
        if (Player_Topdown.Local != null)
        {
            Player_Topdown.Local.OnPotionChanged -= UpdateUI;
        }
    }

    private void UpdateUI()
    {
        var player = Player_Topdown.Local;
        if (player == null) return;

        // 선택된 물약 데이터
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
                potionIcon.sprite = emptySlotSprite;
                potionIcon.enabled = emptySlotSprite != null;
            }
        }

        // 개수 업데이트
        if (countText != null)
        {
            countText.text = count.ToString();
        }
    }
}
