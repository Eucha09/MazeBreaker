using Player;
using ShaderCrew.SeeThroughShader;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections;

public class NoteBook : Interactable
{
	void Start()
    {
		_name = "경비병의 수첩";
	}

	public override void Interact()
	{
		ItemInfo itemInfo = new ItemInfo()
		{
			TemplateId = 601,
			Count = 1,
			Equipped = false
		};
		Item newItem = Item.MakeItem(itemInfo);
		Managers.Inven.Add(newItem, Managers.Inven.QuickSlotCount, Managers.Inven.SlotCount - 1);
		Managers.Quest.StartQuest(1003);
		Managers.Quest.StartQuest(1004);
		Managers.Resource.Destroy(gameObject);
	}
}
