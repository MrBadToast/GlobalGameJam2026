using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점 UI - 카테고리별 아이템 표시
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("Category Roots")]
    [SerializeField] private Transform potionRoot;
    [SerializeField] private Transform whiteRoot;
    [SerializeField] private Transform blueRoot;
    [SerializeField] private Transform yellowRoot;
    [SerializeField] private Transform redRoot;

    [Header("Prefab")]
    [SerializeField] private GameObject shopSlotPrefab;

    [Header("Shop Data")]
    [SerializeField] private ShopDatabase shopDatabase;

    private List<ShopSlot> spawnedSlots = new List<ShopSlot>();

    private void Start()
    {
        InitializeShop();
    }

    /// <summary>
    /// 상점 초기화 - 카테고리별로 슬롯 생성
    /// </summary>
    public void InitializeShop()
    {
        ClearShop();

        if (shopDatabase == null)
        {
            Debug.LogWarning("ShopDatabase가 설정되지 않았습니다.");
            return;
        }

        foreach (var shopItem in shopDatabase.AllItems)
        {
            if (shopItem == null) continue;

            Transform root = GetRootByCategory(shopItem.category);
            if (root == null) continue;

            CreateSlot(shopItem, root);
        }
    }

    /// <summary>
    /// 상점 슬롯 모두 제거
    /// </summary>
    public void ClearShop()
    {
        foreach (var slot in spawnedSlots)
        {
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }
        spawnedSlots.Clear();
    }

    /// <summary>
    /// 슬롯 생성
    /// </summary>
    private void CreateSlot(ShopItemData shopItem, Transform root)
    {
        if (shopSlotPrefab == null) return;

        GameObject slotObj = Instantiate(shopSlotPrefab, root);
        ShopSlot slot = slotObj.GetComponent<ShopSlot>();

        if (slot != null)
        {
            slot.Initialize(shopItem);
            spawnedSlots.Add(slot);
        }
    }

    /// <summary>
    /// 카테고리에 맞는 루트 반환
    /// </summary>
    private Transform GetRootByCategory(ShopCategory category)
    {
        switch (category)
        {
            case ShopCategory.Potion:
                return potionRoot;
            case ShopCategory.BuffWhite:
                return whiteRoot;
            case ShopCategory.BuffBlue:
                return blueRoot;
            case ShopCategory.BuffYellow:
                return yellowRoot;
            case ShopCategory.BuffRed:
                return redRoot;
            default:
                return null;
        }
    }

    /// <summary>
    /// 런타임에 상점 슬롯 추가
    /// </summary>
    public void AddShopItem(ShopItemData shopItem)
    {
        if (shopItem == null) return;

        Transform root = GetRootByCategory(shopItem.category);
        if (root != null)
        {
            CreateSlot(shopItem, root);
        }
    }

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
        if (Player_Topdown.Instance != null)
        {
            Player_Topdown.Instance.SetInputEnabled(false);
        }
    }

    /// <summary>
    /// 상점 닫기
    /// </summary>
    public void CloseShop()
    {
        gameObject.SetActive(false);

        // 플레이어 입력 활성화
        if (Player_Topdown.Instance != null)
        {
            Player_Topdown.Instance.SetInputEnabled(true);
        }
    }
}
