using Data;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_Storage_Item : UI_Inventory_Item
{
    UI_Storage _storageUI;

    public override void Init()
    {
        base.Init();

        _storageUI = GetComponentInParent<UI_Storage>();
	}

    void Update()
    {

	}

    public override void OnPointerEnter(PointerEventData data)
	{
		_storageUI.ShowItemInfo(TemplateId);
	}

	public override void OnPointerExit(PointerEventData data)
	{
		_storageUI.CloseItemInfo(TemplateId);
	}

	public override void OnClick(PointerEventData data)
    {
		if (TemplateId == 0)
			return;

		if (data.clickCount == 2)
			_storageUI.StorageSystem.PopItem(Slot);
		else if (data.button == PointerEventData.InputButton.Right)
			_storageUI.StorageSystem.PopItem(Slot);
		else if (data.button == PointerEventData.InputButton.Middle)
			Managers.Inven.SplitItem(Slot);
	}
}
