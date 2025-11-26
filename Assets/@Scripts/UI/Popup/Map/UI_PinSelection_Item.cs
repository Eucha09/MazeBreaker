using Data;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_PinSelection_Item : UI_Base
{
	[SerializeField]
	protected Image _icon = null;

	[SerializeField]
	protected Image _frame = null;

	public int TemplateId { get; private set; }
	public Sprite IconSprite { get { return _icon.sprite; } }
	public Color IconColor { get { return _icon.color; } }
	int _idx;
	UI_Map_Node _node;

	public override void Init()
	{
		gameObject.BindEvent(OnPointerUp, UIEvent.PointerUp);
	}

	void Update()
	{
		if (_node == null || _node.TemplateId != TemplateId)
			_frame.gameObject.SetActive(false);
		else
			_frame.gameObject.SetActive(true);
	}

	public void SetPinData(MinimapMarkerData data)
	{
		TemplateId = data.id;
		_icon.sprite = Managers.Resource.Load<Sprite>(data.iconPath);
	}

	public void SetNode(int idx, UI_Map_Node node)
	{
		_idx = idx;
		_node = node;
	}

	public void OnPointerUp(PointerEventData data)
	{
		if (_node == null)
			return;

		GetComponentInParent<UI_PinSelection>().SelectPinType(_idx);
	}
}
