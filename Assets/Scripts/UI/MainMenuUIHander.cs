using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIHander : MonoBehaviour
{
    public TMP_InputField inputField;

    public GameObject sessionUI;

    void Start()
    {
        if(PlayerPrefs.HasKey("PlayerNickName"))
        {
            inputField.text = PlayerPrefs.GetString("PlayerNickName");
        }
    }

    public void OnPopUpSessionUI()
    {
        sessionUI.gameObject.SetActive(true);
    }

    public void OnJoinGameCliked()
    {
        PlayerPrefs.SetString("PlayerNickName", inputField.text);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Platformer");
    }
}
