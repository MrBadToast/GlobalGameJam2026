using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    static private SceneLoader instance;
    static public SceneLoader Instance { get { return instance; } }

    [SerializeField] private GameObject visualGroup;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private DOTweenAnimation fadeAnimation;

    private bool sceneLoading = false;

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void LoadNewScene(string sceneID)
    {
        if (instance == null) { Debug.LogError("�� �ε��� �ʿ��� SceneLoader�� �����ϴ�. �ش� ������Ʈ�� �ִ��� Ȯ�����ּ���."); return; }

        if (sceneLoading) return;
        StopAllCoroutines();
        StartCoroutine(Cor_LoadNewScene(sceneID));
    }

    private IEnumerator Cor_LoadNewScene(string sceneID)
    {
        if(Time.timeScale != 1f)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        visualGroup.SetActive(true);
        sceneLoading = true;
        fadeAnimation.DORestartById("FADE_IN");
        Tween tw = fadeAnimation.GetTweens()[0];
        yield return tw.WaitForCompletion();

        var async = SceneManager.LoadSceneAsync(sceneID);
        async.allowSceneActivation = true;

        while(!async.isDone)
        {
            progressSlider.value = async.progress / 0.9f;
            yield return null;
        }

        yield return new WaitForEndOfFrame();

        yield return new WaitForSeconds(0.5f);

        fadeAnimation.DORestartById("FADE_OUT");
        tw = fadeAnimation.GetTweens()[1];
        yield return tw.WaitForCompletion();

        sceneLoading = false;
        visualGroup.SetActive(false);
    }
}
