using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 상점 데이터베이스 - 모든 ShopItemData를 관리
/// </summary>
[CreateAssetMenu(fileName = "ShopDatabase", menuName = "Game/Shop Database")]
public class ShopDatabase : ScriptableObject
{
    [Header("Settings")]
    [SerializeField, FolderPath]
    private string shopItemFolderPath = "Assets/Data/ShopItems";

    [Header("Shop Items")]
    [SerializeField, ReadOnly]
    private List<ShopItemData> allItems = new List<ShopItemData>();

    public IReadOnlyList<ShopItemData> AllItems => allItems;

    /// <summary>
    /// 카테고리별 아이템 가져오기
    /// </summary>
    public List<ShopItemData> GetItemsByCategory(ShopCategory category)
    {
        List<ShopItemData> result = new List<ShopItemData>();
        foreach (var item in allItems)
        {
            if (item != null && item.category == category)
            {
                result.Add(item);
            }
        }
        return result;
    }

    /// <summary>
    /// 모든 물약 아이템
    /// </summary>
    public List<ShopItemData> GetPotionItems() => GetItemsByCategory(ShopCategory.Potion);

    /// <summary>
    /// 모든 버프 아이템
    /// </summary>
    public List<ShopItemData> GetBuffItems()
    {
        List<ShopItemData> result = new List<ShopItemData>();
        foreach (var item in allItems)
        {
            if (item != null && item.category != ShopCategory.Potion)
            {
                result.Add(item);
            }
        }
        return result;
    }

#if UNITY_EDITOR
    [Button("폴더에서 ShopItemData 불러오기", ButtonSizes.Large)]
    private void LoadItemsFromFolder()
    {
        allItems.Clear();

        if (string.IsNullOrEmpty(shopItemFolderPath))
        {
            Debug.LogWarning("폴더 경로가 설정되지 않았습니다.");
            return;
        }

        // 폴더 내 모든 ShopItemData 찾기
        string[] guids = AssetDatabase.FindAssets("t:ShopItemData", new[] { shopItemFolderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ShopItemData item = AssetDatabase.LoadAssetAtPath<ShopItemData>(path);

            if (item != null)
            {
                allItems.Add(item);
            }
        }

        // 변경사항 저장
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log($"ShopDatabase: {allItems.Count}개의 아이템을 불러왔습니다.");
    }

    [Button("목록 초기화")]
    private void ClearItems()
    {
        allItems.Clear();
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
#endif
}
