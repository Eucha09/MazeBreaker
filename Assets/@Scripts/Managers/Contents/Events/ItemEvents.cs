using System;
using UnityEngine;
using static Define;

public class ItemEvents
{
    public Action<int, int, int, int> GainItemAction;
    public void OnGainItem(int itemId, int count, int startSlot, int endSlot)
    {
        if (GainItemAction != null)
            GainItemAction(itemId, count, startSlot, endSlot);
	}

	public Action<int, int, int, int> RemoveItemAction;
	public void OnRemoveItem(int itemId, int count, int startSlot, int endSlot)
	{
		if (RemoveItemAction != null)
			RemoveItemAction(itemId, count, startSlot, endSlot);
	}

	public Action<int, EquipmentType, bool> EquipItemAction;
    public void OnEquipItem(int itemId, EquipmentType equipmentType, bool equipped)
    {
        if (EquipItemAction != null)
            EquipItemAction(itemId, equipmentType, equipped);
    }

    public Action<int> UseItemAction;
    public void OnUseItem(int itemId)
    {
        if (UseItemAction != null)
            UseItemAction(itemId);
    }

    public void Clear()
    {
        GainItemAction = null;
        EquipItemAction = null;
        UseItemAction = null;
    }
}
