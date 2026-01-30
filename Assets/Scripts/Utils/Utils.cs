using UnityEngine;

public static class Utils
{
    public static Vector2 GetRandomSpawnPoint()
    {
        return new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
    }
}
