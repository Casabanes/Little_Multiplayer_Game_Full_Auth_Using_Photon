using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NormalBullet : MonoBehaviourPun
{
    [SerializeField] Renderer r;
    [SerializeField] float _speed = 4;
    float _time;
    float _timeToDie=15;
    float _dmg;
    public int teamxd;
    PCharacter _owner;

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        _time += Time.deltaTime;
        if (_time >= _timeToDie)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        transform.position += transform.up * _speed * Time.fixedDeltaTime;
    }

    public NormalBullet SetOwner(PCharacter owner)
    {
        _time = 0;
        _owner = owner;
        return this;
    }

    public NormalBullet SetDmg(float dmg)
    {
        _dmg = dmg;
        return this;
    }
    public NormalBullet SetForward(Vector3 forward)
    {
        transform.up = forward;
        return this;
    }
    public NormalBullet SetColor(Color newColor, int team)
    {
        teamxd = team;
        r.material.color = newColor;
        photonView.RPC(nameof(RPC_SetColor), RpcTarget.Others, team);
        return this;
    }

    [PunRPC]
    void RPC_SetColor(int team)
    {
        teamxd = team;
        switch (team)
        {
            case 1:
                r.material.color = Color.red;
                break;
            case 2:
                r.material.color = Color.blue;
                break;
            case 3:
                r.material.color = Color.yellow;
                break;
            case 4:
                r.material.color = Color.green;
                break;
            case 5:
                r.material.color = Color.cyan;
                break;
            case 6:
                r.material.color = Color.white;
                break;
            case 7:
                r.material.color = Color.grey;
                break;
            case 8:
                r.material.color = Color.black;
                break;
            default:
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        var character = other.GetComponent<PCharacter>();

        if (character && character != _owner)
        {
            character.TakeDmg(_dmg);

            PhotonNetwork.Destroy(gameObject);
        }
    }
}
