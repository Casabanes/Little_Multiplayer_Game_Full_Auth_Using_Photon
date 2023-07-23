using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class PlayerSpawner : MonoBehaviourPun
{
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private PCharacter _characterPrefab;
    int _playersOnline=-1;


    void Start()
    {
        if (!PhotonNetwork.IsMasterClient) Destroy(gameObject);
    }

    public PCharacter CreatePlayer()
    {
        _playersOnline++;
        
        return PhotonNetwork.Instantiate(_characterPrefab.name, _spawnPoints[_playersOnline].position, Quaternion.identity)
                                .GetComponent<PCharacter>();        
    }
}
