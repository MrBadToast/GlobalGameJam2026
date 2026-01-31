using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 에디터 개발용 부트스트래퍼
/// Title 씬을 거치지 않고 바로 게임 씬에서 시작할 수 있도록 함
/// </summary>
public class DevBootstrap : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Settings")]
    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private GameMode gameMode = GameMode.Single;
    [SerializeField] private bool autoStartInEditor = true;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    private NetworkRunner runner;

    private async void Start()
    {
#if UNITY_EDITOR
        // 에디터에서만 동작
        if (!autoStartInEditor) return;

        // 이미 NetworkRunner가 있으면 (Title에서 온 경우) 무시
        var existingRunner = FindFirstObjectByType<NetworkRunner>();
        if (existingRunner != null && existingRunner.IsRunning)
        {
            Debug.Log("[DevBootstrap] NetworkRunner already exists, skipping auto-start.");
            Destroy(gameObject);
            return;
        }

        Debug.Log("[DevBootstrap] Starting network in development mode...");
        await StartNetwork();
#else
        // 빌드에서는 이 컴포넌트 비활성화
        Destroy(gameObject);
#endif
    }

    private async Task StartNetwork()
    {
        // Runner 생성
        if (runnerPrefab != null)
        {
            runner = Instantiate(runnerPrefab);
        }
        else
        {
            var runnerObj = new GameObject("NetworkRunner");
            runner = runnerObj.AddComponent<NetworkRunner>();
        }

        runner.name = "NetworkRunner (Dev)";
        runner.ProvideInput = true;

        // Scene Manager 추가
        var sceneManager = runner.GetComponent<INetworkSceneManager>();
        if (sceneManager == null)
        {
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        // 콜백 등록
        runner.AddCallbacks(this);

        // 네트워크 시작
        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            SessionName = "DevSession",
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = sceneManager
        });

        if (result.Ok)
        {
            Debug.Log($"[DevBootstrap] Network started successfully in {gameMode} mode.");
        }
        else
        {
            Debug.LogError($"[DevBootstrap] Failed to start network: {result.ShutdownReason}");
        }
    }

    // ========== INetworkRunnerCallbacks ==========

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[DevBootstrap] Player joined: {player}");

        // 로컬 플레이어만 스폰
        if (runner.LocalPlayer == player)
        {
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;

            if (playerPrefab.IsValid)
            {
                var playerObj = runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);
                Debug.Log($"[DevBootstrap] Spawned player: {playerObj.name}");
            }
            else
            {
                Debug.LogWarning("[DevBootstrap] PlayerPrefab not set!");
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // Player_Topdown의 GetNetworkInput 호출
        if (Player_Topdown.Local != null)
        {
            input.Set(Player_Topdown.Local.GetNetworkInput());
        }
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"[DevBootstrap] Network shutdown: {shutdownReason}");
    }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
