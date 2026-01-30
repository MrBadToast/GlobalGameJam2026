using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Monster용 베이스 클래스 (싱글톤이 아닌 엔티티용)
/// </summary>
public abstract class EntityBase : SerializedMonoBehaviour, IEntity
{
    // ========== 공통 변수 ==========
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 100f;
    protected float currentHealth;

    [Header("Expression")]
    [SerializeField] protected ExpressionType expression;

    [Header("Components")]
    [SerializeField] protected Animator animator;

    // ========== IEntity 구현 ==========
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public ExpressionType Expression => expression;
    public WeaponType Weapon => expression.ToWeapon();  // 표정에서 자동 파생
    public bool IsDead { get; protected set; } = false;
    public Animator Animator => animator;

    // ========== 공통 함수 ==========

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float damage, Vector2 direction)
    {
        if (IsDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    protected virtual void Die()
    {
        IsDead = true;
        animator?.SetTrigger("Death");
        OnDeath();
    }

    protected abstract void OnDeath();
}
