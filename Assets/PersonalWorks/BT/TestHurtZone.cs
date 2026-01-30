using System.Linq;
using UnityEngine;

public class TestHurtZone : MonoBehaviour
{
    public LayerMask hitLayer;
    public BoxCollider2D col;
    public float damageForce = 10.0f;
    public float damageAmount = 5.0f;


    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
    }

    float t = 0.0f;
    private void Update()
    {
        t += Time.deltaTime;
        
        if(t > 1.0f)
        {
            t = 0.0f;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(col.bounds.center, col.bounds.size, 0.0f, Vector2.zero, 0.0f, hitLayer);

            hits.ToList().ForEach(hit =>
            {
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                hit.collider.GetComponent<IEntity>()?.TakeDamage(damageAmount, dir * damageForce);
            }
            );
        }
    }
}
