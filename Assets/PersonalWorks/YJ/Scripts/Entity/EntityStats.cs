using System;
using UnityEngine;

/// <summary>
/// 개체별 추가 스탯 (아이템, 버프 등으로 증가)
/// </summary>
[Serializable]
public class EntityStats
{
    [Tooltip("추가 피격 데미지 가중치")]
    public float damageTakenModifier;

    [Tooltip("추가 공격력 가중치")]
    public float attackModifier;

    [Tooltip("추가 이동속도 가중치")]
    public float moveSpeedModifier;

    [Tooltip("추가 공격속도 가중치")]
    public float attackSpeedModifier;

    public EntityStats()
    {
        damageTakenModifier = 0f;
        attackModifier = 0f;
        moveSpeedModifier = 0f;
        attackSpeedModifier = 0f;
    }

    /// <summary>
    /// 모든 스탯 초기화
    /// </summary>
    public void Reset()
    {
        damageTakenModifier = 0f;
        attackModifier = 0f;
        moveSpeedModifier = 0f;
        attackSpeedModifier = 0f;
    }

    /// <summary>
    /// 다른 스탯을 더함
    /// </summary>
    public void Add(EntityStats other)
    {
        damageTakenModifier += other.damageTakenModifier;
        attackModifier += other.attackModifier;
        moveSpeedModifier += other.moveSpeedModifier;
        attackSpeedModifier += other.attackSpeedModifier;
    }

    /// <summary>
    /// 피격 데미지 가중치 추가
    /// </summary>
    public void AddDamageTaken(float value) => damageTakenModifier += value;

    /// <summary>
    /// 공격력 가중치 추가
    /// </summary>
    public void AddAttack(float value) => attackModifier += value;

    /// <summary>
    /// 이동속도 가중치 추가
    /// </summary>
    public void AddMoveSpeed(float value) => moveSpeedModifier += value;

    /// <summary>
    /// 공격속도 가중치 추가
    /// </summary>
    public void AddAttackSpeed(float value) => attackSpeedModifier += value;
}
