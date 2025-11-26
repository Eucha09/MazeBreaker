using System.Collections.Generic;
using UnityEngine;
using static Define;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Data;

public class UI_Building_Item : UI_Base
{
    [SerializeField]
    Image _icon = null;

    //[SerializeField]
    //Image _frame = null;

    [SerializeField]
    Text _itemName = null;

    public int TemplateId { get; private set; }
    public Define.StructureType Type { get; private set; }
    public List<RequiredItem> RequiredItem { get; private set; } = new List<RequiredItem>();

    Toggle _toggle;

    UI_Building _buildingUI;

    public override void Init()
	{
		gameObject.BindEvent(OnPointerEnter, UIEvent.PointerEnter);
		gameObject.BindEvent(OnPointerExit, UIEvent.PointerExit);
		gameObject.BindEvent(OnClick);
        _toggle = GetComponent<Toggle>();
        _toggle.group = transform.parent.GetComponent<ToggleGroup>();
        _buildingUI = GetComponentInParent<UI_Building>(true);

	}

    void Update()
    {
        //_frame.gameObject.SetActive(_toggle.isOn);
    }

    public void SetItem(int objectId)
    {
        if (objectId == 0)
        {
            TemplateId = 0;
            Type = StructureType.None;

            _icon.gameObject.SetActive(false);
            _itemName.gameObject.SetActive(false);
        }
        else
        {
            TemplateId = objectId;

            Data.BuildingData buildingData = null;
            Managers.Data.BuildingDict.TryGetValue(TemplateId, out buildingData);
            Data.ObjectData objectData = null;
            Managers.Data.ObjectDict.TryGetValue(TemplateId, out objectData);

            Sprite icon = Managers.Resource.Load<Sprite>(objectData.iconPath);
            _icon.sprite = icon;
            _itemName.text = objectData.name;

            _icon.gameObject.SetActive(true);
            //_frame.gameObject.SetActive(Equipped);
            //_countText.gameObject.SetActive(item.Stackable);

            Type = (objectData as StructureData).structureType;
        }
	}

	public void OnPointerEnter(PointerEventData data)
	{
		_buildingUI.ShowBuildingInfo(TemplateId);
	}

	public void OnPointerExit(PointerEventData data)
	{
		_buildingUI.CloseBuildingInfo(TemplateId);
	}

	public void OnClick(PointerEventData data)
    {
        if (TemplateId == 0)
            return;

		if (data.button == PointerEventData.InputButton.Left)
		{
			if (_buildingUI.BuildingSystem.BuildPreview(TemplateId))
				Managers.Sound.Play("UISOUND/SelectBuildItem");
			else
				Managers.Sound.Play("UISOUND/CraftFail");
		}

    }

    void OnDisable()
    {
        if (_toggle != null)
            _toggle.isOn = false;
    }
}
