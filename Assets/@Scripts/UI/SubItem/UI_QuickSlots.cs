using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class UI_QuickSlots : UI_Base
{
	[SerializeField]
	UI_ItemInfo _itemInfoUI;

	public List<UI_QuickSlot_Item> Items { get; } = new List<UI_QuickSlot_Item>();

    public override void Init()
    {
        Items.Clear();

		GameObject grid = Util.FindChild(gameObject, "ItemGrid", true);
		foreach (Transform child in grid.transform)
			Destroy(child.gameObject);

        for (int i = 0; i < Managers.Inven.QuickSlotCount; i++)
		{
            UI_QuickSlot_Item item = Managers.UI.MakeSubItem<UI_QuickSlot_Item>(grid.transform);
            Items.Add(item);
        }

		RefreshUI();
    }

    public void RefreshUI()
    {
        if (Items.Count == 0)
            return;

        //List<Item> items = Managers.Inven.Items.Values.ToList();
        //items.Sort((left, right) => { return left.Slot - right.Slot; });

        for (int i = 0; i < Managers.Inven.QuickSlotCount; i++)
        {
            Items[i].SetItem(Managers.Inven.GetItemBySlot(i), i, i + 1);
        }
	}

	public void ShowItemInfo(int templateId)
	{
		if (templateId == 0)
		{
			_itemInfoUI.gameObject.SetActive(false);
			return;
		}

		_itemInfoUI.gameObject.SetActive(true);
		_itemInfoUI.SetItem(templateId);
	}

	public void CloseItemInfo(int templateId)
	{
		_itemInfoUI.SetItem(0);
		_itemInfoUI.gameObject.SetActive(false);
	}

	void OnEnable()
	{
		_itemInfoUI.SetItem(0);
		_itemInfoUI.gameObject.SetActive(false);
	}

	void OnDisable()
	{
		_itemInfoUI.SetItem(0);
		_itemInfoUI.gameObject.SetActive(false);
	}
}
