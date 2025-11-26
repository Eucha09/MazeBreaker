using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CraftSystem : MonoBehaviour
{
	[SerializeField]
	int _objectId;
	public int ObjectId { get { return _objectId; } set { _objectId = value; } }
	public string Name { get; private set; }
	public bool IsPlayer { get; set; }

	WorkState _state;
	public WorkState State
	{
		get { return _state; }
		set
		{
			if (_state == value)
				return;
			_state = value;
		}
	}
	public int CraftingItemId { get; private set; }
	double _workStartTime;
	double _workEndTime;
	List<ItemInfo> _pendingItems = new List<ItemInfo>();

	public float ProgressRatio
	{
		get
		{
			if (_workEndTime - _workStartTime < 0.001)
				return 0.0f;
			double ratio = (Managers.Time.CurTime - _workStartTime) / (_workEndTime - _workStartTime);
			return Mathf.Clamp01((float)ratio);
		}
	}

	Storage _storage;

	#region ObjectInfoBinder
	public BuildingObjectInfo Info { get; set; }

	public void Bind(BuildingObjectInfo info)
	{
		Info = info;

		State = Info.State;
		CraftingItemId = Info.CurItemId;
		_workStartTime = Info.WorkStartTime;
		_workEndTime = Info.WorkEndTime;
		_pendingItems = Info.PendingItems;
	}

	public void Refresh()
	{
		if (Info == null)
			return;

	}

	public void Unbind()
	{
		Info.State = State;
		Info.CurItemId = CraftingItemId;
		Info.WorkStartTime = _workStartTime;
		Info.WorkEndTime = _workEndTime;
		_pendingItems = null;
		Info = null;
	}
	#endregion

	void Start()
    {
		_storage = GetComponent<Storage>();

		if (GetComponent<PlayerController>())
		{
			IsPlayer = true;
			_objectId = 0;
		}
		else
		{
			_objectId = GetComponent<BaseController>().TemplateId;
			ObjectData objectData = null;
			Managers.Data.ObjectDict.TryGetValue(_objectId, out objectData);
			Name = objectData.name;
		}
	}

	void Update()
	{
		switch (State)
		{
			case WorkState.Idle:
				UpdateIdle();
				break;
			case WorkState.Work:
				UpdateWork();
				break;
			case WorkState.Pending:
				UpdatePending();
				break;
		}
	}

	void UpdateIdle()
	{
		if (_pendingItems.Count > 0)
		{
			State = WorkState.Pending;
			return;
		}

		if (CraftingItemId != 0)
		{
			State = WorkState.Work;
			return;
		}
	}

	void UpdateWork()
	{
		if (_pendingItems.Count > 0)
		{
			State = WorkState.Pending;
			return;
		}

		if (CraftingItemId == 0)
		{
			State = WorkState.Idle;
			return;
		}

		if (CraftingItemId != 0 && _workEndTime <= Managers.Time.CurTime)
		{
			Managers.Sound.Play("UISOUND/CraftSucceed");
			ItemInfo itemInfo = new ItemInfo()
			{
				TemplateId = CraftingItemId,
				Count = 1,
				Equipped = false
			};
			Item newItem = Item.MakeItem(itemInfo);
			int remainCount = 0;
			if (IsPlayer)
				remainCount = Managers.Inven.Add(newItem);
			else
				remainCount = _storage.Add(newItem);

			if (remainCount > 0)
			{
				itemInfo.Count = remainCount;
				_pendingItems.Add(itemInfo);
			}
			if (_pendingItems.Count == 0)
				CraftingItemId = 0;
		}
	}

	float _delay;
	void UpdatePending()
	{
		if (_pendingItems.Count == 0)
		{
			CraftingItemId = 0;
			State = WorkState.Idle;
			return;
		}
		if (_delay > 0.0f)
		{
			_delay -= Time.unscaledDeltaTime;
			return;
		}

		for (int i = _pendingItems.Count - 1; i >= 0; i--)
		{
			Item newItem = Item.MakeItem(_pendingItems[i]);
			int remainCount = 0;
			if (IsPlayer)
				remainCount = Managers.Inven.Add(newItem);
			else
				remainCount = _storage.Add(newItem);

			if (remainCount == 0)
				_pendingItems.RemoveAt(i);
			else
			{
				_pendingItems[i].Count = remainCount;
				_delay = 0.25f;
			}
		}
	}

	public bool Craft(int templateId)
	{
		if (CraftingItemId != 0)
			return false;

		CraftingData craftingData = null;
		Managers.Data.CraftingDict.TryGetValue(templateId, out craftingData);

		if (!CanCraft(craftingData))
			return false;

		foreach (RequiredItem requiredItem in craftingData.requiredItems)
			Managers.Inven.Remove(requiredItem.id, requiredItem.count);

		_workStartTime = Managers.Time.CurTime;
		_workEndTime = _workStartTime + craftingData.timeRequired;
		CraftingItemId = templateId;
		return true;
	}

    public bool CanCraft(int templateId)
	{
		CraftingData craftingData = null;
		Managers.Data.CraftingDict.TryGetValue(templateId, out craftingData);

		return CanCraft(craftingData);
	}

	public bool CanCraft(CraftingData craftingData)
	{
		bool possible = true;
		foreach (RequiredItem requiredItem in craftingData.requiredItems)
		{
			int count = Managers.Inven.GetItemCount(requiredItem.id);
			if (count < requiredItem.count)
				possible = false;
		}

		return possible;
	}

	public void Cancel(int templateId)
	{
		if (templateId == 0 || CraftingItemId != templateId)
			return;

		if (_pendingItems.Count > 0)
			return;

		CraftingItemId = 0;

		CraftingData craftingData = null;
		Managers.Data.CraftingDict.TryGetValue(templateId, out craftingData);

		foreach (RequiredItem requiredItem in craftingData.requiredItems)
		{
			ItemInfo itemInfo = new ItemInfo()
			{
				TemplateId = requiredItem.id,
				Count = requiredItem.count,
				Equipped = false
			};
			Item newItem = Item.MakeItem(itemInfo);
			int remainCount = 0;
			if (IsPlayer)
				remainCount = Managers.Inven.Add(newItem);
			else
				remainCount = _storage.Add(newItem);
			if (remainCount > 0)
			{
				PlayerController player = Managers.Object.GetPlayer();
				RewardObjectInfo rewardObject = new RewardObjectInfo();
				rewardObject.SpawnPos = player.transform.position;
				rewardObject.Init(itemInfo.TemplateId, remainCount, player.transform.position);
				Managers.Map.ApplyEnter(rewardObject);
			}
		}
	}
}
