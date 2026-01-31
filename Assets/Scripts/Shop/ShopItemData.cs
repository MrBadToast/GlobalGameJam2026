using UnityEngine;

/// <summary>
/// 상점 아이템 데이터 (ScriptableObject)
/// </summary>
[CreateAssetMenu(fileName = "NewShopItem", menuName = "Game/Shop Item Data")]
public class ShopItemData : ScriptableObject
{
    [Header("Item")]
    public ItemData itemData;

    [Header("Shop")]
    public int price;

    [Header("Category")]
    public ShopCategory category;
}

/// <summary>
/// 상점 카테고리
/// </summary>
public enum ShopCategory
{
    Potion,      // 물약
    BuffWhite,   // 하양 버프
    BuffBlue,    // 파랑 버프
    BuffYellow,  // 노랑 버프
    BuffRed      // 빨강 버프
}
