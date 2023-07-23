using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using Photon.Realtime;
public class WatakshiNoServer : MonoBehaviourPun
{
    public static WatakshiNoServer Instance;
    [SerializeField] PCharacter _characterPrefab;
    public float packagesPerSecond = 60;
    Dictionary<Player, PCharacter> _dictModels = new Dictionary<Player, PCharacter>();
    Dictionary<Player, bool> _winLose= new Dictionary<Player, bool>();

    private bool _startWhen2ClientsOrMoreAreConected;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (photonView.IsMine)
        {
            if (Instance == null)
            {
                photonView.RPC(nameof(SetServer), RpcTarget.AllBuffered);
            }
        }
    }
    [PunRPC]
    void SetServer()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (PhotonNetwork.PlayerList.Length > 2)
        {
            _startWhen2ClientsOrMoreAreConected = true;
        }
    }
    private void Update()
    {
        if (_startWhen2ClientsOrMoreAreConected)
        {
            _startWhen2ClientsOrMoreAreConected = false;
            photonView.RPC(nameof(StartGame), RpcTarget.AllBuffered);
        }
    }
    [PunRPC] void StartGame()
    {
        if (PhotonNetwork.PlayerList.Length < 10)
        {
            PhotonNetwork.LoadLevel(1);

            if (PhotonNetwork.LocalPlayer != PhotonNetwork.MasterClient)
            {
                photonView.RPC(nameof(AddPlayer), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
            }
        }
    }
    [PunRPC]
    void AddPlayer(Player newClient)
    {
        StartCoroutine(WaitForLevel(newClient));
    }
    IEnumerator WaitForLevel(Player newClient)
    {
        while (PhotonNetwork.LevelLoadingProgress > 0.9f)
        {
            yield return new WaitForEndOfFrame();
        }
            PlayerSpawner spawner = FindObjectOfType<PlayerSpawner>();
            PCharacter c = spawner.CreatePlayer();
            c.SetInitialParameters(newClient, _dictModels.Count+1);
            _dictModels.Add(newClient, c);
            _winLose.Add(newClient, true);
    }
    #region Requests
    public void RequestMove(Player clientRequest, Vector3 dir)
    {
        photonView.RPC(nameof(Move), RpcTarget.MasterClient, clientRequest, dir);
    }
    public void RequestShoot(Player clientRequest)
    {
        photonView.RPC(nameof(Shoot), RpcTarget.MasterClient, clientRequest);
    }
    public void RequestShielding(Player clientRequest)
    {
        photonView.RPC(nameof(Shielding), RpcTarget.MasterClient, clientRequest);
    }
    public void RequestRageQuit(Player clientOwner)
    {
        photonView.RPC(nameof(RPC_PlayerDisconnect), RpcTarget.MasterClient, clientOwner);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    public void RequestDie(Player clientOwner)
    {
        if (!_winLose.ContainsKey(clientOwner))
        {
            return;
        }
        _winLose[clientOwner] = false;
        int num = 2; 
        foreach (var item in _winLose)
        {
            if (item.Value)
            {
                num--;
            }
        }
        if (num == 1)
        {
            foreach (var item in _winLose)
            {
                _dictModels[item.Key].LaunchUI(item.Value);
            }
        }
    }
    public void Disconect(Player playerRequest)
    {
        photonView.RPC(nameof(RPC_DisconnectMainMenu), RpcTarget.MasterClient, playerRequest);
    }
    #endregion
    #region punes
    [PunRPC]
    void Move(Player clientRequest, Vector3 dir)
    {
        if (_dictModels.ContainsKey(clientRequest))
        {
            _dictModels[clientRequest].Move(dir);
        }
    }
    [PunRPC]
    void Shoot(Player playerRequest)
    {
        if (_dictModels.ContainsKey(playerRequest))
        {
            Vector3 pos = _dictModels[playerRequest].bulletSpawner.position;
            Quaternion rot = _dictModels[playerRequest].bulletSpawner.transform.rotation;
            _dictModels[playerRequest].Shoot(pos, rot);
        }
    }
    [PunRPC]
    void Shielding(Player playerRequest)
    {
        if (_dictModels.ContainsKey(playerRequest))
        {
            _dictModels[playerRequest].Shielding();
        }
    }
    [PunRPC]
    public void RPC_PlayerDisconnect(Player player)
    {
        PhotonNetwork.Destroy(_dictModels[player].gameObject);
        _dictModels.Remove(player);
    }
    [PunRPC]
    public void RPC_DisconnectMainMenu(Player playerRequest)
    {
        if (_dictModels.ContainsKey(playerRequest))
        {
            _dictModels[playerRequest].DisconnectToMainMenu();
            _dictModels.Remove(playerRequest);
        }
        if (_winLose.ContainsKey(playerRequest))
        {
            _winLose.Remove(playerRequest);
        }
        if (_dictModels.Count == 0 && _winLose.Count == 0)
        {
            PhotonNetwork.LoadLevel(0);
        }
    }
    #endregion
}
