using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;

public class MazeCell : MonoBehaviour
{
    public Vector2Int GridPos { get; set; }
	public CellType CellType { get; set; }
	public TileType Type { get { return Managers.Map.GetTileType(GridPos); } }

	public TileObjectInfo TileObject { get; set; } // floor, wall, door ...
	public HashSet<CreatureInfo> Monsters { get; set; } = new HashSet<CreatureInfo>();
	public HashSet<NatureInfo> Natures { get; set; } = new HashSet<NatureInfo>();
	public HashSet<RewardObjectInfo> RewardObjects { get; set; } = new HashSet<RewardObjectInfo>();
	public HashSet<BuildingObjectInfo> BuildingObjects { get; set; } = new HashSet<BuildingObjectInfo>();

	int _lastSpawnDate; // 마지막 리스폰된 날

	bool _active;

	Coroutine _coRespawn;

	public virtual void Init(MazeObjectData data, Vector2Int gridPos)
    {
		int rowSize = Managers.Map.RowSize;
		int colSize = Managers.Map.ColSize;

		GridPos = gridPos;
		CellType = data.cellType;
		_lastSpawnDate = Managers.Time.Days;

		TileObject = new TileObjectInfo();
		TileObject.SpawnPos = transform.position;
		TileObject.Rotation = Quaternion.identity;
		if (Mathf.Abs(gridPos.x - colSize / 2) % 2 == 1 && Mathf.Abs(gridPos.y - rowSize / 2) % 2 == 0)
			TileObject.Rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
		TileObject.Cell = this;
		TileObject.Init(data, gridPos);
	}

	public virtual void LoadContent(bool immediately = false)
	{
		_active = true;

		TileObject.LoadObject(immediately);
		foreach (CreatureInfo monster in Monsters)
			monster.LoadObject(immediately);
		foreach (NatureInfo nature in Natures)
			nature.LoadObject(immediately);
		foreach (RewardObjectInfo reward in RewardObjects)
			reward.LoadObject(immediately);
		foreach (BuildingObjectInfo building in BuildingObjects)
			building.LoadObject(immediately);

		Refresh();
	}

    public virtual void UnLoadContent()
	{
		_active = false;

		TileObject.UnLoadObject();
		foreach (CreatureInfo monster in Monsters)
			monster.UnLoadObject();
		foreach (NatureInfo nature in Natures)
			nature.UnLoadObject();
		foreach (RewardObjectInfo reward in RewardObjects)
			reward.UnLoadObject();
		foreach (BuildingObjectInfo building in BuildingObjects)
			building.UnLoadObject();

		if (_coRespawn != null)
		{
			StopCoroutine(_coRespawn);
			_coRespawn = null;
		}

	}

	public void Refresh()
	{
		TileObject.Refresh();

		int respawnPeriod = Managers.Game.RespawnPeriod;
		if (_coRespawn == null && _lastSpawnDate < Managers.Time.Days / respawnPeriod * respawnPeriod)
			Respawn();
	}

	public bool IsLoaded()
	{
		if (TileObject == null || TileObject.IsLoaded() == false)
			return false;
		return true;
	}

	public void Enter(ObjectInfo objectInfo)
	{
		switch (objectInfo.Type)
		{
			case ObjectInfoType.Creature:
				Monsters.Add((CreatureInfo)objectInfo);
				break;
			case ObjectInfoType.Nature:
				Natures.Add((NatureInfo)objectInfo);
				break;
			case ObjectInfoType.RewardObject:
				RewardObjects.Add((RewardObjectInfo)objectInfo);
				break;
			case ObjectInfoType.BuildingObject:
				BuildingObjects.Add((BuildingObjectInfo)objectInfo);
				break;
		}
		objectInfo.Cell = this;
		if (objectInfo.gameObject != null)
			objectInfo.gameObject.transform.SetParent(transform);

		if (_active)
			objectInfo.LoadObject();
		else
			objectInfo.UnLoadObject();
	}

	public void Leave(ObjectInfo objectInfo)
	{
		switch (objectInfo.Type)
		{
			case ObjectInfoType.Creature:
				Monsters.Remove((CreatureInfo)objectInfo);
				break;
			case ObjectInfoType.Nature:
				Natures.Remove((NatureInfo)objectInfo);
				break;
			case ObjectInfoType.RewardObject:
				RewardObjects.Remove((RewardObjectInfo)objectInfo);
				break;
			case ObjectInfoType.BuildingObject:
				BuildingObjects.Remove((BuildingObjectInfo)objectInfo);
				break;
		}
	}

	public bool EnterRandomPosition(ObjectInfo objectInfo)
	{
		bool success = true;

		for (int i = 0; i < 10; i++)
		{
			Vector3 randPos = Vector3.zero;
			randPos.x = Random.Range(-(TileObject.SizeX / 2.0f - objectInfo.ColliderRadius), TileObject.SizeX / 2.0f - objectInfo.ColliderRadius);
			randPos.z = Random.Range(-(TileObject.SizeY / 2.0f - objectInfo.ColliderRadius), TileObject.SizeY / 2.0f - objectInfo.ColliderRadius);
			randPos += transform.position;
			success = true;

			foreach (ObjectInfo obj in Monsters)
			{
				if (success == false)
					break;
				float dist = (obj.SpawnPos - randPos).magnitude;
				if (dist < obj.ColliderRadius + objectInfo.ColliderRadius)
					success = false;
			}
			foreach (ObjectInfo obj in Natures)
			{
				if (success == false)
					break;
				float dist = (obj.SpawnPos - randPos).magnitude;
				if (dist < obj.ColliderRadius + objectInfo.ColliderRadius)
					success = false;
			}
			foreach (BuildingObjectInfo obj in BuildingObjects)
			{
				if (success == false)
					break;
				float dist = (obj.SpawnPos - randPos).magnitude;
				if (dist < obj.ColliderRadius + objectInfo.ColliderRadius)
					success = false;
			}

			if (success)
			{
				objectInfo.SpawnPos = randPos;
				if (objectInfo.MinimapMarker != null)
					objectInfo.MinimapMarker.SetPosition(randPos);
				Enter(objectInfo);
				break;
			}
		}

		return success;
	}

	public void Respawn()
	{
		float delay = Mathf.Max(0.0f, 4.0f - Managers.Time.SecondsToday);

		_coRespawn = StartCoroutine(CoRespawn(delay));
	}

	IEnumerator CoRespawn(float delay = 0.0f)
	{
		yield return new WaitForSeconds(delay);

		_lastSpawnDate = Managers.Time.Days;
		foreach (CreatureInfo obj in Monsters)
		{
			if (obj.IsDead)
			{
				obj.IsDead = false;
				obj.Refresh();
			}
		}
		foreach (CreatureInfo obj in Natures)
		{
			if (obj.IsDead)
			{
				obj.IsDead = false;
				obj.Refresh();
			}
		}

		_coRespawn = null;
	}
}
