/// <summary>
/// 아이템 종류
/// </summary>
public enum ItemType
{
    // 물약 (소모품)
    Potion1,
    Potion2,
    Potion3,

    // 영구 버프
    BuffNeutral,  // 하양
    BuffHappy,    // 노랑
    BuffSad,      // 파랑
    BuffAngry     // 빨강
}

public static class ItemTypeExtensions
{
    /// <summary>
    /// 버프 아이템의 대상 표정 인덱스 반환 (-1이면 해당 없음)
    /// </summary>
    public static int GetExpressionIndex(this ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.BuffNeutral: return 0;  // Neutral
            case ItemType.BuffHappy: return 1;    // Happy
            case ItemType.BuffSad: return 2;      // Sad
            case ItemType.BuffAngry: return 3;    // Angry
            default: return -1;
        }
    }
}

/// <summary>
/// 아이템 카테고리
/// </summary>
public enum ItemCategory
{
    Potion,  // 소모품
    Buff     // 즉시 사용 (영구 버프)
}
