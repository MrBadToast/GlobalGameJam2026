using System;
using UnityEngine;

/// <summary>
/// 상성에 따른 스탯 가중치
/// </summary>
[Serializable]
public class MatchupModifier
{
    [Tooltip("피격 데미지 가중치 (상성에 의한 추가 보정)")]
    public float damageTakenModifier;

    [Tooltip("공격 데미지 가중치 (상성에 의한 추가 보정)")]
    public float damageDealtModifier;

    public MatchupModifier(float damageTakenModifier, float damageDealtModifier)
    {
        this.damageTakenModifier = damageTakenModifier;
        this.damageDealtModifier = damageDealtModifier;
    }

    /// <summary>
    /// 기본값 (상성 효과 없음)
    /// </summary>
    public static MatchupModifier Default => new MatchupModifier(0f, 0f);

    /// <summary>
    /// 상성 유리 (공격 +30%, 피격 -20%)
    /// </summary>
    public static MatchupModifier Advantage => new MatchupModifier(-0.2f, 0.3f);

    /// <summary>
    /// 상성 불리 (공격 -30%, 피격 +20%)
    /// </summary>
    public static MatchupModifier Disadvantage => new MatchupModifier(0.2f, -0.3f);
}
