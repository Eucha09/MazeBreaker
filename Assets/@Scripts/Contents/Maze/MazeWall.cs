using Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

public class MazeWall : MazeCell
{
    [SerializeField]
    protected GameObject _wallObject;
    [SerializeField]
    protected Vector3 _upPos;
    [SerializeField]
    protected Vector3 _downPos;
    [SerializeField]
    protected float _speed = 1.0f;

    protected Coroutine _coMove;
    protected bool _update;

	protected Vector3 _initialPosition;
	protected Quaternion _initialRotation;

	protected float _shakeAmount = 1.0f;

    public bool IsMoving { get { return _coMove != null; } }

    public override void Init(MazeObjectData data, Vector2Int gridPos)
    {
        base.Init(data, gridPos);

        TileType type = Managers.Map.GetTileType(GridPos);

        if (type == Define.TileType.Wall)
            _wallObject.transform.localPosition = _upPos;
        else
            _wallObject.transform.localPosition = _downPos;

		_initialPosition = _wallObject.transform.position;
		_initialRotation = _wallObject.transform.rotation;
	}

    void Update()
    {
        if (_update)
        {
            Managers.Star.CreateEdgesAround(transform.position);
            _update = false;
        }
    }

    public void RefreshWall(float delay)
    {
        if (_coMove != null)
            StopCoroutine(_coMove);

        TileType type = Managers.Map.GetTileType(GridPos);
        if (type == TileType.Wall)
            _coMove = StartCoroutine(CoUp(delay));
        else if (type == TileType.Empty)
            _coMove = StartCoroutine(CoDown(delay));
    }

    IEnumerator CoUp(float delay)
    {
		yield return StartCoroutine(CoRestore());

		yield return new WaitForSeconds(delay);

		Managers.Sound.Play3DSound(gameObject, "WallChange", 0.0f, 20.0f);
        while (true)
        {
            // Shake
            float shakeX = Random.Range(-_shakeAmount, _shakeAmount);
            float shakeZ = Random.Range(-_shakeAmount, _shakeAmount);
            _wallObject.transform.localPosition += new Vector3(shakeX, 0, shakeZ) * 0.02f;

            Vector3 dir = _upPos - _wallObject.transform.localPosition;
            _wallObject.transform.localPosition += dir.normalized * _speed * Time.deltaTime;
            if (Vector3.Distance(_wallObject.transform.localPosition, _upPos) < _speed * Time.deltaTime)
            {
                _wallObject.transform.localPosition = _upPos;
                break;
            }

            yield return null;
        }

		_initialPosition = _wallObject.transform.position;
		_initialRotation = _wallObject.transform.rotation;
		_coMove = null;
    }

    IEnumerator CoDown(float delay)
	{
		yield return StartCoroutine(CoRestore());

		yield return new WaitForSeconds(delay);

		Managers.Sound.Play3DSound(gameObject, "WallChange", 0.0f, 20.0f);
        while (true)
        {
            // Shake
            float shakeX = Random.Range(-_shakeAmount, _shakeAmount);
            float shakeZ = Random.Range(-_shakeAmount, _shakeAmount);
            _wallObject.transform.localPosition += new Vector3(shakeX, 0, shakeZ) * 0.02f;

            Vector3 dir = _downPos - _wallObject.transform.localPosition;
            _wallObject.transform.localPosition += dir.normalized * _speed * Time.deltaTime;
            if (Vector3.Distance(_wallObject.transform.localPosition, _downPos) < _speed * Time.deltaTime)
            {
                _wallObject.transform.localPosition = _downPos;
                break;
            }

            yield return null;
        }

		_initialPosition = _wallObject.transform.position;
		_initialRotation = _wallObject.transform.rotation;
		_update = true;
        _coMove = null;
	}

	protected IEnumerator CoRestore()
	{
		float recoverySpeed = 0.0f;

		float elapsedTime = 0f;
		Vector3 startPosition = _wallObject.transform.position;
		Quaternion startRotation = _wallObject.transform.rotation;

		while (elapsedTime < recoverySpeed)
		{
			_wallObject.transform.position = Vector3.Lerp(startPosition, _initialPosition, elapsedTime / recoverySpeed);
			_wallObject.transform.rotation = Quaternion.Lerp(startRotation, _initialRotation, elapsedTime / recoverySpeed);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		_wallObject.transform.position = _initialPosition;
		_wallObject.transform.rotation = _initialRotation;
	}
}
