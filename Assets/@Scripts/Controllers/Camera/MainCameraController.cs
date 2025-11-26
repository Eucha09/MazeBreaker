using Unity.Cinemachine;
using UnityEngine;
using static Define;
using static UnityEngine.Rendering.DebugUI;

public class MainCameraController : MonoBehaviour
{
    [SerializeField]
    FollowCamController _followCam;
	[SerializeField]
	TopDownCamController _topDownCam;

	public FollowCamController FollowCam { get { return _followCam; } }
	public TopDownCamController TopDownCam { get { return _topDownCam; } }

	Define.CameraType _type = Define.CameraType.Follow;

    void Start()
    {
		
	}

    void Update()
    {
		//if (Input.GetKeyDown(KeyCode.Z))
			//SetCamera(Define.CameraType.Follow);
		//if (Input.GetKeyDown(KeyCode.X))
			//SetCamera(Define.CameraType.TopDown);
	}

    public void SetCameraType(Define.CameraType type)
	{
		_type = type;
		if (_type == Define.CameraType.Follow)
		{
			_followCam.gameObject.SetActive(true);
			_topDownCam.gameObject.SetActive(false);
		}
		else if (_type == Define.CameraType.TopDown)
		{
			_topDownCam.gameObject.SetActive(true);
			_followCam.gameObject.SetActive(false);
		}
	}

	public void SetTarget(GameObject target)
	{
		if (_type == Define.CameraType.Follow)
			_followCam.SetTarget(target);
		else if (_type == Define.CameraType.TopDown)
			_topDownCam.SetTarget(target);
	}

	public void SetTarget(GameObject target, Define.CameraType type)
	{
		SetCameraType(type);
		SetTarget(target);
	}
}
