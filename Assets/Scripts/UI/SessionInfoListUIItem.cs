using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;

public class SessionInfoListUIItem : MonoBehaviour
{
    public TextMeshProUGUI sessionNameText;
    public TextMeshProUGUI playerCountText;
    public Button JoinButton;

    SessionInfo sessionInfo;
    public event Action<SessionInfo> OnJoinSession;


    public void SetInfomation(SessionInfo sessionInfo)
    {
        this.sessionInfo = sessionInfo;

        sessionNameText.text = sessionInfo.Name;
        playerCountText.text = $"{sessionInfo.PlayerCount.ToString()} / {sessionInfo.MaxPlayers.ToString()}";

        bool isJoinButtonActive = true;

        if(sessionInfo.PlayerCount >= sessionInfo.MaxPlayers)
        {
            isJoinButtonActive = false;
        }

        JoinButton.gameObject.SetActive(isJoinButtonActive);
    }

    public void OnClick()
    {
        OnJoinSession?.Invoke(sessionInfo);
    }
}
