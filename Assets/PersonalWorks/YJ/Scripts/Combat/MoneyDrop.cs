using UnityEngine;

/// <summary>
/// 돈 드롭 아이템. 플레이어와 접촉 시 획득.
/// CircleCollider2D (IsTrigger) 필요.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class MoneyDrop : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int moneyAmount = 1;

    [Header("Sound")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (Player_Topdown.Instance != null)
        {
            Player_Topdown.Instance.AddMoney(moneyAmount);

            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
            }
        }

        Destroy(gameObject);
    }
}
