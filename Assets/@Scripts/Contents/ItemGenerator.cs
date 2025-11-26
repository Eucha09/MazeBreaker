using Data;
using UnityEngine;

public class ItemGenerator : Interactable
{
	[SerializeField]
	int _itemId;
	[SerializeField]
	int _count;

	public int ItemId
	{
		get { return _itemId; }
		set
		{
			_itemId = value;
			ItemData itemData = null;
			Managers.Data.ItemDict.TryGetValue(_itemId, out itemData);
			if (itemData != null)
				_name = itemData.name;
			_update = true;
		}
	}
	public int Count { get { return _count; } set { _count = value; } }

	void Start()
	{
		Init(_itemId, _count);
	}

	public void Init(int itemId, int count)
	{
		_itemId = itemId;
		_count = count;

		ItemData itemData = null;
		Managers.Data.ItemDict.TryGetValue(_itemId, out itemData);
		if (itemData != null)
		{
			_name = itemData.name;
			GameObject go = Managers.Resource.Instantiate(itemData.prefabPath, transform);
			go.transform.Rotate(new Vector3(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
		}
	}

	public override void Interact()
	{
		Managers.Sound.Play("FarmingSound/PickupItem");
		ItemInfo itemInfo = new ItemInfo()
		{
			TemplateId = _itemId,
			Count = _count,
			Equipped = false
		};

		Item newItem = Item.MakeItem(itemInfo);
		Managers.Inven.Add(newItem);
	}
}
