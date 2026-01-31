using UnityEngine;

public static class Utils
{
    public static Vector2 GetRandomSpawnPoint()
    {
        // FloorSpawnPoints가 있으면 타일맵 기반 스폰
        if (FloorSpawnPoints.Instance != null && FloorSpawnPoints.Instance.SpawnPointCount > 0)
        {
            return FloorSpawnPoints.Instance.GetRandomSpawnPoint();
        }

        // 폴백: 기본 랜덤 위치
        Debug.LogWarning("[Utils] FloorSpawnPoints가 없습니다. 기본 랜덤 위치 사용");
        return new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
    }
}
