using UnityEngine;

/// <summary>
/// 플레이어 스폰 위치 관리
/// </summary>
public class FloorSpawnPoints : MonoBehaviour
{
    public static FloorSpawnPoints Instance { get; private set; }

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 랜덤 스폰 위치 반환
    /// </summary>
    public Vector3 GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[FloorSpawnPoints] 스폰 포인트가 없습니다!");
            return Vector3.zero;
        }

        int randomIndex = Random.Range(0, spawnPoints.Length);
        return spawnPoints[randomIndex].position;
    }

    /// <summary>
    /// 스폰 포인트 개수
    /// </summary>
    public int SpawnPointCount => spawnPoints != null ? spawnPoints.Length : 0;

    private void OnDrawGizmos()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.green;
        foreach (var point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.5f);
            }
        }
    }
}
