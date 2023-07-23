using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
public class PController : MonoBehaviourPun
{
    [SerializeField] private Player _localPlayer;
    [SerializeField] private LayerMask _floor;
    [SerializeField] private Camera _camera;
    public static PController instance;
    private Action clickPointOnWindow=delegate { };
    void Start()
    {
        if (instance)
        {
            Destroy(instance.gameObject);
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
        _localPlayer = PhotonNetwork.LocalPlayer;
        clickPointOnWindow = ReturnUntilHaveACamera;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            WatakshiNoServer.Instance.RequestShoot(_localPlayer);
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            WatakshiNoServer.Instance.RequestShielding(_localPlayer);
        }
        if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKey(KeyCode.Mouse1))
        {
            clickPointOnWindow.Invoke();
        }
    }
    public void ReturnUntilHaveACamera()
    {
        if (_camera != null)
        {
            clickPointOnWindow=ClickPointOnWindow;
            clickPointOnWindow.Invoke();
        }
    }
    public void ClickPointOnWindow()
    {
            Vector2 pointClicked = Input.mousePosition;
             Ray ray = _camera.ScreenPointToRay(pointClicked);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, _floor))
            {
                WatakshiNoServer.Instance.RequestMove(_localPlayer, raycastHit.point);
            }
    }
    public void SetCamera(CameraController c)
    {
        _camera = c.GetComponent<Camera>();
    }
}
