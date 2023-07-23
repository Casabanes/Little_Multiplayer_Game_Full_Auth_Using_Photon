using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
public class PCharacter : MonoBehaviourPun, IPunObservable
{
    public Player _clientOwner;

    [SerializeField] float _maxLife;
    float _currentLife;
    [SerializeField] float _speed;
    [SerializeField] float _dmg;

    [SerializeField] NormalBullet _bulletPrefab;
    [SerializeField] public Transform bulletSpawner;

    public event Action<float> OnLifebarUpdate = delegate { };
    public event Action OnDeath = delegate { };
    [SerializeField] private CameraController _camera;
    [SerializeField] private Animator _animator;
    [SerializeField] private string shootAnimatorParameter="shoot";
    private Transform _targetPoint;

    [SerializeField] private MeshRenderer _shieldmeshRenderer;
    [SerializeField] private BoxCollider _shieldbc;
    [SerializeField] private Shield _shield;
    public Color myColor;
    [SerializeField] private int _team;
    private bool _canShoot=true;
    private float _attackSpeed = 0.25f;
    [SerializeField] private GameObject[] hideComponents;
    private Vector3 _direction;
    private const int _constZero = 0;
    [SerializeField] private float _timeRotating;
    [SerializeField] private float _timeToRotate;
    [SerializeField] private string moveAnimatorParameter = "moving";
    private Vector3 _startingForward;
    [SerializeField] private GameObject _victoryUI;
    [SerializeField] private GameObject _defeatUI;

    private void Start()
    {
        if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        CanvasLifeBar lifebarManager = FindObjectOfType<CanvasLifeBar>();
        lifebarManager?.SpawnBar(this);
    }

