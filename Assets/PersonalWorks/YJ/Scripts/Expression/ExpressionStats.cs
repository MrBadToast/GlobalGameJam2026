using System;
using UnityEngine;

/// <summary>
/// 표정별 기본 스탯 가중치
/// </summary>
[Serializable]
public class ExpressionStats
{
    [Tooltip("공격 거리 타입")]
    public AttackRangeType attackRange;

    [Tooltip("피격 데미지 가중치 (0.2 = +20%, -0.4 = -40%)")]
    [Range(-1f, 1f)]
    public float damageTakenModifier;

    [Tooltip("공격력 가중치 (0.5 = +50%, -0.25 = -25%)")]
    [Range(-1f, 1f)]
    public float attackModifier;

    [Tooltip("이동속도 가중치")]
    [Range(-1f, 1f)]
    public float moveSpeedModifier;

    [Tooltip("공격속도 가중치")]
    [Range(-1f, 1f)]
    public float attackSpeedModifier;

    public ExpressionStats(
        AttackRangeType attackRange,
        float damageTakenModifier,
        float attackModifier,
        float moveSpeedModifier,
        float attackSpeedModifier)
    {
        this.attackRange = attackRange;
        this.damageTakenModifier = damageTakenModifier;
        this.attackModifier = attackModifier;
        this.moveSpeedModifier = moveSpeedModifier;
        this.attackSpeedModifier = attackSpeedModifier;
    }
}
