using Fusion;
using UnityEngine;

/// <summary>
/// 돈 픽업 오브젝트 - 플레이어가 접촉 시 Money 획득
/// 서버에서 먼저 접촉한 플레이어에게만 지급
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MoneyPickup : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private int moneyAmount = 10;
    [SerializeField] private float lifetime = 30f;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;

    [Networked] private NetworkBool IsPickedUp { get; set; }

    private float spawnTime;

    public override void Spawned()
    {
        spawnTime = Time.time;
        IsPickedUp = false;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        // 수명 초과 시 제거
        if (Time.time - spawnTime >= lifetime)
        {
            Runner.Despawn(Object);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 서버에서만 처리
        if (!Object.HasStateAuthority) return;
        if (IsPickedUp) return;

        // 플레이어인지 확인
        var player = other.GetComponent<Player_Topdown>();
        if (player == null) return;

        // 픽업 처리
        IsPickedUp = true;

        // 플레이어에게 돈 지급
        player.Money += moneyAmount;

        // 사운드 재생 RPC
        RPC_PlayPickupEffect(transform.position);

        // 오브젝트 제거
        Runner.Despawn(Object);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayPickupEffect(Vector3 position)
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, position, soundVolume);
        }
    }

    /// <summary>
    /// 돈 양 설정 (스폰 시 호출)
    /// </summary>
    public void SetAmount(int amount)
    {
        moneyAmount = amount;
    }
}
