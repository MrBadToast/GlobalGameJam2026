using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;

public class NetworkPlayer : NetworkBehaviour/*, IPlayerLeft*/
{
   /* public TextMeshProUGUI playerNickName;
    public static NetworkPlayer Local {  get; private set; }
    public Transform playerModel;

    [Networked]
    [OnChangedRender(nameof(OnNickNameChanged))]
    public NetworkString<_16> nickName { get; set; }

    public override void Spawned()
    {
        if(Object.HasInputAuthority)
        {
            Local = this;

            //Camera.main.gameObject.SetActive(false);

            RPC_SetNickName(PlayerPrefs.GetString("PlayerNickName"));

            Debug.Log("Spawned local player!!");
        }
        else
        {
            *//*Camera localCamera = GetComponentInChildren<Camera>();
            localCamera.enabled = false;*/

            /*AudioListener audioListener = GetComponentInChildren<AudioListener>();
            audioListener.enabled = false;*//*

            Debug.Log("Spawned remote player!!!!");
        }

        transform.name = $"P_{Object.Id}";
    }

    public void PlayerLeft(PlayerRef player)
    {
        throw new System.NotImplementedException();
    }

    private void OnNickNameChanged()
    {
        Debug.Log($"Nick anme changed for player to {nickName} for player {gameObject.name}");

        playerNickName.text = nickName.ToString();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string nickName, RpcInfo info = default)
    {
        Debug.Log($"[RPC] SetNickName {nickName}");
        this.nickName = nickName;
    }*/
}
