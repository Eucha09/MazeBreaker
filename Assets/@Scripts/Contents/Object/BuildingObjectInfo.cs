using Data;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BuildingObjectInfo : ObjectInfo
{
	public int ObjectId { get; set; }
	public float Hp { get; set; }

	public int SlotCount { get; set; }
	public int StartSlotNum { get; set; }
	public int EndSlotNum { get; set; }

	public WorkState State { get; set; }
	public int CurItemId { get; set; }
	public int NextItemId { get; set; }
	public double WorkStartTime { get; set; }
	public double WorkEndTime { get; set; }
	public double CombustionStartTime { get; set; }
	public double CombustionEndTime { get; set; }
	public List<ItemInfo> PendingItems { get; set; } = new List<ItemInfo>();

	public virtual void Init(int objectId)
	{
		Type = ObjectInfoType.BuildingObject;

		ObjectId = objectId;
		ObjectData objectData = null;
		Managers.Data.ObjectDict.TryGetValue(objectId, out objectData);
		if (objectData == null)
			return;
		StructureData structureData = objectData as StructureData;
		PrefabPath = structureData.prefabPath;
		ColliderRadius = structureData.colliderRadius;

		Hp = structureData.stat.maxHp;
		SlotCount = structureData.slotCount;
		if (SlotCount > 0)
		{
			int startSlotNum;
			int endSlotNum;
			Managers.Inven.CreateStorage(SlotCount, out startSlotNum, out endSlotNum);
			StartSlotNum = startSlotNum;
			EndSlotNum = endSlotNum;
		}
	}

	public void SetSlotCount(int count)
	{
		SlotCount = count;
		if (SlotCount > 0)
		{
			int startSlotNum;
			int endSlotNum;
			Managers.Inven.CreateStorage(SlotCount, out startSlotNum, out endSlotNum);
			StartSlotNum = startSlotNum;
			EndSlotNum = endSlotNum;
		}
	}

	public void PushAllItems()
	{
		if (SlotCount == 0)
			return;
		for (int i = 0; i < Managers.Inven.SlotCount; i++)
		{
			Item curItem = Managers.Inven.GetItemBySlot(i);
			if (curItem == null)
				continue;
			if (curItem.ItemType == Define.ItemType.SpecialItem && curItem.TemplateId != 600)
				continue;
			ItemInfo itemInfo = new ItemInfo
			{
				TemplateId = curItem.TemplateId,
				Count = curItem.Count,
				Durability = curItem.Durability,
				Equipped = false
			};
			Item newItem = Item.MakeItem(itemInfo);
			int remainCnt = Managers.Inven.Add(newItem, StartSlotNum, EndSlotNum);
			Managers.Inven.RemoveFromSlot(i, curItem.Count - remainCnt);
		}
	}

	public override void LoadObject(bool immediately = false)
	{
		_active = true;

		if (gameObject == null && _loading == false)
		{
			if (immediately)
			{
				gameObject = Managers.Resource.Instantiate(PrefabPath, SpawnPos, Rotation, Cell.transform);
				OnSpawn(gameObject);
			}
			else
			{
				_loading = true;
				Managers.Resource.EnqueueInstantiate(PrefabPath, SpawnPos, Rotation, Cell.transform, OnSpawn);
			}
		}
	}

	public override void UnLoadObject()
	{
		_active = false;

		if (gameObject != null)
		{
			gameObject.GetComponent<BaseController>().Unbind();
			Managers.Resource.Destroy(gameObject);
			gameObject = null;
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
			gameObject.GetComponent<StructureController>()?.Bind(this);
			gameObject.GetComponent<Storage>()?.Bind(this);
			gameObject.GetComponent<RefinementSystem>()?.Bind(this);
			gameObject.GetComponent<CraftSystem>()?.Bind(this);
			gameObject.GetComponent<RestingSystem>()?.Bind(this);
		}
		else
		{
			Managers.Resource.Destroy(go);
		}
	}
}
