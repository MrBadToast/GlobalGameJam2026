using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

public class NetworkRunnerHandler : MonoBehaviour
{
    public static NetworkRunnerHandler _instance;
    public NetworkRunner networkRunnerPrefab;

    NetworkRunner networkRunner;

    // player spawn secene
    int targetSceneIndex = 1;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 이미 존재한다면 새로 만들어진 중복 객체를 파괴
            Destroy(gameObject);
        }
    }

    public void FirstJoin()
    {
        if (networkRunner != null)
        {
            // 아직 작동 중이라면 셧다운 시도
            if (networkRunner.IsRunning)
            {
                networkRunner.Shutdown();
            }

            // 하이어라키에서 제거
            Destroy(networkRunner.gameObject);
            networkRunner = null;
        }

        // 완전히 깨끗한 상태에서 다시 생성
        networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.name = "NetworkRunner_" + Time.frameCount; // 이름 중복 방지

        InitializeNetworkRunner(
            networkRunner,
            GameMode.AutoHostOrClient,
            NetAddress.Any(),
            SceneRef.FromIndex(targetSceneIndex),
            null
        );

        Debug.Log($"NetworkRunner Re-Started!!!");
    }

    private void Start()
    {
        /*networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.name = "NetworkRunner";

        var clientTask = InitializeNetworkRunner(
            networkRunner,
            GameMode.AutoHostOrClient,
            NetAddress.Any(),
            SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            null
        );

        Debug.Log($"Server NetworkRunner Started!!!");*/
    }

    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, NetAddress address, SceneRef scene, Action<NetworkRunner> initialized)
    {
        var sceneObjectPrevider = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();

        if (sceneObjectPrevider == null)
        {
            sceneObjectPrevider = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        runner.ProvideInput = true;

        return runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene = scene,
            SessionName = "TestRoom",
            OnGameStarted = initialized,
            SceneManager = sceneObjectPrevider
        });
    }
}
