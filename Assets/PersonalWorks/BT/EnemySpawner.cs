using Fusion;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    // 1. 싱글톤 인스턴스 추가 (편의성을 위해)
    public static EnemySpawner Instance { get; private set; }

    [SerializeField, LabelText("활성화할 NPC")] private GameObject npcObject;
    [Networked] private NetworkBool isNpcActive { get; set; }

    // 2. 동기화를 위해 [Networked] 사용 (서버가 값을 바꾸면 클라이언트에게 자동 전달)
    public int currentWave { get; set; } = 1;

    [SerializeField] private bool spawnOnStart = true;
    [SerializeReference] public SpawnSegment[] spawnSegments;

    public override void Spawned()
    {
        Instance = this;
        // 호스트만 스폰 루틴을 돌림
        if (Object.HasStateAuthority && spawnOnStart)
        {
            StartCoroutine(Cor_SpawnProcess());
        }
    }

    public override void Render()
    {
        if (npcObject != null)
        {
            npcObject.SetActive(isNpcActive);
        }
    }

    public void SetNpcActive(bool active)
    {
        if (Object.HasStateAuthority)
        {
            isNpcActive = active;
        }
    }

    IEnumerator Cor_SpawnProcess()
    {
        foreach(var segment in spawnSegments)
        {
            yield return StartCoroutine(segment.Cor_Segment(Runner));
        }
    }

    [System.Serializable]
    public class SpawnSegment
    {
        public virtual IEnumerator Cor_Segment(NetworkRunner runner) => null;
    }

    [System.Serializable]
    public class SpawnEnemy : SpawnSegment
    {
        [SerializeField,LabelText("스폰할 적 프리팹")] private GameObject enemyPrefab;
        [SerializeField,LabelText("스폰할 위치")] private Transform spawnPoint;
        [SerializeField,LabelText("스폰위치 오차")] private float spawnRange = 1.0f;
        [SerializeField,LabelText("스폰할 수")] private int spawnQuantity = 1;

        public override IEnumerator Cor_Segment(NetworkRunner runner)
        {
            for (int i = 0; i < spawnQuantity; i++)
            {
                Vector3 pos = spawnPoint.position + Random.insideUnitSphere * spawnRange;
                NetworkObject enemy = runner.Spawn(enemyPrefab, pos, Quaternion.identity);
                enemy.GetComponent<EnemyBehavior_Generic>().Setproperty(EnemySpawner.Instance.currentWave);
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

        public override IEnumerator Cor_Segment(NetworkRunner runner)
        {
            for(int i = 0; i < repeatCount; i++)
            {
                NetworkObject enemy = runner.Spawn(enemyPrefab, spawnPoint.position + Random.insideUnitSphere * spawnRange, Quaternion.identity);
                if(enemy)
                    enemy.GetComponent<EnemyBehavior_Generic>().Setproperty(EnemySpawner.Instance.currentWave);
                
                yield return new WaitForSeconds(interaval);
            }
        }
    }

    [System.Serializable]
    public class Wait : SpawnSegment
    {
        [SerializeField, LabelText("대기 시간(초)")] private float waitTime = 1f;
        [SerializeField, LabelText("웨이브 증가 여부")] private bool incrementWave = true;

        [SerializeField, LabelText("NPC 활성화 여부")] private bool activateNpc = true;
        [SerializeField, LabelText("대기 후 NPC 비활성화?")] private bool deactivateAfterWait = true;
        public override IEnumerator Cor_Segment(NetworkRunner runner)
        {
            if (activateNpc)
            {
                EnemySpawner.Instance.SetNpcActive(true);
            }

            yield return new WaitForSeconds(waitTime);

            if (incrementWave)
            {
                EnemySpawner.Instance.currentWave++;
            }

            // 대기 종료 후 NPC 비활성화 처리
            if (deactivateAfterWait)
            {
                EnemySpawner.Instance.SetNpcActive(false);
            }
        }
    }

}