using System.Collections;
using UnityEngine;
using static Define;

public class MovingWall : MonoBehaviour
{
	Vector2Int _gridPos;
	TileType _type;

	[SerializeField]
	protected GameObject _wallObject;
	[SerializeField]
	protected Vector3 _upPos;
	[SerializeField]
	protected Vector3 _downPos;
	[SerializeField]
	float _duration = 7.5f;

	protected Coroutine _coMove;
	protected bool _update;

	protected float _shakeAmount = 0.02f;

	public bool IsMoving { get { return _coMove != null; } }

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
		if (_type != Managers.Map.GetTileType(_gridPos) && Managers.Time.IsNight && _coMove == null)
		{
			float endTime = Managers.Time.DayLength + Managers.Time.NightLength / 3.0f;
			float range = Mathf.Max(0.0f, endTime - Managers.Time.SecondsToday - _duration);
			float delay = Random.Range(0.0f, range);
			if (Managers.Map.GetTileType(_gridPos) == TileType.Wall)
				_coMove = StartCoroutine(CoUp(delay));
			else if (Managers.Map.GetTileType(_gridPos) == TileType.Empty)
				_coMove = StartCoroutine(CoDown(delay));
		}
		else
		{
			_type = Managers.Map.GetTileType(_gridPos);
			if (_type == Define.TileType.Wall)
				_wallObject.transform.localPosition = _upPos;
			else
				_wallObject.transform.localPosition = _downPos;
		}
	}

	IEnumerator CoUp(float delay)
	{
		yield return new WaitForSeconds(delay);

		Managers.Sound.Play3DSound(gameObject, "WallChange", 0.0f, 20.0f);

		for (float i = 0.0f; i < _duration; i += Time.deltaTime)
		{
			// Shake
			float shakeX = Random.Range(-_shakeAmount, _shakeAmount);
			float shakeZ = Random.Range(-_shakeAmount, _shakeAmount);

			_wallObject.transform.localPosition = Vector3.Lerp(_downPos, _upPos, i / _duration) + new Vector3(shakeX, 0, shakeZ);
			yield return null;
		}

		_wallObject.transform.localPosition = _upPos;

		_type = TileType.Wall;
		_coMove = null;
	}

	IEnumerator CoDown(float delay)
	{
		yield return new WaitForSeconds(delay);

		Managers.Sound.Play3DSound(gameObject, "WallChange", 0.0f, 20.0f);

		for (float i = 0.0f; i < _duration; i += Time.deltaTime)
		{
			// Shake
			float shakeX = Random.Range(-_shakeAmount, _shakeAmount);
			float shakeZ = Random.Range(-_shakeAmount, _shakeAmount);

			_wallObject.transform.localPosition = Vector3.Lerp(_upPos, _downPos, i / _duration) + new Vector3(shakeX, 0, shakeZ);
			yield return null;
		}

		_wallObject.transform.localPosition = _downPos;

		_type = TileType.Empty;
		_update = true;
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
