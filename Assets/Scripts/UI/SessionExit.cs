using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionExit : MonoBehaviour
{
    public static SessionExit Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        gameObject.SetActive(false);
    }

    public void LeaveSession()
    {
        // 1. 현재 씬에 있는 NetworkRunner를 찾습니다.
        NetworkRunner runner = Object.FindFirstObjectByType<NetworkRunner>();

        if (runner != null)
        {
            // 클라이언트가 나가도 세션(방)은 터지지 않습니다.
            // ShutdownReason.Ok는 정상적인 종료를 의미합니다.
            runner.Shutdown(true, ShutdownReason.Ok);

            // 3. (선택 사항) 로비나 메인 메뉴 씬으로 이동합니다.
            // 보통 Shutdown 이후에 호출하거나, OnShutdown 콜백에서 처리합니다.
            SceneManager.LoadScene("Title");
        }
    }
}
