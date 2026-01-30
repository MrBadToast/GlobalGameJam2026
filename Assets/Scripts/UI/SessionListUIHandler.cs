using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Fusion;
using TMPro;

public class SessionListUIHandler : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    public GameObject sessionItemListPrefab;
    public VerticalLayoutGroup verticalLayoutGroup;

    public void ClearList()
    {
        foreach (Transform child in verticalLayoutGroup.transform)
        {
            Destroy(child.gameObject);
        }

        statusText.gameObject.SetActive(false);
    }

    public void AddToList(SessionInfo sessionInfo)
    {
        SessionInfoListUIItem addedSessionInfoListUIItem = Instantiate(sessionItemListPrefab, verticalLayoutGroup.transform).GetComponent<SessionInfoListUIItem>();

        addedSessionInfoListUIItem.SetInfomation(sessionInfo);

        addedSessionInfoListUIItem.OnJoinSession += AddedSessionListUIItem_OnJoinSession;
    }

    private void AddedSessionListUIItem_OnJoinSession(SessionInfo obj)
    {

    }

    public void OnNoSessionFound()
    {
        statusText.text = "No Game Session Found!";
        statusText.gameObject.SetActive(true);
    }

    public void OnLookingForGameSession()
    {
        statusText.text = "Looking for game session...";
        statusText.gameObject.SetActive(true);
    }
}
