using Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Crafting : UI_Base
{
    CraftSystem _craftSystem;
    public CraftSystem CraftSystem { get { return _craftSystem; } }

	[SerializeField]
	Text _titleText;
    [SerializeField]
    UI_CraftingInfo _craftingInfoUI;
	UI_Storage _storageUI;

	public List<UI_Crafting_Item> CraftingItems { get; } = new List<UI_Crafting_Item>();

    bool _init;

    public override void Init()
    {
		_craftingInfoUI.gameObject.SetActive(false);
		_storageUI = GetComponent<UI_Storage>();

		RefreshUI();
		_init = true;
	}

    public void SetCraftSystem(CraftSystem craftSystem)
    {
        _craftSystem = craftSystem;

		if (!_craftSystem.IsPlayer)
			_titleText.text = _craftSystem.Name;

		CraftingItems.Clear();

		GameObject grid = Util.FindChild(gameObject, "CraftingGrid", true);
		foreach (Transform child in grid.transform)
			Destroy(child.gameObject);

		foreach (CraftingData craftingData in Managers.Data.CraftingDict.Values)
		{
			if (craftingData.stationId == _craftSystem.ObjectId)
			{
				if (craftingData.locked == false || Managers.Game.UnLockedCrafting.Contains(craftingData.itemId))
				{
					UI_Crafting_Item craftingItem = Managers.UI.MakeSubItem<UI_Crafting_Item>(grid.transform);
					craftingItem.SetItem(craftingData.itemId);
					CraftingItems.Add(craftingItem);
				}
			}
		}
	}

    public void RefreshUI()
	{
		if (gameObject.activeInHierarchy == false)
			return;
		if (_init == false)
            return;


        _craftingInfoUI.RefreshUI();
		if (_storageUI != null)
			_storageUI.RefreshUI();
	}

	public void ShowCraftingInfo(int templateId)
	{
		if (templateId == 0)
		{
			_craftingInfoUI.gameObject.SetActive(false);
			return;
		}

		_craftingInfoUI.gameObject.SetActive(true);
		_craftingInfoUI.SetItem(templateId);
	}

	public void CloseCraftingInfo(int templateId)
	{
		_craftingInfoUI.SetItem(0);
		_craftingInfoUI.gameObject.SetActive(false);
	}

	void OnEnable()
	{
		_craftingInfoUI.SetItem(0);
		_craftingInfoUI.gameObject.SetActive(false);
	}

	void OnDisable()
	{
		_craftingInfoUI.SetItem(0);
		_craftingInfoUI.gameObject.SetActive(false);
	}
}
