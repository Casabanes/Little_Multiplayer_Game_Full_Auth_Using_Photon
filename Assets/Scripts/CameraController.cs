using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private float _cameraspeed;
    [SerializeField] private bool _isCameraLocked;
    [SerializeField] private bool _isPermanentLocked;
    [SerializeField] private float _boundsToMoveWithMouse;
    private const int _constZero = 0;
    private float _realBounds;
    private void Start()
    {
        _realBounds = Screen.height / 100 * _boundsToMoveWithMouse;
    }
    void Update()
    {
        CheckLocks();
        if (_isCameraLocked)
        {
            FollowingTarget();
        }
        else
        {
            MoveCameraWithArrows();
            MoveWithMouse();
        }
    }
    public void SetTarget(Transform target)
    {
        _target = target;
        if (_target.gameObject.GetComponent<PCharacter>()._clientOwner != PhotonNetwork.LocalPlayer)
        {
            Destroy(gameObject);
        }
    }
    private void FollowingTarget()
    {
        if (_target)
        {
            transform.position = _target.position + _offset;
        }
    }
    private void CheckLocks()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            _isCameraLocked = !_isCameraLocked;
            _isPermanentLocked = _isCameraLocked;
        }
        if (_isPermanentLocked)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.Space))
        {
            _isCameraLocked = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            _isCameraLocked = false;
        }
    }
    private void MoveCameraWithArrows()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += Vector3.forward * _cameraspeed * Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKey(KeyCode.DownArrow))
        {
            transform.position -= Vector3.forward * _cameraspeed * Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.position += Vector3.right * _cameraspeed * Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position -= Vector3.right * _cameraspeed * Time.deltaTime;
        }
    }
    private void MoveWithMouse()
    {
        Vector2 mousePosition = Input.mousePosition;
        if (mousePosition.x + _realBounds >= Screen.width)
        {
            transform.position += Vector3.right * _cameraspeed * Time.deltaTime;
        }
        if (mousePosition.x - _realBounds <= _constZero)
        {
            transform.position -= Vector3.right * _cameraspeed * Time.deltaTime;
        }
        if (mousePosition.y + _realBounds >= Screen.height)
        {
            transform.position += Vector3.forward * _cameraspeed * Time.deltaTime;
        }
        if (mousePosition.y - _realBounds <= _constZero)
        {
            transform.position -= Vector3.forward * _cameraspeed * Time.deltaTime;
        }
    }
}
