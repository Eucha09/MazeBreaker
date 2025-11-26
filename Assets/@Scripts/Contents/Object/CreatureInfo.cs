using Data;
using NUnit.Framework;
using TreeMonster.Search;
using UnityEngine;
using static Define;

public class CreatureInfo : ObjectInfo
{
	public int ObjectId { get; set; }
	bool _isDead;
	public virtual bool IsDead
	{
		get { return _isDead; }
		set
		{
			_isDead = value;
			if (MinimapMarker != null)
				MinimapMarker.gameObject.SetActive(_isDead == false);
		}
	}
	public GameObjectType GameObjectType { get; set; }

	// Monster only
	public DetectorType DetectorType { get; set; }
	//public Vector3 CurPos { get; set; }
	public Vector3 ChasingPos { get; set; }
	public bool ChaseTrigger { get; set; }

	public virtual void Init(int objectId)
	{
		Type = ObjectInfoType.Creature;

		ObjectId = objectId;
		ObjectData objectData = null;
		Managers.Data.ObjectDict.TryGetValue(objectId, out objectData);
		if (objectData == null)
			return;

		GameObjectType = objectData.objectType;
		if (objectData.objectType == GameObjectType.Monster)
		{
			MonsterData monsterData = objectData as MonsterData;
			PrefabPath = monsterData.prefabPath;
			DetectorType = monsterData.detectorType;
		}
		else if (objectData.objectType == GameObjectType.Nature)
		{
			NatureData natureData = objectData as NatureData;
			PrefabPath = natureData.prefabPaths[Random.Range(0, natureData.prefabPaths.Count)];
			DetectorType = DetectorType.None;
		}

		ColliderRadius = objectData.colliderRadius;
		IsDead = false;
		//CurPos = SpawnPos;

		if (objectData.minimapMarkerId != 0 && objectData.isAwaysVisibleOnMinimap)
		{
			MinimapMarker = Managers.Resource.Instantiate("Minimap/Minimap_Marker").GetComponent<Minimap_Marker>();
			MinimapMarker.SetTemplateId(objectData.minimapMarkerId, SpawnPos);
			Managers.Minimap.AddMarker(MinimapMarker);
		}
	}

	public override void LoadObject(bool immediately = false)
	{
		_active = true;

		Vector3 curPos = SpawnPos;
		if (DummyObject != null)
		{
			curPos = DummyObject.transform.position;
			DummyObject.GetComponent<DummyController>()?.Unbind();
			Managers.Resource.Destroy(DummyObject);
			DummyObject = null;
		}
		if (gameObject == null && _loading == false)
		{
			if (immediately)
			{
				gameObject = Managers.Resource.Instantiate(PrefabPath, curPos, Rotation, Cell.transform);
				OnSpawn(gameObject);
			}
			else
			{
				_loading = true;
				Managers.Resource.EnqueueInstantiate(PrefabPath, curPos, Rotation, Cell.transform, OnSpawn);
			}
		}
	}

	public override void UnLoadObject()
	{
		_active = false;

		Vector3 curPos = SpawnPos;
		MazeCell spawnCell = Managers.Map.GetCell(SpawnPos);

		if (gameObject != null)
		{
			curPos = gameObject.transform.position;
			gameObject.GetComponent<BaseController>().Unbind();
			Managers.Resource.Destroy(gameObject);
			gameObject = null;
		}
		if (DummyObject == null && (ChaseTrigger || spawnCell != Cell))
		{
			DummyObject = Managers.Resource.Instantiate("Creature/DummyObject", curPos, Rotation, Cell.transform);
			DummyObject.GetComponent<DummyController>().Bind(this);
		}
		if (DummyObject != null && ChaseTrigger == false)
		{
			curPos = DummyObject.transform.position;
			DummyObject.GetComponent<DummyController>()?.Unbind();
			Managers.Resource.Destroy(DummyObject);
			DummyObject = null;
		}
	}

	public override void Refresh()
	{
		if (gameObject != null)
			gameObject.GetComponent<BaseController>().Refresh();
	}

	public void OnSpawn(GameObject go)
	{
		_loading = false;

		if (_active)
		{
			gameObject = go;
			BaseController bc = gameObject.GetComponent<BaseController>();
			bc.Bind(this);
		}
		else
		{
			Managers.Resource.Destroy(go);
		}
	}

	public void SetChasingPos(Vector3 pos)
	{
		if (GameObjectType != GameObjectType.Monster)
			return;

		ChasingPos = pos;
		ChaseTrigger = true;

		if (_active && gameObject != null)
			gameObject.GetComponent<MonsterController2>().SetChasingPos(pos);
		else if (_active == false && DummyObject != null)
			DummyObject.GetComponent<DummyController>().SetChasingPos(pos);
		else if (_active == false && DummyObject == null)
		{
			DummyObject = Managers.Resource.Instantiate("Creature/DummyObject", SpawnPos, Rotation, Cell.transform);
			DummyObject.GetComponent<DummyController>().Bind(this);
		}
	}

	public void ChasingEnd()
	{
		if (GameObjectType != GameObjectType.Monster)
			return;

		ChaseTrigger = false;

		if (_active && gameObject != null)
			gameObject.GetComponent<MonsterController2>().ChasingEnd();
		else if (_active == false && DummyObject != null)
			DummyObject.GetComponent<DummyController>().ChasingEnd();
	}

	public void ReturnSpawnPos()
	{
		if (gameObject != null)
		{
			gameObject.GetComponent<BaseController>().Unbind();
			Managers.Resource.Destroy(gameObject);
			gameObject = null;
		}
		if (DummyObject != null)
		{
			DummyObject.GetComponent<DummyController>()?.Unbind();
			Managers.Resource.Destroy(DummyObject);
			DummyObject = null;
		}

		MazeCell now = Cell;
		MazeCell after = Managers.Map.GetCell(SpawnPos);
		now.Leave(this);
		after.Enter(this);
	}
}
