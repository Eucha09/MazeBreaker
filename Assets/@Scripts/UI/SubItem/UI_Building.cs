using Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Building : UI_Base
{
	enum Images
	{
		Survival,
	}

	enum Texts
    {
        SelectedCategoryText,
    }

    BuildingSystem _buildingSystem;
    public BuildingSystem BuildingSystem { get { return _buildingSystem; } }

    [SerializeField]
    UI_BuildingInfo _buildingInfoUI;

    public List<UI_Building_Item> BuildingItems { get; } = new List<UI_Building_Item>();

    //[SerializeField]
    //Define.StructureType _filteringType = Define.StructureType.Survival;

    bool _init;

    public override void Init()
    {
        //Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
        //GetImage((int)Images.Survival).gameObject.BindEvent((evt) => { SetFiltering(Define.StructureType.Survival, Images.Survival); });

		_buildingInfoUI.gameObject.SetActive(false);
		//SetFiltering(Define.StructureType.Survival, Images.Survival);
		RefreshUI();
		_init = true;
    }

    public void SetBuildingSystem(BuildingSystem buildingSystem)
    {
        _buildingSystem = buildingSystem;

		BuildingItems.Clear();

		GameObject grid = Util.FindChild(gameObject, "BuildingGrid", true);
		foreach (Transform child in grid.transform)
			Destroy(child.gameObject);

		foreach (BuildingData buildingData in Managers.Data.BuildingDict.Values)
		{
			if (buildingData.locked == false || Managers.Game.UnLockedBuilding.Contains(buildingData.objectId))
			{
				UI_Building_Item buildingItem = Managers.UI.MakeSubItem<UI_Building_Item>(grid.transform);
				buildingItem.SetItem(buildingData.objectId);
				BuildingItems.Add(buildingItem);
			}
		}
	}

    void SetFiltering(Define.StructureType type, Images image)
    {
        //_filteringType = type;

        //Text categoryText = GetText((int)Texts.SelectedCategoryText);
        //categoryText.text = image.ToString();
        //Vector3 textPos = categoryText.transform.position;
        //textPos.x = GetImage((int)image).transform.position.x;
        //categoryText.transform.position = textPos;

        RefreshUI();
    }

    public void RefreshUI()
	{
		if (_init == false)
			return;

		//GetImage((int)Images.Survival).color = _filteringType == Define.StructureType.Survival ? Color.white : Color.gray;

        //foreach (UI_Building_Item item in BuildingItems)
        //{
        //    if (_filteringType == item.Type)
        //        item.gameObject.SetActive(true);
        //    else
        //        item.gameObject.SetActive(false);
        //}

        _buildingInfoUI.RefreshUI();
	}

	public void ShowBuildingInfo(int templateId)
	{
		if (templateId == 0)
		{
			_buildingInfoUI.gameObject.SetActive(false);
			return;
		}

		_buildingInfoUI.gameObject.SetActive(true);
		_buildingInfoUI.SetItem(templateId);
	}

	public void CloseBuildingInfo(int templateId)
	{
		_buildingInfoUI.SetItem(0);
		_buildingInfoUI.gameObject.SetActive(false);
	}

	void OnEnable()
	{
		_buildingInfoUI.SetItem(0);
		_buildingInfoUI.gameObject.SetActive(false);
	}

	void OnDisable()
	{
		_buildingInfoUI.SetItem(0);
		_buildingInfoUI.gameObject.SetActive(false);
	}
}
