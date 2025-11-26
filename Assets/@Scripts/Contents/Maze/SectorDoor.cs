using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SectorDoor : MonoBehaviour
{
    [SerializeField]
    string _keyStr;

	MazeCell _mazeCell;

	[SerializeField]
	List<GameObject> _movingObjects = new List<GameObject>();
	[SerializeField]
	List<Vector3> _closedPos = new List<Vector3>();
	[SerializeField]
	List<Vector3> _openPos = new List<Vector3>();
	[SerializeField]
	float _speed = 1.0f;

	Coroutine _coMove;

	void Start()
	{
		_mazeCell = GetComponentInParent<MazeCell>(true);

		TileType type = _mazeCell.Type;

		if (type == Define.TileType.Wall)
		{
			for (int i = 0; i < _movingObjects.Count; i++)
				_movingObjects[i].transform.localPosition = _closedPos[i];
		}
		else
		{
			for (int i = 0; i < _movingObjects.Count; i++)
				_movingObjects[i].transform.localPosition = _openPos[i];
		}

		CheckStatus();
		Managers.Event.GameEvents.GameEventAction += (status) => { CheckStatus(); };
	}

	public void CheckStatus()
	{
		if (Managers.Game.CheckStatus(_keyStr))
			Open();
	}

	public void Open()
	{
		if (_coMove != null)
			StopCoroutine(_coMove);

		Managers.Map.SetTileType(_mazeCell.GridPos, Define.TileType.Empty);
		_coMove = StartCoroutine(CoOpen());
	}

	public void Close()
	{
		if (_coMove != null)
			StopCoroutine(_coMove);

		Managers.Map.SetTileType(_mazeCell.GridPos, Define.TileType.Wall);
		_coMove = StartCoroutine(CoClose());
	}

	protected IEnumerator CoClose()
	{
		Managers.Sound.Play3DSound(gameObject, "WallChange", 0.0f, 20.0f);
		while (true)
		{
			bool _updated = false;
			for (int i = 0; i < _movingObjects.Count; i++)
			{
				Vector3 dir = _closedPos[i] - _movingObjects[i].transform.localPosition;
				_movingObjects[i].transform.localPosition += dir.normalized * _speed * Time.deltaTime;
				if (Vector3.Distance(_movingObjects[i].transform.localPosition, _closedPos[i]) < _speed * Time.deltaTime)
				{
					_movingObjects[i].transform.localPosition = _closedPos[i];
					continue;
				}
				_updated = true;
			}

			if (_updated == false)
				break;
			yield return null;
		}

		_coMove = null;
	}

	protected IEnumerator CoOpen()
	{
		Managers.Sound.Play3DSound(gameObject, "WallChange", 0.0f, 20.0f);
		while (true)
		{
			bool _updated = false;
			for (int i = 0; i < _movingObjects.Count; i++)
			{
				Vector3 dir = _openPos[i] - _movingObjects[i].transform.localPosition;
				_movingObjects[i].transform.localPosition += dir.normalized * _speed * Time.deltaTime;
				if (Vector3.Distance(_movingObjects[i].transform.localPosition, _openPos[i]) < _speed * Time.deltaTime)
				{
					_movingObjects[i].transform.localPosition = _openPos[i];
					continue;
				}
				_updated = true;
			}

			if (_updated == false)
				break;
			yield return null;
		}

		_coMove = null;
	}
}
