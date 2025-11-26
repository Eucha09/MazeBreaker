using System.Collections.Generic;
using UnityEngine;
using static Define;

public class UI_HostilityMeters : UI_Base
{
	Dictionary<DetectorType, UI_HostilityMeters_Item> _items = new();

	public override void Init()
	{
		_items.Clear();
		foreach (Transform child in transform)
			Destroy(child.gameObject);

		foreach (DetectorType type in System.Enum.GetValues(typeof(DetectorType)))
		{
			if (type == DetectorType.None)
				continue;
			var item = Managers.UI.MakeSubItem<UI_HostilityMeters_Item>(transform);
			_items[type] = item;
			item.gameObject.SetActive(false);
		}
	}

	public void UpdateHostilityMeters(HostilityMeter hostilitymeter)
	{
		if (_items.TryGetValue(hostilitymeter.Type, out var item))
			item.SetHostilityMeter(hostilitymeter);
	}
}
