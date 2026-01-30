using UnityEngine;

/// <summary>
/// 플레이어/몬스터 공통 인터페이스
/// </summary>
public interface IEntity
{
    // 속성
    float MaxHealth { get; }
    float CurrentHealth { get; }
    ExpressionType Expression { get; }
    WeaponType Weapon { get; }
    bool IsDead { get; }
    Animator Animator { get; }

    // 메서드
    void TakeDamage(float damage);
}
