using UnityEngine;

public class RadarObject : Interactable
{
	void Start()
	{
		_name = "레이더";
	}

	public override void Interact()
	{
		ItemInfo itemInfo = new ItemInfo()
		{
			TemplateId = 602,
			Count = 1,
			Equipped = false
		};
		Item newItem = Item.MakeItem(itemInfo);
		Managers.Inven.Add(newItem, Managers.Inven.QuickSlotCount, Managers.Inven.SlotCount - 1);

		Managers.Quest.StartQuest(1002);
		Managers.Resource.Destroy(gameObject);
	}
}
