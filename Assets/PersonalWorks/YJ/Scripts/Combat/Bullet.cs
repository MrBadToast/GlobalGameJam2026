using UnityEngine;

/// <summary>
/// 원거리 공격 총알. CircleCollider2D (IsTrigger) 필요.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifeTime = 5f;

    [Header("Sound")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;

    private IEntity owner;
    private float baseDamage;
    private LayerMask targetLayer;
    private bool isInitialized = false;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // Collider를 트리거로 설정
        var collider = GetComponent<CircleCollider2D>();
        collider.isTrigger = true;
    }

    /// <summary>
    /// 총알 초기화
    /// </summary>
    /// <param name="owner">발사한 엔티티</param>
    /// <param name="direction">발사 방향</param>
    /// <param name="speed">이동 속도</param>
    /// <param name="baseDamage">기본 데미지</param>
    /// <param name="targetLayer">타겟 레이어</param>
    public void Initialize(IEntity owner, Vector2 direction, float speed, float baseDamage, LayerMask targetLayer)
    {
        this.owner = owner;
        this.baseDamage = baseDamage;
        this.targetLayer = targetLayer;
        this.isInitialized = true;

        // 속도 설정
        rb.linearVelocity = direction.normalized * speed;

        // 방향에 맞게 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 수명 후 파괴
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInitialized) return;

        // 레이어 마스크 체크
        if ((targetLayer.value & (1 << other.gameObject.layer)) == 0) return;

        // IEntity 확인
        if (!other.TryGetComponent<IEntity>(out var target)) return;

        // 생존 확인
        if (target.IsDead) return;

        // 데미지 계산 (표정 스탯 + 보너스 스탯 + 상성)
        float finalDamage = ExpressionData.CalculateDamage(
            baseDamage,
            owner.Expression,
            target.Expression,
            owner.BonusStats,
            target.BonusStats
        );

        // 피격 방향
        Vector2 direction = ((Vector2)target.GameObject.transform.position - (Vector2)transform.position).normalized;

        // 데미지 적용
        target.TakeDamage(finalDamage, direction);

        // 피격 사운드
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, soundVolume);
        }

        // 총알 파괴
        Destroy(gameObject);
    }
}
