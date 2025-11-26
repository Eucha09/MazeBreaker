using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_ItemCollect : UI_Base
{
	Queue<Tuple<UI_ItemCollect_Item, float>> _queue = new Queue<Tuple<UI_ItemCollect_Item, float>>();

	public override void Init()
	{
		foreach (Transform child in transform)
			Destroy(child.gameObject);

		Managers.Event.ItemEvents.GainItemAction += (id, count, startSlot, endSlot) => 
		{
			if (endSlot <= Managers.Inven.SlotCount - 1)
				AddItem(id, count);
		};
	}

	void Update()
	{
		while (_queue.Count > 0 && (_queue.Count > 4 || _queue.Peek().Item2 + 2.0f < Time.time))
		{
			Tuple<UI_ItemCollect_Item, float> t = _queue.Dequeue();
			if (_queue.Count > 4)
				Managers.Resource.Destroy(t.Item1.gameObject);
			else
				t.Item1.CloseUI();
		}
	}

	void AddItem(int templateId, int count)
	{
		UI_ItemCollect_Item item = Managers.UI.MakeSubItem<UI_ItemCollect_Item>(gameObject.transform);
		item.SetItem(templateId, count);
		_queue.Enqueue(new Tuple<UI_ItemCollect_Item, float>(item, Time.time));
	}
}
