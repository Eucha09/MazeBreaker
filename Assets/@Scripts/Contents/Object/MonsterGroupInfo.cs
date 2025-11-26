using Data;
using System.Collections.Generic;
using static Define;

public class MonsterGroupInfo : CreatureInfo
{
	public override bool IsDead
	{
		get
		{
			foreach (CreatureInfo monster in Monsters)
				if (monster.IsDead)
					return true;
			return false;
		}
		set
		{
			foreach (CreatureInfo monster in Monsters)
				monster.IsDead = value;
		}
	}

	public List<CreatureInfo> Monsters { get; set; } = new List<CreatureInfo>();

	public override void Init(int objectId)
	{
		Type = ObjectInfoType.Creature;

		ObjectId = objectId;
		ObjectData objectData = null;
		Managers.Data.ObjectDict.TryGetValue(objectId, out objectData);
		if (objectData == null)
			return;

		MonsterGroupData monsterGroupData = objectData as MonsterGroupData;
		foreach (int monsterId in monsterGroupData.monsterIds)
		{
			CreatureInfo creatureInfo = new CreatureInfo();
			creatureInfo.SpawnPos = SpawnPos;
			creatureInfo.Rotation = Rotation;
			creatureInfo.Cell = Cell;
			creatureInfo.Init(monsterId);
			Monsters.Add(creatureInfo);
		}

		ColliderRadius = objectData.colliderRadius; // TODO
	}

	public override void LoadObject(bool immediately = false)
	{
		_active = true;

		foreach (CreatureInfo monster in Monsters)
			monster.LoadObject(immediately);
	}

	public override void UnLoadObject()
	{
		_active = false;

		foreach (CreatureInfo monster in Monsters)
			monster.UnLoadObject();
	}

	public override void Refresh()
	{
		foreach (CreatureInfo monster in Monsters)
			monster.Refresh();
	}
}
