using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private bool spawnOnStart = true;
    [SerializeReference] public SpawnSegment[] spawnSegments;

    public void Start()
    {
        if (spawnOnStart)
            StartCoroutine(Cor_SpawnProcess());
    }

    IEnumerator Cor_SpawnProcess()
    {
        foreach(var segment in spawnSegments)
        {
            yield return StartCoroutine(segment.Cor_Segment());
        }
    }

    [System.Serializable]
    public class SpawnSegment
    {
        public virtual IEnumerator Cor_Segment()
        {
            yield return null;
        }
    }

    [System.Serializable]
    public class SpawnEnemy : SpawnSegment
    {
        [SerializeField,LabelText("스폰할 적 프리팹")] private GameObject enemyPrefab;
        [SerializeField,LabelText("스폰할 위치")] private Transform spawnPoint;
        [SerializeField,LabelText("스폰위치 오차")] private float spawnRange = 1.0f;
        [SerializeField,LabelText("스폰할 수")] private int spawnQuantity = 1;

        public override IEnumerator Cor_Segment()
        {
            for (int i = 0; i < spawnQuantity; i++)
            {
                Instantiate(enemyPrefab, spawnPoint.position + Random.insideUnitSphere * spawnRange, Quaternion.identity);
            }
            yield return null;
        }

    }

    [System.Serializable]
    public class SpawnRepeating : SpawnSegment
    {
        [SerializeField, LabelText("스폰할 적 프리팹")] private GameObject enemyPrefab;
        [SerializeField, LabelText("스폰할 위치")] private Transform spawnPoint;
        [SerializeField, LabelText("스폰위치 오차")] private float spawnRange = 1.0f;

        [SerializeField, LabelText("스폰 간격(초)")] private float interaval = 1f;
        [SerializeField, LabelText("반복 횟수")] private int repeatCount = 5;

        public override IEnumerator Cor_Segment()
        {
            for(int i = 0; i < repeatCount; i++)
            {
                Instantiate(enemyPrefab, spawnPoint.position + Random.insideUnitSphere * spawnRange, Quaternion.identity);
                yield return new WaitForSeconds(interaval);
            }
        }
    }

    [System.Serializable]
    public class Wait : SpawnSegment
    {
        [SerializeField, LabelText("대기 시간(초)")] private float waitTime = 1f;
        public override IEnumerator Cor_Segment()
        {
            yield return new WaitForSeconds(waitTime);
        }
    }

}
