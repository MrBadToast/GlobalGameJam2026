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
        if (shopItemData == null || shopItemData.itemData == null) return;

        var player = Player_Topdown.Local;
        if (player == null) return;

        // 돈 체크 (로컬에서 먼저 확인)
        if (player.Money < shopItemData.price)
        {
            Debug.Log("돈이 부족합니다!");
            return;
        }

        var item = shopItemData.itemData;

        // 카테고리에 따라 다른 RPC 호출
        if (item.category == ItemCategory.Potion)
        {
            // 물약 구매
            int potionIndex = GetPotionIndex(item.itemType);
            if (potionIndex >= 0)
            {
                player.RPC_BuyItem(shopItemData.price, potionIndex);
                Debug.Log($"{item.itemName} 구매 완료!");
            }
        }
        else if (item.category == ItemCategory.Buff)
        {
            // 버프 대상 표정 인덱스 가져오기
            int expressionIndex = item.itemType.GetExpressionIndex();
            if (expressionIndex < 0)
            {
                Debug.LogError($"잘못된 버프 아이템: {item.itemType}");
                return;
            }

            // 버프 구매 (해당 표정에만 적용)
            player.RPC_BuyBuff(
                shopItemData.price,
                expressionIndex,
                item.attackModifier,
                item.damageTakenModifier,
                item.moveSpeedModifier,
                item.attackSpeedModifier
            );
            Debug.Log($"{item.itemName} 구매 완료! (표정: {(ExpressionType)expressionIndex})");
        }
    }

    private int GetPotionIndex(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Potion1: return 0;
            case ItemType.Potion2: return 1;
            case ItemType.Potion3: return 2;
            default: return -1;
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
