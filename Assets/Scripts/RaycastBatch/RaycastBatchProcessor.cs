using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class RaycastBatchProcessor
{
    [SerializeField] int maxRaycastsPerJob = 10000;

    NativeArray<RaycastCommand> rayCommands;
    NativeArray<RaycastHit> hitResults;

    public void PerformRaycasts(
        Vector3[] origins,
        Vector3[] directions,
        int layerMask,
        bool hitBackfaces,
        bool hitTriggers,
        bool hitMultiFace,
        Action<RaycastHit[]> callback)
    {
        // [DEBUG] 시작 로그
        Debug.Log($"<color=cyan>[Batch] Raycast 시도 시작 - 개수: {origins.Length}</color>");

        const float maxDistance = 15f; // 거리가 1f면 너무 짧을 수 있어 확장 제안
        int rayCount = Mathf.Min(origins.Length, maxRaycastsPerJob);

        if (rayCount == 0)
        {
            Debug.LogWarning("[Batch] 입력된 Origin 데이터가 없어 작업을 중단합니다.");
            return;
        }

        QueryTriggerInteraction queryTriggerInteraction = hitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

        using (rayCommands = new NativeArray<RaycastCommand>(rayCount, Allocator.TempJob))
        {
            QueryParameters parameters = new QueryParameters
            {
                layerMask = layerMask,
                hitBackfaces = hitBackfaces,
                hitTriggers = queryTriggerInteraction,
                hitMultipleFaces = hitMultiFace
            };

            for (int i = 0; i < rayCount; i++)
            {
                rayCommands[i] = new RaycastCommand(origins[i], directions[i], parameters, maxDistance);
                // [DEBUG] 각 레이의 시작 지점과 방향 시각화 (빨간색)
                Debug.DrawRay(origins[i], directions[i] * maxDistance, Color.red, 0.5f);
            }

            ExecuteRaycasts(rayCommands, callback);
        }
    }

    void ExecuteRaycasts(NativeArray<RaycastCommand> raycastCommands, Action<RaycastHit[]> callback)
    {
        int maxHitsPerRaycast = 1;
        int totalHitsNeeded = raycastCommands.Length * maxHitsPerRaycast;

        using (hitResults = new NativeArray<RaycastHit>(totalHitsNeeded, Allocator.TempJob))
        {
            // [DEBUG] 잡 스케줄링 전
            Debug.Log($"[Batch] Job 스케줄링 시작 (Target: {totalHitsNeeded} hits)");

            JobHandle raycastJobHandle = RaycastCommand.ScheduleBatch(raycastCommands, hitResults, maxHitsPerRaycast);

            // Job이 끝날 때까지 대기
            raycastJobHandle.Complete();

            // [DEBUG] 잡 완료 후 데이터 분석
            int hitCount = 0;
            RaycastHit[] results = hitResults.ToArray();

            for (int i = 0; i < results.Length; i++)
            {
                if (results[i].collider != null)
                {
                    hitCount++;
                    // [DEBUG] 충돌 성공 시 - 이름, 위치, 태그 등 상세 정보 출력
                    Debug.Log($"<color=green>[Hit Success]</color> 대상: {results[i].collider.name} | 좌표: {results[i].point} | 거리: {results[i].distance}");

                    // [DEBUG] 충돌 지점 시각화 (녹색 선)
                    Debug.DrawLine(raycastCommands[i].from, results[i].point, Color.green, 1.0f);
                }
                else
                {
                    // [DEBUG] 충돌 실패 시 (해당 인덱스의 레이가 아무것도 맞추지 못함)
                    // Debug.Log($"[Hit Fail] Index {i}: 공중으로 날아감");
                }
            }

            Debug.Log($"<color=yellow>[Batch] 최종 결과 - 총 {raycastCommands.Length}개 중 {hitCount}개 적중</color>");

            // 결과 콜백 실행
            callback?.Invoke(results);
        }
    }
}