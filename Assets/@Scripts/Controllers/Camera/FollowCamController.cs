using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class FollowCamController : MonoBehaviour
{
	[SerializeField]
	Transform _target;
    float _followingSpeed = 5f;

    private GameObject _targetTracker;    // 빈 오브젝트

    CinemachineInputAxisController _controller;
    CameraInputActions _inputActions;

    //Tracking Pos 생성 후, 플레이어를 Smooth하게 따라가게 만들기
    void Start()
    {
		_targetTracker = new GameObject("TargetTracker");
        if (_target != null)
            _targetTracker.transform.position = _target.position;
        GetComponent<CinemachineCamera>().Target.TrackingTarget = _targetTracker.transform;
        _controller = GetComponent<CinemachineInputAxisController>();
		_controller.Controllers[0].Enabled = false;
		_controller.Controllers[1].Enabled = false;
        //_inputActions = new CameraInputActions();
        //_inputActions.Camera.Enable();

    }

    void Update()
	{
        if (EventSystem.current.IsPointerOverGameObject())
            _controller.Controllers[2].Enabled = false;
        else
            _controller.Controllers[2].Enabled = true;

        //if (Input.GetMouseButton(1))
        //{
        //    if (!EventSystem.current.IsPointerOverGameObject())
        //    {
        //        _controller.Controllers[0].Enabled = true;
        //        _controller.Controllers[1].Enabled = true;
        //    }
        //}
        //else
        //{
        //    _controller.Controllers[0].Enabled = false;
        //    _controller.Controllers[1].Enabled = false;
        //}


		//PlayerTracker가 따라다니는 부분
		_targetTracker.transform.position = Vector3.Lerp(
			_targetTracker.transform.position,
            _target.position,
            Time.deltaTime * _followingSpeed
        );
    }

    public void EnableCameraRotation(bool enable)
    {
        _controller.Controllers[0].Enabled = enable;
        _controller.Controllers[1].Enabled = enable;
    }

    public void SetTarget(GameObject target)
    {
        _target = target.transform;
	}
}