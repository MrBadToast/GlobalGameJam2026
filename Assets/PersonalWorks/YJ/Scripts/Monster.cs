using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 몬스터 클래스
/// </summary>
public class Monster : EntityBase
{
    [Header("Monster Only")]
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private int goldAmount = 10;

    protected override void OnDeath()
    {
        // 골드 프리팹 스폰
        if (goldPrefab != null)
        {
            var gold = Instantiate(goldPrefab, transform.position, Quaternion.identity);
            gold.GetComponent<GoldPickup>()?.SetAmount(goldAmount);
        }

        // 오브젝트 제거 (애니메이션 후)
        Destroy(gameObject, 1f);
    }
}
