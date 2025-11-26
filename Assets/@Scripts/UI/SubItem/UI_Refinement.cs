using Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Refinement : UI_Base
{
	RefinementSystem _refinementSystem;
	public RefinementSystem RefinementSystem { get { return _refinementSystem; } }

	[SerializeField]
	Text _titleText;
	[SerializeField]
	UI_ItemInfo _itemInfoUI;
	UI_StorageIO _storageIOUI;
	[SerializeField]
	UI_Storage_Item _fuelSlot;

	public List<UI_Refinement_Item> RefinementItems { get; } = new List<UI_Refinement_Item>();

	bool _init;

	public override void Init()
	{
		_storageIOUI = GetComponent<UI_StorageIO>();
		_itemInfoUI.gameObject.SetActive(false);
		RefreshUI();
		_init = true;
	}

	void Update()
	{
		if (_fuelSlot != null)
			_fuelSlot.SetProgressRatio(_refinementSystem.FuelProgressRatio);
	}

	public void SetRefinementSystem(RefinementSystem refinementSystem)
	{
		_refinementSystem = refinementSystem;

		_titleText.text = _refinementSystem.Name;

		RefinementItems.Clear();

		GameObject grid = Util.FindChild(gameObject, "RefinementGrid", true);
		foreach (Transform child in grid.transform)
			Destroy(child.gameObject);

		foreach (RefinementData refinementData in Managers.Data.RefinementDict.Values)
		{
			if (refinementData.stationId == _refinementSystem.ObjectId)
			{
				UI_Refinement_Item refinementItem = Managers.UI.MakeSubItem<UI_Refinement_Item>(grid.transform);
				refinementItem.SetItem(refinementData.refinementId);
				RefinementItems.Add(refinementItem);
			}
		}
	}

	public void RefreshUI()
	{
		if (gameObject.activeInHierarchy == false)
			return;
		if (_init == false)
			return;

		if (_storageIOUI != null)
			_storageIOUI.RefreshUI();
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
