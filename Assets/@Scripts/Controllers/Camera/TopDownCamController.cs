using Unity.Cinemachine;
using UnityEngine;

public class TopDownCamController : MonoBehaviour
{
	[SerializeField]
	Vector3 _followOffset;
	[SerializeField]
	Transform _target;

	void Start()
    {

	}


    void LateUpdate()
    {
		if (_target != null)
			transform.position = _target.transform.position + _followOffset;
	}

	public void SetTarget(GameObject target)
	{
		//GetComponent<CinemachineCamera>().Target.TrackingTarget = target.transform;
		_target = target.transform;

		Vector3 cameraDirection = Camera.main.GetComponent<MainCameraController>().FollowCam.transform.forward;

		Vector3[] directions = {
			Vector3.forward,
			Vector3.left,
			Vector3.back,
			Vector3.right
		};
		int closestDirIndex = 0;
		float closestAngle = float.MaxValue;

		for (int i = 0; i < 4; i++)
		{
			Vector3 direction = directions[i];
			float angle = Vector3.Angle(cameraDirection, direction);
			if (angle < closestAngle)
			{
				closestAngle = angle;
				closestDirIndex = i;
			}
		}

		transform.rotation = Quaternion.Euler(new Vector3(90.0f, 0.0f, 90.0f * closestDirIndex));

	}
}
