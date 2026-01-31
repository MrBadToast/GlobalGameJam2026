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

/// <summary>
/// 아이템 카테고리
/// </summary>
public enum ItemCategory
{
    Potion,  // 소모품
    Buff     // 즉시 사용 (영구 버프)
}
