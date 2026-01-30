/// <summary>
/// 캐릭터 표정 타입
/// </summary>
public enum ExpressionType
{
    Neutral = 0,  // 무표정 (하얀색) - 원거리
    Happy = 1,    // 웃음 (노랑색) - 원거리
    Sad = 2,      // 슬픔 (하늘색) - 근거리
    Angry = 3     // 분노 (빨간색) - 근거리
}

/// <summary>
/// 공격 거리 타입
/// </summary>
public enum AttackRangeType
{
    Ranged,  // 원거리
    Melee    // 근거리
}
