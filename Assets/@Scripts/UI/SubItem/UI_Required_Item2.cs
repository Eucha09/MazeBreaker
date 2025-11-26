using Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_Required_Item2 : UI_Base
{
	[SerializeField]
	Image _icon = null;

	[SerializeField]
	Text _countText = null;

	public int TemplateId { get; private set; }

	UI_Refinement _refinementUI;

	public override void Init()
	{
		gameObject.BindEvent(OnPointerEnter, UIEvent.PointerEnter);
		gameObject.BindEvent(OnPointerExit, UIEvent.PointerExit);
		gameObject.BindEvent(OnClick, UIEvent.Click);
		_refinementUI = GetComponentInParent<UI_Refinement>(true);
	}

	public void SetItem(RequiredItem requiredItem)
	{
		Data.ItemData itemData = null;
		Managers.Data.ItemDict.TryGetValue(requiredItem.id, out itemData);

		TemplateId = requiredItem.id;

		Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
		_icon.sprite = icon;
		_countText.text = "x" + requiredItem.count;
	}

	public void OnPointerEnter(PointerEventData data)
	{
		_refinementUI.ShowItemInfo(TemplateId);
	}

	public void OnPointerExit(PointerEventData data)
	{
		_refinementUI.CloseItemInfo(TemplateId);
	}

	public void OnClick(PointerEventData data)
	{
		UI_Refinement_Item refinement_Item = GetComponentInParent<UI_Refinement_Item>();
		refinement_Item.OnClick(data);
	}
}
