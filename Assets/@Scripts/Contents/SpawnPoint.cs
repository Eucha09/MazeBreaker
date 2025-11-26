using Data;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static Define;
using Random = UnityEngine.Random;

public class SpawnPoint : MonoBehaviour
{

    [SerializeField]
    int _objectId;
	GameObject _spawnObject;
	float _collisionRadius = 1.0f; // TODO

	int _respawnPeriod = 1;
	int _dayOfRespawn = 0;

	public int ObjectId { get { return _objectId; } set { _objectId = value; } }
	public GameObject SpawnObject { get { return _spawnObject; } }
	public float CollisionRadius { get { return _collisionRadius; } set { _collisionRadius = value; } }

	Coroutine _coRespawn;

    void Start()
    {
        if (_objectId == 0)
            return;

		MazeCell mazeCell = GetComponentInParent<MazeCell>(true);
		ObjectInfo objectInfo = Managers.Object.GenerateObjectInfo(_objectId, transform.position, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
		mazeCell.Enter(objectInfo);

	}
}
