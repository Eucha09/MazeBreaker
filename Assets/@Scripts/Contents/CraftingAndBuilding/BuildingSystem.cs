using Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BuildingSystem : MonoBehaviour
{
	public int TemplateId { get; private set; }

	PlayerController _pc;

	// building
	bool _isPreviewActivated = false;
	GameObject _goPreview;
	RaycastHit _hitInfo;
	int _layerMask = (1 << (int)Define.Layer.Ground);
	float _range = 100.0f;

	// demolition
	bool _isDemolitionMode = false;
	public bool IsDemolitionMode { get { return _isDemolitionMode; }
		set 
		{ 
			if (_hoveringObject != null)
			{
				_hoveringObject.SetOriginalMat();
				_hoveringObject = null;
			}
			_isDemolitionMode = value;
		}
	}
	StructureController _hoveringObject;
	int _mask = (1 << (int)Define.Layer.Obstacle) | (1 << (int)Define.Layer.Default);

	void Start()
    {
		_pc = GetComponent<PlayerController>();
    }

    void Update()
	{
		if (_isPreviewActivated)
		{
			if (_hoveringObject != null)
			{
				_hoveringObject.SetOriginalMat();
				_hoveringObject = null;
			}

			PreviewPositionUpdate();

			if (Managers.Input.IsPointerOverGameObject())
				return;
			if (Input.GetMouseButtonDown(0))
			{
				if (_isPreviewActivated && _goPreview.GetComponent<PreviewObject>().IsBuildable())
				{
					Managers.Sound.Play("UISound/SelectBuildItem");  //위치선택Sound

					_isPreviewActivated = false;
					_pc.MoveToBuildPosition(_goPreview.GetComponent<PreviewObject>());
				}
			}
		}
		else if (IsDemolitionMode)
		{
			// hovering
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Managers.Input.GetMousePosition());
			bool raycastHit = Physics.Raycast(ray, out hit, 100.0f, _mask);
			if (raycastHit && hit.collider.GetComponentInParent<StructureController>())
			{
				StructureController structure = hit.collider.GetComponentInParent<StructureController>();
				if (structure.StructureType != Define.StructureType.Building)
					return;
				if (_hoveringObject != null)
					_hoveringObject.SetOriginalMat();
				_hoveringObject = structure;
				structure.SetHighlightMat();
			}
			else if (_hoveringObject != null)
			{
				_hoveringObject.SetOriginalMat();
				_hoveringObject = null;
			}


			if (Managers.Input.IsPointerOverGameObject())
				return;
			if (Input.GetMouseButtonUp(1))
			{
				if (_hoveringObject != null)
				{
					ReturnRequiredItem(_hoveringObject.TemplateId);
					Managers.Sound.Play("UISound/BuildCancel");  //위치선택Sound
					Managers.Resource.Instantiate("Effects/Building/SmokeBomb", _hoveringObject.transform.position, _hoveringObject.transform.rotation);
					Managers.Object.Remove(_hoveringObject.gameObject);
					_hoveringObject = null;
				}
			}
		}
	}

	// TEMP
	public void OnBuild()
	{
		if (_isPreviewActivated)
		{
			if (Managers.Input.IsPointerOverGameObject())
				return;
			if (_isPreviewActivated && _goPreview.GetComponent<PreviewObject>().IsBuildable())
			{
				Managers.Sound.Play("UISound/SelectBuildItem");  //위치선택Sound

				_isPreviewActivated = false;
				_pc.MoveToBuildPosition(_goPreview.GetComponent<PreviewObject>());
			}
		}
	}

	// TEMP
	public void OnDemolition()
	{
		if (IsDemolitionMode)
		{
			if (Managers.Input.IsPointerOverGameObject())
				return;
			if (_hoveringObject != null)
			{
				ReturnRequiredItem(_hoveringObject.TemplateId);
				Managers.Sound.Play("UISound/BuildCancel");  //위치선택Sound
				Managers.Resource.Instantiate("Effects/Building/SmokeBomb", _hoveringObject.transform.position, _hoveringObject.transform.rotation);
				Managers.Object.Remove(_hoveringObject.gameObject);
				_hoveringObject = null;
			}
		}
	}

	public void BuildingObjectRotation(float angleY)
	{
		if (_isPreviewActivated)
		{
			_goPreview.transform.Rotate(0.0f, angleY, 0.0f);
		}
	}

	void PreviewPositionUpdate()
	{
		Ray ray = Camera.main.ScreenPointToRay(Managers.Input.GetMousePosition());
		if (Physics.Raycast(ray, out _hitInfo, _range, _layerMask))
		{
			if (_hitInfo.transform != null)
			{
				_goPreview.transform.position = _hitInfo.point;
			}
		}
	}

	public bool BuildPreview(int templateId)
	{
		if (TemplateId != 0)
			return false;
		if (_isPreviewActivated)
			return false;
		if (_pc.CurrentWeapon == null || _pc.CurrentWeapon.WeaponType != Define.WeaponType.Hammer)
			return false;

		BuildingData buildingData = null;
		Managers.Data.BuildingDict.TryGetValue(templateId, out buildingData);

		if (!CanBuild(buildingData))
			return false;

		TemplateId = templateId;
		foreach (RequiredItem requiredItem in buildingData.requiredItems)
			Managers.Inven.Remove(requiredItem.id, requiredItem.count);

		_goPreview = Managers.Resource.Instantiate(buildingData.prefabPath + "_Preview");
		_goPreview.GetComponent<PreviewObject>().SetInfo(templateId, this);
		_isPreviewActivated = true;

		return true;
	}

	public bool CanBuild(int templateId)
	{
		BuildingData buildingData = null;
		Managers.Data.BuildingDict.TryGetValue(templateId, out buildingData);

		return CanBuild(buildingData);
	}
	
	public bool CanBuild(BuildingData buildingData)
	{
		bool possible = true;
		foreach (RequiredItem requiredItem in buildingData.requiredItems)
		{
			int count = Managers.Inven.GetItemCount(requiredItem.id);
			if (count < requiredItem.count)
				possible = false;
		}

		return possible;
	}

	public void Build()
	{
		int templateId = TemplateId;
		TemplateId = 0;

		//Managers.Object.SpawnObject(templateId, _goPreview.transform.position, _goPreview.transform.rotation);
		BuildingObjectInfo buildingObject = new BuildingObjectInfo();
		buildingObject.SpawnPos = _goPreview.transform.position;
		buildingObject.Rotation = _goPreview.transform.rotation;
		buildingObject.Init(templateId);
		Managers.Map.ApplyEnter(buildingObject);
		Managers.Resource.Instantiate("Effects/Building/SmokeBomb", _goPreview.transform.position, _goPreview.transform.rotation);
        Managers.Resource.Destroy(_goPreview);
		_isPreviewActivated = false;
		_goPreview = null;

		GetComponent<PlayerController>().BuildingDone();
		Managers.Sound.Play("UISOUND/CraftSucceed");
		Managers.Event.BuildingEvents.OnBuildingCompleted(templateId);
	}

	public void Cancel()
	{
		if (TemplateId == 0)
			return;

		int templateId = TemplateId;
		TemplateId = 0;

		ReturnRequiredItem(templateId);

		Managers.Resource.Destroy(_goPreview);
		_goPreview = null;
		_isPreviewActivated = false;

		GetComponent<PlayerController>().BuildingDone();
	}

	void ReturnRequiredItem(int templateId)
	{
		BuildingData buildingData = null;
		Managers.Data.BuildingDict.TryGetValue(templateId, out buildingData);

		foreach (RequiredItem requiredItem in buildingData.requiredItems)
		{
			ItemInfo itemInfo = new ItemInfo()
			{
				TemplateId = requiredItem.id,
				Count = requiredItem.count,
				Equipped = false
			};
			Item newItem = Item.MakeItem(itemInfo);
			Managers.Inven.Add(newItem);
		}
	}
}
