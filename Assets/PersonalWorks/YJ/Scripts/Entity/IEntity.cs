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
    bool IsDead { get; }
    Animator Animator { get; }
    GameObject GameObject { get; }
    EntityStats BonusStats { get; }

    // 메서드
    void TakeDamage(float damage, Vector2 direction);
    void Die();
}
