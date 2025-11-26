using Data;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using static Define;
using static UnityEditor.Experimental.GraphView.GraphView;

public class DetectorAI : MonoBehaviour
{
    Dictionary<DetectorType, HostilityMeter> _hostilities = new Dictionary<DetectorType, HostilityMeter>();
	Dictionary<DetectorType, DetectionZone> _zones = new Dictionary<DetectorType, DetectionZone>();
    int _detectorRange = 2;

    Coroutine _coSearchHostility;

	void Start()
    {
		gameObject.GetOrAddComponent<DetectorEventRouter>();

		// 각 DetectorType별 살기 게이지 초기화
		foreach (DetectorType type in Enum.GetValues(typeof(DetectorType)))
		{
			if (type == DetectorType.None)
				continue;
			_hostilities[type] = new HostilityMeter(type);
		}

		_coSearchHostility = StartCoroutine(CoSearchHostiility(1f));
	}

	public void RemoveDetectionZone(DetectorType detectorType)
	{
		if (_zones.TryGetValue(detectorType, out var zone))
		{
			Managers.Resource.Destroy(zone.gameObject);
			_zones.Remove(detectorType);
		}
		if (_hostilities.TryGetValue(detectorType, out var hostility))
			hostility.Reset();
	}

	public void IncreaseHostility(DetectorType type, float amount, bool force = false)
	{
		if (force == false && _zones.TryGetValue(type, out var detectionZone))
			return;
		if (!_hostilities.TryGetValue(type, out var hostility))
			return;
		if (hostility.IsActive == false)
			return;

		hostility.Increase(amount);

		if (hostility.IsTriggered)
		{
			// 주변에 해당 타입의 몬스터가 있다면 추격 개시
			TriggerDetectionZone(type);
		}
	}

	void TriggerDetectionZone(DetectorType detectorType)
	{
		PlayerController player = Managers.Object.GetPlayer();
        if (player == null)
            return;
        Vector2Int playerGridPos = Managers.Map.WorldToGrid(player.transform.position);
		List<CreatureInfo> monsters = GetAdjacentMonster(playerGridPos, _detectorRange, detectorType);
		if (monsters.Count == 0)
			return;

		if (!_zones.TryGetValue(detectorType, out var zone))
		{
			GameObject go = new($"@DetectionZone_{detectorType}");
			go.transform.parent = transform;
			zone = go.AddComponent<DetectionZone>();
			_zones[detectorType] = zone;
		}
		zone.SetInfo(this, detectorType, player.transform, playerGridPos, _detectorRange, monsters);
	}

	public List<CreatureInfo> GetAdjacentMonster(Vector2Int gridPos, int range, DetectorType detectorType)
	{
		range = range * 2; // 보정
		List<CreatureInfo> monsters = new List<CreatureInfo>();
		for (int x = -range; x <= range; x++)
		{
			for (int y = -range; y <= range; y++)
			{
				Vector2Int checkPos = new Vector2Int(gridPos.x + x, gridPos.y + y);
				if (checkPos.x < 0 || checkPos.y < 0 || checkPos.x >= Managers.Map.ColSize || checkPos.y >= Managers.Map.RowSize)
					continue;
				MazeCell cell = Managers.Map.Maze.Cells[checkPos.y, checkPos.x];
				if (cell == null)
					continue;

				foreach (CreatureInfo monster in cell.Monsters)
				{
					if (monster.DetectorType == detectorType && monster.IsDead == false)
						monsters.Add(monster);
				}
			}
		}

		return monsters;
	}

	IEnumerator CoSearchHostiility(float time)
	{
		while (true)
		{
			PlayerController player = Managers.Object.GetPlayer();
			if (player != null)
			{
				Vector2Int playerGridPos = Managers.Map.WorldToGrid(player.transform.position);
				HashSet<DetectorType> detectorTypes = GetAdjacentDetectorTypes(playerGridPos, _detectorRange);

				foreach (DetectorType detectorType in _hostilities.Keys)
				{
					if (detectorTypes.Contains(detectorType))
						_hostilities[detectorType].IsActive = true;
					else
					{
						_hostilities[detectorType].IsActive = false;
						_hostilities[detectorType].Reset();
					}
				}
			}

			yield return new WaitForSeconds(time);
		}
	}

	HashSet<DetectorType> GetAdjacentDetectorTypes(Vector2Int gridPos, int range)
	{
		range = range * 2; // 보정
		HashSet<DetectorType> detectorTypes = new HashSet<DetectorType>();
		for (int x = -range; x <= range; x++)
		{
			for (int y = -range; y <= range; y++)
			{
				Vector2Int checkPos = new Vector2Int(gridPos.x + x, gridPos.y + y);
				if (checkPos.x < 0 || checkPos.y < 0 || checkPos.x >= Managers.Map.ColSize || checkPos.y >= Managers.Map.RowSize)
					continue;
				MazeCell cell = Managers.Map.Maze.Cells[checkPos.y, checkPos.x];
				if (cell == null)
					continue;
				foreach (CreatureInfo monster in cell.Monsters)
				{
					if (monster.IsDead == false)
						detectorTypes.Add(monster.DetectorType);
				}
			}
		}
		return detectorTypes;
	}
}
