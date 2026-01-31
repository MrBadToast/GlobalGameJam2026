using UnityEngine;

public class TitleFirstjoinLinker : MonoBehaviour
{
    NetworkRunnerHandler networkRunnerHandler;

    private void OnEnable()
    {
        networkRunnerHandler = FindFirstObjectByType<NetworkRunnerHandler>();
        if (networkRunnerHandler == null)
            Debug.Log("NetworkRunnerHandler Was not found");
    }

    public void FirstJoin()
    {
        if (networkRunnerHandler != null)
            networkRunnerHandler.FirstJoin();
    }
}
