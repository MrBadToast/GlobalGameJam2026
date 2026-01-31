using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spawner : MonoBehaviour, INetworkRunnerCallbacks
{
    public Player_Topdown playerPrefab;
    public Player_Topdown characterInputHandler;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {

    }

    // 1. 플레이어가 접속했을 때 (가장 중요)
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Fusion] 플레이어 접속 완료: {player}. 현재 총 인원: {runner.ActivePlayers.Count()}");

        if (runner.IsServer)
        {
            var spawnedObj = runner.Spawn(playerPrefab, Utils.GetRandomSpawnPoint(), Quaternion.identity, player);

            if (spawnedObj != null)
                Debug.Log($"스폰 완료! 오브젝트 이름: {spawnedObj.name}, 위치: {spawnedObj.transform.position}");
        }
        else Debug.Log("OnPlayerJoined");
    }

    // 2. 플레이어가 나갔을 때
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Fusion] 플레이어 퇴장: {player}");
    }

    // 3. 입력값을 수집할 때 (매 프레임 발생하므로 확인 후 주석 처리 권장)
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if(characterInputHandler == null && Player_Topdown.Local != null)
            characterInputHandler = Player_Topdown.Local.GetComponent<Player_Topdown>();

        if (characterInputHandler != null)
            input.Set(characterInputHandler.GetNetworkInput());
    }

    // 4. 서버와 연결되었을 때
    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("[Fusion] 서버 연결 성공!");
    }

    // 5. 연결에 실패했을 때
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogWarning($"[Fusion] 연결 실패! 사유: {reason}");
    }

    // 6. 셧다운(종료) 되었을 때
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"[Fusion] 네트워크 종료. 사유: {shutdownReason}");
    }

    // 7. 씬 로드가 끝났을 때
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("[Fusion] 씬 로딩 완료");
    }

    // --- 아래는 자주 쓰이지 않지만 인터페이스 유지를 위해 남겨두는 함수들 (로그 생략 가능) ---

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { Debug.Log($"[Fusion] 서버와 끊어짐: {reason}"); }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { Debug.Log("[Fusion] 세션 목록 업데이트됨"); }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
