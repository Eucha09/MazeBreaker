using System.Collections.Generic;
using UnityEngine;

public class UI_StorageIO : UI_Storage
{

	public override void SetStorage(Storage storage)
	{
		_storage = storage;

		Items.Clear();

		GameObject inputItemGrid = Util.FindChild(gameObject, "InputItemGrid");
		GameObject outputItemGrid = Util.FindChild(gameObject, "OutputItemGrid");
		GameObject fuelItemGrid = Util.FindChild(gameObject, "FuelItemGrid");

		if (inputItemGrid != null)
			foreach (Transform child in inputItemGrid.transform)
				Items.Add(child.GetComponent<UI_Storage_Item>());
		if (fuelItemGrid != null)
			foreach (Transform child in fuelItemGrid.transform)
				Items.Add(child.GetComponent<UI_Storage_Item>());
		if (outputItemGrid != null)
			foreach (Transform child in outputItemGrid.transform)
				Items.Add(child.GetComponent<UI_Storage_Item>());

		RefreshUI();
	}

	public override void RefreshUI()
	{
		if (gameObject.activeInHierarchy == false)
			return;
		if (Items.Count == 0)
			return;

		for (int i = 0; i < _storage.SlotCount; i++)
		{
			int slot = _storage.StartSlot + i;
			Items[i].SetItem(Managers.Inven.GetItemBySlot(slot), slot);
		}
	}
}
