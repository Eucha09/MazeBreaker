using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class UI_Inventory : UI_Base
{
    [SerializeField]
    UI_ItemInfo _itemInfoUI;

    public List<UI_Inventory_Item> Items { get; } = new List<UI_Inventory_Item>();

    public override void Init()
    {
		_itemInfoUI.gameObject.SetActive(false);

		Items.Clear();

        GameObject grid = Util.FindChild(gameObject, "ItemGrid", true);
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        for (int i = Managers.Inven.QuickSlotCount; i < Managers.Inven.SlotCount; i++)
        {
            UI_Inventory_Item item = Managers.UI.MakeSubItem<UI_Inventory_Item>(grid.transform);
            Items.Add(item);
        }


		Managers.Inven.Sort();
		RefreshUI();
	}

    public void RefreshUI()
    {
        if (Items.Count == 0)
            return;

        for (int i = Managers.Inven.QuickSlotCount; i < Managers.Inven.SlotCount; i++)
        {
            Item item = Managers.Inven.GetItemBySlot(i);
            Items[i - Managers.Inven.QuickSlotCount].SetItem(item, i);
            //if (item != null && _filteringType != Define.ItemType.None && item.ItemType != _filteringType)
            //    Items[i - Managers.Inven.QuickSlotCount].gameObject.SetActive(false);
            //else
            //    Items[i - Managers.Inven.QuickSlotCount].gameObject.SetActive(true);
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
