using Fusion;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[RequireComponent(typeof(CircleCollider2D))]
public class EnemyProjectile : NetworkBehaviour
{
    [SerializeField] private float travelSpeed = 10f;
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private float projectileLifetime = 5f;

    private CircleCollider2D circleCollider;
    private void Start()
    {
        Destroy(gameObject, projectileLifetime);
    }

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
    }
        
    public override void FixedUpdateNetwork()
    {
        transform.position += transform.right * travelSpeed * Time.fixedDeltaTime;
    }

    // 트리거 충돌 시 호출되는 이벤트 함수
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 태그가 "Player"인지 확인
        if (collision.CompareTag("Player"))
        {
            // 2. 데미지 인터페이스 실행
            collision.GetComponent<IEntity>()?.TakeDamage(projectileDamage, transform.right);

            // 3. 충돌 후 발사체 제거
            Destroy(gameObject);
        }
    }
}
