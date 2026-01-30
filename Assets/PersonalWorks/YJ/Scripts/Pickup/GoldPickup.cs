using UnityEngine;

/// <summary>
/// 골드 픽업 아이템 - 플레이어가 접근하면 자동 획득
/// </summary>
public class GoldPickup : MonoBehaviour
{
    [SerializeField] private int amount = 10;

    /// <summary>
    /// 몬스터에서 골드량 설정
    /// </summary>
    public void SetAmount(int value) => amount = value;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Player_Topdown>(out var player))
        {
            player.AddMoney(amount);
            Destroy(gameObject);
        }
    }
}