    public PCharacter SetInitialParameters(Player clientOwner,int team)
    {
        _clientOwner = clientOwner;
        _currentLife = _maxLife;
        SetTargetPoint();
        photonView.RPC(nameof(RPC_SetClientParams), _clientOwner, _currentLife,team);
        return this;
    }
    #region LocalFunctions
    private void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        GoToTarget();
    }
    public int GetTeam()
    {
        return _team;
    }
    public Player GetClientOwner()
    {
        return _clientOwner;
    }
    void SetCamera()
    {
        _camera = FindObjectOfType<CameraController>();
        _camera.SetTarget(transform);
        FindObjectOfType<PController>().SetCamera(_camera);
    }
    void SetTargetPoint()
    {
        _targetPoint = new GameObject().transform;
        _targetPoint.transform.position = transform.position;
    }
    public void Move(Vector3 dir)
    {
        if (!_targetPoint)
        {
            return;
        }
        _startingForward = transform.forward;
        _timeRotating = _constZero;
        dir.y = _targetPoint.position.y;
        _targetPoint.position = dir;
    }
    public void GoToTarget()
    {
        if (!_targetPoint)
        {
            return;
        }
        if (transform.position == _targetPoint.position)
        {
            return;
        }
        _direction = (_targetPoint.position - transform.position);
        _direction.y = _constZero;
        if (_direction.magnitude <= (_direction.normalized * _speed * Time.deltaTime).magnitude)
        {
            transform.position = _targetPoint.position;
                photonView.RPC(nameof(RPC_AnimMove), RpcTarget.All, false);
        }
        else
        {
            transform.position += _direction.normalized * _speed * Time.deltaTime;
            if (!_animator.GetBool(moveAnimatorParameter))
            {
                photonView.RPC(nameof(RPC_AnimMove), RpcTarget.All, true);
            }
        }
        RotateToTarget();
    }
    public void RotateToTarget()
    {
        if (_direction.normalized == Vector3.zero)
        {
            return;
        }
        if (_timeRotating <= _timeToRotate)
        {
            _timeRotating += Time.deltaTime;
            transform.forward = Vector3.Slerp(_startingForward, _direction.normalized
                , _timeRotating / _timeToRotate);
            Mathf.Clamp(_timeRotating, _constZero, _timeToRotate);
        }
    }
    public void Shoot(Vector3 position, Quaternion rot)
    {
        if (!_canShoot)
        {
            return;
        }
        _canShoot = false;
        StartCoroutine(ShootCD());
        PhotonNetwork.Instantiate(_bulletPrefab.name, position, rot)
                                 .GetComponent<NormalBullet>()
                                 .SetOwner(this)
                                 .SetDmg(_dmg)
                                 .SetColor(myColor, _team);
        photonView.RPC(nameof(RPC_AnimShoot), RpcTarget.All);

    }
    private IEnumerator ShootCD()
    {
        yield return new WaitForSeconds(_attackSpeed);
        _canShoot = true;
    }
    public void Shielding()
    {
        photonView.RPC(nameof(RPC_Shielding), RpcTarget.All);
    }
    public void TakeDmg(float dmg)
    {
        _currentLife -= dmg;

        OnLifebarUpdate(_currentLife / _maxLife);

        if (_currentLife <= 0)
        {
            OnHide();
            photonView.RPC(nameof(RPC_Die), RpcTarget.All);
            WatakshiNoServer.Instance.RequestDie(_clientOwner);
            //WatakshiNoServer.Instance.RPC_PlayerDisconnect(_clientOwner);
            //photonView.RPC(nameof(RPC_DisconnectOwner), _clientOwner);

        }
    }
    private void OnHide()
    {
        GetComponent<Animator>().enabled = false;
        foreach (var item in hideComponents)
        {
            Destroy(item);
        }
        GetComponent<PCharacter>().enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        OnDestroy();
    }
    public void LaunchUI(bool value)
    {
        photonView.RPC(nameof(RPC_ShowFinalUI),_clientOwner,value);
    }
    private void OnApplicationQuit()
    {
        if (_clientOwner == PhotonNetwork.LocalPlayer)
        {
            WatakshiNoServer.Instance.RequestRageQuit(_clientOwner);
        }

        PhotonNetwork.Disconnect();
    }
    #endregion

    #region PunRPCs
    [PunRPC]
    void RPC_SetClientParams(float startHp,int team)
    {
        _clientOwner = PhotonNetwork.LocalPlayer;
        _currentLife = startHp;
        SetCamera();
        photonView.RPC(nameof(RPC_ChangeColor), RpcTarget.All, team);
    }
    [PunRPC]
    void RPC_DisconnectOwner()
    {
        PhotonNetwork.Disconnect();
    }
    [PunRPC]
    void RPC_LifeChange(float newLife)
    {
        _currentLife = newLife;
    }
    [PunRPC]
    void RPC_AnimShoot()
    {
        _animator.SetTrigger(shootAnimatorParameter);
    }
    [PunRPC]
    void RPC_AnimMove(bool value)
    {
        _animator.SetBool(moveAnimatorParameter,value);
    }
    [PunRPC]
    void RPC_Shielding()
    {
        if (!_shieldbc ||!_shield||!_shieldmeshRenderer)
        {
            return;
        }
        _shieldbc.enabled = true;
        _shieldmeshRenderer.enabled = true;
        _shield.TurningOff();
    }
    [PunRPC]
    void RPC_ChangeColor(int team)
    {
        _team = team;
        switch (team)
		{
            case 1:
				myColor = Color.red;
                GetComponent<ColorChanger>().ChangeColor(Color.red);
                break;
			case 2:
				myColor = Color.blue;
				GetComponent<ColorChanger>().ChangeColor(Color.blue);
				break;
			case 3:
				myColor = Color.yellow;
				GetComponent<ColorChanger>().ChangeColor(Color.yellow);
				break;
			case 4:
				myColor = Color.green;
				GetComponent<ColorChanger>().ChangeColor(Color.green);
				break;
			case 5:
				myColor = Color.cyan;
				GetComponent<ColorChanger>().ChangeColor(Color.cyan);
				break;
			case 6:
				myColor = Color.white;
				GetComponent<ColorChanger>().ChangeColor(Color.white);
				break;
			case 7:
				myColor = Color.grey;
				GetComponent<ColorChanger>().ChangeColor(Color.grey);
				break;
			case 8:
				myColor = Color.black;
				GetComponent<ColorChanger>().ChangeColor(Color.black);
				break;
			default:
				break;
		}
    }
    [PunRPC]
    void RPC_Die()
    {
        OnHide();
    }
    [PunRPC]
    void RPC_ShowFinalUI(bool value)
    {
        if (value)
        {
            _victoryUI.gameObject.SetActive(true);
        }
        else
        {
            _defeatUI.gameObject.SetActive(true);
        }
    }
    public void DisconnectToMainMenu()
    {
        photonView.RPC(nameof(RPC_ToMainMenu), _clientOwner);
    }
    [PunRPC]
    void RPC_ToMainMenu()
    {
        PhotonNetwork.Disconnect();
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);

    }
    #endregion
    #region OtherOnes
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_currentLife / _maxLife);
        }
        else
        {
            OnLifebarUpdate((float)stream.ReceiveNext());
        }
    }
    private void OnDestroy()
    {
        OnDeath();
    }
    #endregion
}
