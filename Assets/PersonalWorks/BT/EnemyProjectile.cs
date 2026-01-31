using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float travelSpeed = 10f;
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private float projectileLifetime = 5f;
    [SerializeField] private LayerMask hurtLayer;

    private CircleCollider2D circleCollider;
    private void Start()
    {
        Destroy(gameObject, projectileLifetime);
    }

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
    }

    void FixedUpdate()
    {
        transform.position += transform.right * travelSpeed * Time.fixedDeltaTime;

        RaycastHit2D rHit = Physics2D.CircleCast(transform.position, circleCollider.radius, transform.right,0f, hurtLayer);
        
        if(rHit)
        {
            rHit.collider.GetComponent<IEntity>()?.TakeDamage(projectileDamage, transform.right);
            Destroy(gameObject);
        }
    }
}
