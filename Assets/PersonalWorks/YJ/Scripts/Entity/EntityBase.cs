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

    [Header("Sound")]
    [SerializeField] protected AudioClip hitSound;
    [SerializeField] protected AudioClip deathSound;
    [SerializeField, Range(0f, 1f)] protected float soundVolume = 1f;

    [Header("Drop")]
    [SerializeField] protected GameObject moneyPrefab;
    [SerializeField] protected int dropMoneyCount = 1;

    // 추가 스탯 (아이템, 버프 등)
    protected EntityStats bonusStats = new EntityStats();

    // ========== IEntity 구현 ==========
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public ExpressionType Expression => expression;
    public bool IsDead { get; protected set; } = false;
    public Animator Animator => animator;
    public GameObject GameObject => gameObject;
    public EntityStats BonusStats => bonusStats;

    // ========== 공통 함수 ==========

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float damage, Vector2 direction)
    {
        if (IsDead) return;

        currentHealth -= damage;

        // 피격 사운드
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, soundVolume);
        }

        // 피격 애니메이션
        animator?.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public virtual void Die()
    {
        if (IsDead) return;

        IsDead = true;

        // 죽음 사운드
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position, soundVolume);
        }

        // 죽음 애니메이션
        animator?.SetTrigger("Death");

        // 돈 드롭
        DropMoney();

        OnDeath();
    }

    protected virtual void DropMoney()
    {
        if (moneyPrefab == null) return;

        for (int i = 0; i < dropMoneyCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
            Vector3 spawnPos = transform.position + (Vector3)randomOffset;
            Instantiate(moneyPrefab, spawnPos, Quaternion.identity);
        }
    }

    protected abstract void OnDeath();
}
