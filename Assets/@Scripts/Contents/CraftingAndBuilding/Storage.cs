using Unity.VisualScripting;
using UnityEngine;

public class Storage : MonoBehaviour
{
    [SerializeField]
    protected int _slotCount = 6;
	protected int _startSlot;
	protected int _endSlot;

	public int SlotCount { get { return _slotCount; } }
	public int StartSlot { get { return _startSlot; } }
    public int EndSlot { get { return _endSlot; } }

	#region ObjectInfoBinder
	public BuildingObjectInfo Info { get; set; }

	public virtual void Bind(BuildingObjectInfo info)
	{
		Info = info;

		_slotCount = Info.SlotCount;
		_startSlot = Info.StartSlotNum;
		_endSlot = Info.EndSlotNum;
	}

	public void Refresh()
	{
		if (Info == null)
			return;
	}

	public virtual void Unbind()
	{
		Info = null;

		_slotCount = 0;
		_startSlot = 0;
		_endSlot = 0;
	}
	#endregion

	void Start()
    {
		if (Info == null && _slotCount > 0)
		{
			Managers.Inven.CreateStorage(_slotCount, out _startSlot, out _endSlot);
		}
	}

	public int GetItemCount(int templateId)
	{
		if (_slotCount == 0)
			return 0;
		return Managers.Inven.GetItemCount(templateId, _startSlot, _endSlot);
	}

	public Item GetItemBySlot(int slot)
	{
		if (_slotCount == 0)
			return null;
		if (slot < _startSlot || _endSlot < slot)
			return null;
		return Managers.Inven.GetItemBySlot(slot);
	}

    public int Add(Item item)
    {
		if (_slotCount == 0)
			return item.Count;
        return Managers.Inven.Add(item, _startSlot, _endSlot);
	}

	public void Remove(int templateId, int count)
	{
		if (_slotCount == 0)
			return;
		Managers.Inven.Remove(templateId, count, _startSlot, _endSlot);
	}

	public void RemoveFromSlot(int slot, int count)
	{
		if (_slotCount == 0)
			return;
		if (slot < _startSlot || _endSlot < slot)
			return;
		Managers.Inven.RemoveFromSlot(slot, count);
	}

	public void PopItem(int slot)
	{
		if (_slotCount == 0)
			return;
		Item item = Managers.Inven.GetItemBySlot(slot);
		if (item == null)
			return;

		ItemInfo itemInfo = new ItemInfo
		{
			TemplateId = item.TemplateId,
			Count = item.Count,
			Durability = item.Durability,
			Equipped = false
		};
		Item newItem = Item.MakeItem(itemInfo);
		int remainCnt = Managers.Inven.Add(newItem);
		Managers.Inven.RemoveFromSlot(slot, item.Count - remainCnt);
	}

	public void PopAllItems()
	{
		if (_slotCount == 0)
			return;
		for (int i = StartSlot; i <= EndSlot; i++)
		{
			PopItem(i);
		}
	}

	public void PushAllItems()
	{
		if (_slotCount == 0)
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
			int remainCnt = Managers.Inven.Add(newItem, StartSlot, EndSlot);
			Managers.Inven.RemoveFromSlot(i, curItem.Count - remainCnt);
		}
	}
}
