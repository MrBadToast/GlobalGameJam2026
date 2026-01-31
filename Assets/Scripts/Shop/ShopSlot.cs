using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 상점 슬롯 - 개별 아이템 표시 및 구매
/// </summary>
public class ShopSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button buyButton;

    private ShopItemData shopItemData;

    /// <summary>
    /// 슬롯 초기화
    /// </summary>
    public void Initialize(ShopItemData data)
    {
        shopItemData = data;

        if (data == null || data.itemData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // UI 업데이트
        if (iconImage != null && data.itemData.icon != null)
        {
            iconImage.sprite = data.itemData.icon;
        }

        if (nameText != null)
        {
            nameText.text = data.itemData.itemName;
        }

        if (priceText != null)
        {
            priceText.text = data.price.ToString();
        }

        // 구매 버튼 이벤트
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }
    }

    private void OnBuyClicked()
    {
        if (shopItemData == null) return;

        var player = Player_Topdown.Instance;
        if (player == null) return;

        // 돈 체크
        if (player.Money < shopItemData.price)
        {
            Debug.Log("돈이 부족합니다!");
            return;
        }

        // 구매 처리
        bool success = player.AddItem(shopItemData.itemData);

        if (success)
        {
            // 돈 차감
            player.AddMoney(-shopItemData.price);
            Debug.Log($"{shopItemData.itemData.itemName} 구매 완료!");
        }
        else
        {
            Debug.Log("아이템을 추가할 수 없습니다!");
        }
    }

    // ========== 툴팁 ==========

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (shopItemData?.itemData == null) return;

        TooltipUI.Instance?.Show(shopItemData.itemData, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance?.Hide();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        TooltipUI.Instance?.UpdatePosition(eventData.position);
    }
}
