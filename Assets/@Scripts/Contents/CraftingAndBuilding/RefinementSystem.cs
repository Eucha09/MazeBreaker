using Data;
using Newtonsoft.Json.Bson;
using NUnit.Framework;
using NUnit.Framework.Internal.Execution;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class RefinementSystem : MonoBehaviour
{
	[SerializeField]
	int _objectId;
	public int ObjectId { get { return _objectId; } set { _objectId = value; } }
	public string Name { get; private set; }

	WorkState _state;
	public WorkState State
	{
		get { return _state; }
		set
		{
			_state = value;
			UpdateAnimation();
		}
	}
	public int CurRefinementId { get; private set; }
	public int NextRefinementId { get; private set; }
	double _workStartTime;
	double _workEndTime;
	double _combustionStartTime;
	double _combustionEndTime;
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
	public float FuelProgressRatio
	{ 
		get
		{
			if (_combustionEndTime - _combustionStartTime < 0.001)
				return 0.0f;
			double ratio = (Managers.Time.CurTime - _combustionStartTime) / (_combustionEndTime - _combustionStartTime);
			return Mathf.Clamp01((float)ratio);
		} 
	}

	StorageIO _storageIO;

	Animator _animator;
	[SerializeField]
	GameObject _effect;
	[SerializeField]
	AudioClip _audioClip;

	void UpdateAnimation()
	{
		if (State == WorkState.Idle || State == WorkState.Pending)
		{
			if (_animator != null)
				_animator.CrossFade("IDLE", 0.1f, -1, 0);
			if (_effect != null)
				_effect.SetActive(false);
			if (_audioClip != null)
				Managers.Sound.Stop3DLoop(gameObject.GetComponent<AudioSource>());
		}
		else if (State == WorkState.Work)
		{
			if (_animator != null)
				_animator.CrossFade("WORK", 0.1f, -1, 0);
			if (_effect != null)
				_effect.SetActive(true);
			if (_audioClip != null)
				Managers.Sound.Play3DLoop(gameObject, _audioClip, 0, 20.0f);
		}
	}

	#region ObjectInfoBinder
	public BuildingObjectInfo Info { get; set; }

	public void Bind(BuildingObjectInfo info)
	{
		Info = info;

		State = Info.State;
		CurRefinementId = Info.CurItemId;
		NextRefinementId = Info.NextItemId;
		_workStartTime = Info.WorkStartTime;
		_workEndTime = Info.WorkEndTime;
		_combustionStartTime = Info.CombustionStartTime;
		_combustionEndTime = Info.CombustionEndTime;
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
		Info.CurItemId = CurRefinementId;
		Info.NextItemId = NextRefinementId;
		Info.WorkStartTime = _workStartTime;
		Info.WorkEndTime = _workEndTime;
		Info.CombustionStartTime = _combustionStartTime;
		Info.CombustionEndTime = _combustionEndTime;
		_pendingItems = null;
		Info = null;
	}
	#endregion

	void Start()
    {
		_animator = GetComponentInChildren<Animator>();
		_storageIO = GetComponent<StorageIO>();

		_objectId = GetComponent<BaseController>().TemplateId;
		ObjectData objectData = null;
		Managers.Data.ObjectDict.TryGetValue(_objectId, out objectData);
		Name = objectData.name;

		if (_effect != null)
			_effect.SetActive(false);
	}

    void Update()
	{
		switch(State)
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

		if (CurRefinementId != 0 || NextRefinementId != 0)
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

		if (CurRefinementId == 0 && NextRefinementId == 0)
		{
			State = WorkState.Idle;
			return;
		}

		if (CurRefinementId == 0 && NextRefinementId != 0)
		{
			RefinementData refinementData = null;
			Managers.Data.RefinementDict.TryGetValue(NextRefinementId, out refinementData);

			if (!CanCraft(refinementData))
			{
				NextRefinementId = 0;
				State = WorkState.Idle;
				return;
			}
			if (!IsWorkable())
			{
				NextRefinementId = 0;
				State = WorkState.Idle;
				return;
			}

			foreach (RequiredItem requiredItem in refinementData.inputItems)
				_storageIO.RemoveFromInput(requiredItem.id, requiredItem.count);

			CurRefinementId = NextRefinementId;
			_workStartTime = Managers.Time.CurTime;
			_workEndTime = _workStartTime + refinementData.timeRequired;
		}

		if (CurRefinementId != 0 && _workEndTime <= Managers.Time.CurTime)
		{
			RefinementData refinementData = null;
			Managers.Data.RefinementDict.TryGetValue(CurRefinementId, out refinementData);

			foreach (RequiredItem requiredItem in refinementData.outputItems)
			{
				ItemInfo itemInfo = new ItemInfo()
				{
					TemplateId = requiredItem.id,
					Count = requiredItem.count,
					Equipped = false
				};
				Item newItem = Item.MakeItem(itemInfo);
				int remainCount = _storageIO.AddToOutput(newItem);
				if (remainCount > 0)
				{
					itemInfo.Count = remainCount;
					_pendingItems.Add(itemInfo);
				}
			}
			if (_pendingItems.Count == 0)
				CurRefinementId = 0;
		}

		// 연료 태우기
		if (_combustionEndTime < Managers.Time.CurTime)
		{
			Item item = _storageIO.GetItemBySlot(_storageIO.FuelStartSlot);
			CraftingMaterial cm = item as CraftingMaterial;
			if (cm != null && cm.Fuelable)
			{
				_combustionStartTime = Managers.Time.CurTime;
				_combustionEndTime = _combustionStartTime + cm.FuelEfficiency;
				_storageIO.RemoveFromSlot(_storageIO.FuelStartSlot, 1);
			}
		}
	}

	float _delay;
	void UpdatePending()
	{
		if (_pendingItems.Count == 0)
		{
			CurRefinementId = 0;
			if (NextRefinementId == 0)
				State = WorkState.Idle;
			else
				State = WorkState.Work;
			return;
		}
		if (_delay > 0.0f)
		{
			_delay -= Time.unscaledDeltaTime;
			return;
		}

		for (int i = _pendingItems.Count - 1; i >= 0; i--)
		{
			Item item = Item.MakeItem(_pendingItems[i]);
			int remainCount = _storageIO.AddToOutput(item);
			if (remainCount == 0)
				_pendingItems.RemoveAt(i);
			else
			{
				_pendingItems[i].Count = remainCount;
				_delay = 0.25f;
			}
		}
	}

	public bool ReserveCrafting(int refinementId)
	{
		if (!CanCraft(refinementId))
			return false;

		NextRefinementId = refinementId;
		return true;
	}

	public bool CanCraft(int refinementId)
	{
		RefinementData refinementData = null;
		Managers.Data.RefinementDict.TryGetValue(refinementId, out refinementData);

		return CanCraft(refinementData);
	}

	public bool CanCraft(RefinementData refinementData)
	{
		bool possible = true;
		foreach (RequiredItem requiredItem in refinementData.inputItems)
		{
			int count = _storageIO.GetInputItemCount(requiredItem.id);
			if (count < requiredItem.count)
				possible = false;
		}

		return possible;
	}

	bool IsWorkable()
	{
		if (_combustionEndTime < Managers.Time.CurTime)
		{
			Item item = _storageIO.GetItemBySlot(_storageIO.FuelStartSlot);
			CraftingMaterial cm = item as CraftingMaterial;
			if (cm == null || cm.Fuelable == false)
				return false;
		}
		return true;
	}

	public void Cancel(int refinementId)
	{
		if (refinementId == 0 || (CurRefinementId != refinementId && NextRefinementId != refinementId))
			return;

		if (refinementId == CurRefinementId)
		{
			if (_pendingItems.Count > 0)
				return;
			CurRefinementId = 0;

			RefinementData refinementData = null;
			Managers.Data.RefinementDict.TryGetValue(refinementId, out refinementData);

			foreach (RequiredItem requiredItem in refinementData.inputItems)
			{
				ItemInfo itemInfo = new ItemInfo()
				{
					TemplateId = requiredItem.id,
					Count = requiredItem.count,
					Equipped = false
				};
				Item newItem = Item.MakeItem(itemInfo);
				int remainCount = _storageIO.AddToInput(newItem);
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
		if (refinementId == NextRefinementId)
		{
			NextRefinementId = 0;
		}
	}
}
