using UnityEngine;

/// <summary>
/// 로컬 플레이어를 따라가는 카메라
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Bounds (Optional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    private Transform target;

    private void LateUpdate()
    {
        // 타겟이 없으면 로컬 플레이어 찾기
        if (target == null)
        {
            if (Player_Topdown.Local != null)
            {
                target = Player_Topdown.Local.transform;
            }
            return;
        }

        // 목표 위치 계산
        Vector3 desiredPosition = target.position + offset;

        // 경계 제한 (옵션)
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }

        // 부드러운 이동
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }

    /// <summary>
    /// 타겟 수동 설정
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// 즉시 타겟 위치로 이동
    /// </summary>
    public void SnapToTarget()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
