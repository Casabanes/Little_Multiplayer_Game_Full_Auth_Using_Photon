using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
public class BackToMenu : MonoBehaviourPunCallbacks
{
    Player _clientOwner;
    public void BTN_BackToMenu()
    {
        _clientOwner = GetComponentInParent<PCharacter>()._clientOwner;
        WatakshiNoServer.Instance.Disconect(_clientOwner);
    }
}
