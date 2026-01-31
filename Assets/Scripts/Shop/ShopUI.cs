using UnityEngine;

/// <summary>
/// 상점 UI - 미리 배치된 12개 슬롯 관리
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("Pre-placed Slots (12개)")]
    [SerializeField] private ShopSlot[] slots = new ShopSlot[12];

    private void Update()
    {
        // ESC로 상점 닫기
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    /// <summary>
    /// 상점 열기
    /// </summary>
    public void OpenShop()
    {
        gameObject.SetActive(true);

        // 플레이어 입력 비활성화
        if (Player_Topdown.Local != null)
        {
            Player_Topdown.Local.SetInputEnabled(false);
        }
    }

    /// <summary>
    /// 상점 닫기
    /// </summary>
    public void CloseShop()
    {
        gameObject.SetActive(false);

        // 플레이어 입력 활성화
        if (Player_Topdown.Local != null)
        {
            Player_Topdown.Local.SetInputEnabled(true);
        }
    }

    /// <summary>
    /// 특정 슬롯의 아이템 변경 (런타임용)
    /// </summary>
    public void SetSlotItem(int slotIndex, ShopItemData itemData)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return;
        if (slots[slotIndex] == null) return;

        slots[slotIndex].SetItemData(itemData);
    }

    /// <summary>
    /// 슬롯 가져오기
    /// </summary>
    public ShopSlot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return null;
        return slots[index];
    }

    /// <summary>
    /// 전체 슬롯 수
    /// </summary>
    public int SlotCount => slots.Length;
}
