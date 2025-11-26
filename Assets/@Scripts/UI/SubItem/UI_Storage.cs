using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Storage : UI_Base
{
	[SerializeField]
	protected UI_ItemInfo _itemInfoUI;
	[SerializeField]
	protected GameObject _getAllItemsButton;

	protected Storage _storage;
	public Storage StorageSystem { get { return _storage; } }

	public List<UI_Storage_Item> Items { get; } = new List<UI_Storage_Item>();

	public override void Init()
	{
		_itemInfoUI.gameObject.SetActive(false);

		if (_getAllItemsButton != null)
			_getAllItemsButton.BindEvent((data) => { if (_storage != null) _storage.PopAllItems(); });
	}

	public virtual void SetStorage(Storage storage)
	{
		_storage = storage;
		if (_storage == null)
			return;

		Items.Clear();
		
		GameObject grid = Util.FindChild(gameObject, "StorageGrid", true);
		foreach (Transform child in grid.transform)
			Destroy(child.gameObject);

		for (int i = 0; i < _storage.SlotCount; i++)
		{
			UI_Storage_Item item = Managers.UI.MakeSubItem<UI_Storage_Item>(grid.transform);
			Items.Add(item);
		}

		RefreshUI();
	}

	public virtual void RefreshUI()
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
