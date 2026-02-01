using Fusion;
using TMPro;
using UnityEngine;

/// <summary>
/// 서버 동기화 게임 타이머
/// 씬 로드 시 시작, 모든 플레이어 사망 시 중단
/// </summary>
public class GameTimer : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    // 서버에서 계산한 경과 시간 (동기화됨)
    [Networked] private float SyncedElapsedTime { get; set; }
    // 일시정지 여부
    [Networked] private NetworkBool IsPaused { get; set; }

    public static GameTimer Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            SyncedElapsedTime = 0f;
            IsPaused = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        bool allDead = CheckAllPlayersDead();

        // 모두 죽으면 타이머 정지
        if (allDead && !IsPaused)
        {
            IsPaused = true;
        }
        // 누군가 살아나면 타이머 재개
        else if (!allDead && IsPaused)
        {
            IsPaused = false;
        }

        // 일시정지가 아니면 시간 증가
        if (!IsPaused)
        {
            SyncedElapsedTime += Runner.DeltaTime;
        }
    }

    private void Update()
    {
        UpdateUI();
    }

    public float GetElapsedTime()
    {
        return SyncedElapsedTime;
    }

    private void UpdateUI()
    {
        if (timerText == null) return;

        float elapsed = SyncedElapsedTime;
        int minutes = (int)(elapsed / 60f);
        int seconds = (int)(elapsed % 60f);

        timerText.text = $"{minutes:00} : {seconds:00}";
    }

    private bool CheckAllPlayersDead()
    {
        var players = FindObjectsByType<Player_Topdown>(FindObjectsSortMode.None);
        if (players.Length == 0) return false;

        foreach (var player in players)
        {
            if (!player.IsDead) return false;
        }
        return true;
    }

    public void ResetTimer()
    {
        if (!Object.HasStateAuthority) return;
        SyncedElapsedTime = 0f;
        IsPaused = false;
    }

    public bool IsTimerPaused => IsPaused;
}
