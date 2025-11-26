using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

public class GladeDoor : MonoBehaviour
{
	Vector2Int _gridPos;
	MazeCell _mazeCell;
    TileType _type;

	[SerializeField]
	List<GameObject> _movingObjects = new List<GameObject>();
	[SerializeField]
	List<Vector3> _closedPos = new List<Vector3>();
	[SerializeField]
	List<Vector3> _openPos = new List<Vector3>();
	[SerializeField]
	float _duration = 3.0f;

	Coroutine _coMove;

	float _shakeAmount = 0.02f;

	public TileObjectInfo Info { get; set; }

	public void Bind(TileObjectInfo info)
	{
		Info = info;

		_gridPos = Info.GridPos;
		_type = Info.TileType;

		Init();
	}

	public void Refresh()
	{
		Init();
	}

	public void Unbind()
	{
		Info.TileType = _type;
		Info = null;
	}

	public void Init()
	{
		if (_type != Managers.Map.GetTileType(_gridPos) && _coMove == null)
		{
			TileType nextType = Managers.Map.GetTileType(_gridPos);
			if (nextType == TileType.Wall)
				_coMove = StartCoroutine(CoClose());
			else if (nextType == TileType.Empty)
				_coMove = StartCoroutine(CoOpen());
		}
	}

	protected IEnumerator CoClose()
	{
		Managers.Sound.Play3DSound(gameObject, "WallChange", 0.0f, 20.0f);
		
        for (float i = 0.0f; i < _duration; i += Time.deltaTime)
		{
			// Shake
			float shakeY = Random.Range(-_shakeAmount, _shakeAmount);
			float shakeZ = Random.Range(-_shakeAmount, _shakeAmount);
            for (int j = 0; j < _movingObjects.Count; j++)
                _movingObjects[j].transform.localPosition = Vector3.Lerp(_openPos[j], _closedPos[j], i / _duration) + new Vector3(0, shakeY, shakeZ);
			yield return null;
		}

		for (int j = 0; j < _movingObjects.Count; j++)
            _movingObjects[j].transform.localPosition = _closedPos[j];

        _type = TileType.Wall;
        _coMove = null;
	}

	protected IEnumerator CoOpen()
	{
		Managers.Sound.Play3DSound(gameObject, "WallChange", 0.0f, 20.0f);

		for (float i = 0.0f; i < _duration; i += Time.deltaTime)
		{
			// Shake
			float shakeY = Random.Range(-_shakeAmount, _shakeAmount);
			float shakeZ = Random.Range(-_shakeAmount, _shakeAmount);
			for (int j = 0; j < _movingObjects.Count; j++)
				_movingObjects[j].transform.localPosition = Vector3.Lerp(_closedPos[j], _openPos[j], i / _duration) + new Vector3(0, shakeY, shakeZ);
			yield return null;
		}

		for (int j = 0; j < _movingObjects.Count; j++)
			_movingObjects[j].transform.localPosition = _openPos[j];

		_type = TileType.Empty;
		_coMove = null;
	}

	void OnDisable()
	{
		if (_coMove != null)
		{
			StopCoroutine(_coMove);
			_coMove = null;
		}
	}
}
