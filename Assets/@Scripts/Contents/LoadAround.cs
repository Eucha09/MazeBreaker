using UnityEngine;

public class LoadAround : MonoBehaviour
{
	[SerializeField]
	int _range;
	Vector2Int _lastGridPos;
	float _deadZone = 18.0f; // 셀 내 허용 이동량

	void Start()
    {
		_deadZone = Managers.Map.CellSize * 2.0f;
		_lastGridPos = Managers.Map.WorldToGrid(transform.position);
		Managers.Map.LoadContent(_lastGridPos, _range, true);
	}

    void Update()
	{
		Vector3 pos = transform.position;

		// dead zone 확인
		Vector3 center = Managers.Map.GridToWorld(_lastGridPos);
		float dist = Vector3.Distance(pos, center);

		if (dist < _deadZone)
			return;  // 아직 경계로 이동했다고 판단하지 않음

		Vector2Int curGridPos = Managers.Map.WorldToGrid(pos);
		if (_lastGridPos != curGridPos)
		{
			Managers.Map.LoadContent(curGridPos, _range);
			Managers.Map.UnloadContent(_lastGridPos, _range);
			_lastGridPos = curGridPos;
			Managers.Event.GameEvents.OnLoadOrUnloadContent();
		}
	}
}
