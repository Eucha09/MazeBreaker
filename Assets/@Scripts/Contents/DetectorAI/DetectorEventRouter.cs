using Data;
using System;
using UnityEngine;
using static Define;

public class DetectorEventRouter : MonoBehaviour
{
    DetectorAI _detectorAI;
	PlayerController _player;

	void Start()
    {
        _detectorAI = GetComponent<DetectorAI>();
		_player = Managers.Object.GetPlayer();

		Managers.Event.ItemEvents.UseItemAction += OnUseItem;
		Managers.Event.PlayerEvents.AttackAction += OnAttack;
	}


    void Update()
	{
		float increaseSpeed = (1.2f - _player.Mental / _player.MaxMental) * 5.0f;

		foreach (DetectorType type in Enum.GetValues(typeof(DetectorType)))
		{
			if (type == DetectorType.None)
				continue;
			_detectorAI.IncreaseHostility(type, increaseSpeed * Time.deltaTime);
		}
	}

	void OnUseItem(int itemId)
	{
		if (!_detectorAI) return;

		if (Managers.Data.ItemDict.TryGetValue(itemId, out var itemData))
		{
			if (itemData is ConsumableData c && c.foodType == FoodType.Meat)
				_detectorAI.IncreaseHostility(DetectorType.Meat, 20);
		}
	}

	void OnAttack(BaseController target)
	{
		if (!_detectorAI) return;

		if (target.MaterialType == MaterialType.Stone)
			_detectorAI.IncreaseHostility(DetectorType.Stone, 20);
		else if (target.MaterialType == MaterialType.Tree)
			_detectorAI.IncreaseHostility(DetectorType.Wood, 20);
	}

}
