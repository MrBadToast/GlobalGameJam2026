using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyBehavior_Base : MonoBehaviour
{
    [SerializeField,LabelText("최대 체력")] private float fullHealth = 100f;

    private float currentHealth;

    protected virtual void Start()
    {
        currentHealth = fullHealth;
    }

    public virtual void OnHurt(Vector2 force, float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public virtual void Die()
    {
        currentHealth = 0;
    }
}
