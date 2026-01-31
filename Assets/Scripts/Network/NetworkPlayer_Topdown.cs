using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class NetworkPlayer_Topdown : NetworkBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float moveSpeed = 5f;

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            // 1. 방향 계산
            Vector3 moveVector = new Vector3(data.movementInput.x, data.movementInput.y, 0);
            moveVector.Normalize();

            // 2. Transform 위치 직접 수정
            // Runner.DeltaTime을 곱해 네트워크 틱에 맞게 이동 거리를 계산합니다.
            transform.position += moveVector * moveSpeed * Runner.DeltaTime;
        }
    }
}
