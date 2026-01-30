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
    public NetworkRunner networkRunnerPrefab;

    NetworkRunner networkRunner;

    // player spawn secene
    int targetSceneIndex = 1;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void FirstJoin()
    {
        networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.name = "NetworkRunner";
        var clientTask = InitializeNetworkRunner(
            networkRunner,
            GameMode.AutoHostOrClient,
            NetAddress.Any(),
            SceneRef.FromIndex(targetSceneIndex),
            null
        );

        Debug.Log($"Server NetworkRunner Started!!!");
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
