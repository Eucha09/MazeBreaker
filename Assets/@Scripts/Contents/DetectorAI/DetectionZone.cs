using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class DetectionZone : MonoBehaviour
{
	DetectorAI _detectorAI;
	DetectorType _detectorType;
	Transform _player;
	List<CreatureInfo> _monsters = new List<CreatureInfo>();
	int _minY;
	int _maxY;
	int _minX;
	int _maxX;
	Vector2Int _center;
	int _range;

	Minimap_Zone _minimapZone;

	public void SetInfo(DetectorAI detectorAI, DetectorType detectorType, Transform player, Vector2Int center, int range, List<CreatureInfo> monsters)
	{
		_detectorAI = detectorAI;
		_detectorType = detectorType;

		_center = center;
		_range = range;

		range = range * 2; // 보정
		_minY = center.y - range;
		_maxY = center.y + range;
		_minX = center.x - range;
		_maxX = center.x + range;

		_player = player;
		_monsters.AddRange(monsters);

		if (_minimapZone == null)
		{
			Vector3 pos = Managers.Map.GridToWorld(center);
			_minimapZone = Managers.Resource.Instantiate("Minimap/Minimap_Zone").GetComponent<Minimap_Zone>();
			_minimapZone.SetInfo(pos, _range, new Color(1.0f, 0.0f, 0.0f, 0.3f));
			Managers.Minimap.AddZone(_minimapZone);
		}
	}

	void Update()
	{
		if (_player == null)
			return;

		Vector2Int playerGridPos = Managers.Map.WorldToGrid(_player.position);
		bool inside = (playerGridPos.y >= _minY && playerGridPos.y <= _maxY && playerGridPos.x >= _minX && playerGridPos.x <= _maxX);
		bool anyActive = false;
		
		if (inside)
		{
			foreach (CreatureInfo monster in _monsters)
			{
				if (monster.IsDead)
					continue;
				monster.SetChasingPos(_player.position);
				anyActive = true;
			}
		}

		if (inside == false || anyActive == false)
		{
			foreach (CreatureInfo monster in _monsters)
			{
				if (monster.IsDead)
					continue;
				monster.ChasingEnd();
			}
			Managers.Minimap.RemoveZone(_minimapZone);
			_detectorAI.RemoveDetectionZone(_detectorType);
		}
	}

	//IEnumerator CoSearchMonsters()
	//{
	//	List<CreatureInfo> monsters = _detectorAI.GetAdjacentMonster(_center, _range, _detectorType);

	//	foreach (CreatureInfo monster in monsters)
	//		_monsters.Add(monster);
	//}
}
