using UnityEngine;

public class UI_Minimap : UI_Base
{
	[SerializeField]
	Transform _compass;

	public override void Init()
	{

	}

	void Update()
	{
		Vector3 cameraForward = Camera.main.transform.forward;
		cameraForward.y = 0;
		float angle = Vector3.SignedAngle(Vector3.forward, cameraForward, Vector3.up);
		_compass.rotation = Quaternion.Euler(0, 0, angle);
	}
}
