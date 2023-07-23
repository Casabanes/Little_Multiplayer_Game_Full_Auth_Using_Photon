using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class Launcher : MonoBehaviourPunCallbacks
{
    [SerializeField] private WatakshiNoServer _serverPrefab;
    [SerializeField] private PController _controllerPrefab;
    public void BTN_Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public void BTN_Exit()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (WatakshiNoServer.Instance)
            {
                WatakshiNoServer.Instance.RequestRageQuit(PhotonNetwork.LocalPlayer);
                Application.Quit();
            }
            else
            {
                Application.Quit();
            }
        }
        else
        {
            Application.Quit();
        }
    }

    public override void OnConnectedToMaster()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 9;

        PhotonNetwork.JoinOrCreateRoom("ServerFullAuth", options, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        PhotonNetwork.Instantiate(_serverPrefab.name, Vector3.zero, Quaternion.identity);
    }

    public override void OnJoinedRoom()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Instantiate(_controllerPrefab);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Connection Failed: " + cause.ToString());
    }
}
