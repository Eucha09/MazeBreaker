using Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_BuildingInfo : UI_Base
{
    [SerializeField]
    Image _itemIcon = null;

    [SerializeField]
    Text _itemName = null;

    [SerializeField]
    Text _itemDescription = null;

    [SerializeField]
    GameObject _requiredItems = null;

    int _templateId;
    public int TemplateId { get { return _templateId; } }

    bool _isPreviewActivated = false;
    GameObject _goPreview;

    // Raycast
    RaycastHit _hitInfo;
    int _layerMask = (1 << (int)Define.Layer.Ground);
    float _range = 100.0f;

    public override void Init()
    {
        //_craftButton.gameObject.BindEvent(OnCraftButtonClick);
    }

    // TODO
    // 입력처리 옮겨야 함
    void Update()
    {
        if (_isPreviewActivated)
            PreviewPositionUpdate();

        if (Input.GetKeyDown(KeyCode.Escape))
            Cancel();

        if (EventSystem.current.IsPointerOverGameObject())
            return;
        if (Input.GetMouseButtonDown(0))
            Build();
        if (Input.GetMouseButtonDown(1) && _isPreviewActivated)
        {
            _goPreview.transform.Rotate(0.0f, 90.0f, 0.0f);
        }
    }

    public void SetItem(int templateId)
    {
        if (templateId == 0)
            return;

        _templateId = templateId;

        Data.ObjectData objectData = null;
        Managers.Data.ObjectDict.TryGetValue(templateId, out objectData);
        Data.BuildingData buildingData = null;
        Managers.Data.BuildingDict.TryGetValue(templateId, out buildingData);

        Sprite icon = Managers.Resource.Load<Sprite>(objectData.iconPath);
        _itemIcon.sprite = icon;
        _itemName.text = objectData.name;
        _itemDescription.text = objectData.description;

        foreach (Transform child in _requiredItems.transform)
            Destroy(child.gameObject);
        foreach (RequiredItem requiredItem in buildingData.requiredItems)
        {
            Managers.UI.MakeSubItem<UI_Required_Item>(_requiredItems.transform).SetItem(requiredItem);
        }
    }

    public void RefreshUI()
    {
        if (_templateId == 0)
            return;

        Data.BuildingData buildingData = null;
        Managers.Data.BuildingDict.TryGetValue(_templateId, out buildingData);

        foreach (Transform child in _requiredItems.transform)
            Destroy(child.gameObject);
        foreach (RequiredItem requiredItem in buildingData.requiredItems)
        {
            Managers.UI.MakeSubItem<UI_Required_Item>(_requiredItems.transform).SetItem(requiredItem);
        }
    }

    public void OnCraftButtonClick(PointerEventData data)
    {
        if (_isPreviewActivated)
            return;

        Data.BuildingData buildingData = null;
        Managers.Data.BuildingDict.TryGetValue(_templateId, out buildingData);

        bool possible = true;
        foreach (RequiredItem requiredItem in buildingData.requiredItems)
        {
            int count = Managers.Inven.GetItemCount(requiredItem.id);
            if (count < requiredItem.count)
                possible = false;
        }

        if (!possible)  //재료 없어서 제작 못할 때
        {
            Managers.Sound.Play("UISOUND/CraftFail");
            return;
        }

        CraftPriview(buildingData.prefabPath);
    }

    void PreviewPositionUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out _hitInfo, _range, _layerMask))
        {
            if (_hitInfo.transform != null)
            {
                _goPreview.transform.position = _hitInfo.point;
            }
        }
    }

    void Build()
    {
        if (_isPreviewActivated && _goPreview.GetComponent<PreviewObject>().IsBuildable())
        {
            Data.BuildingData buildingData = null;
            Managers.Data.BuildingDict.TryGetValue(_templateId, out buildingData);

            foreach (RequiredItem requiredItem in buildingData.requiredItems)
                Managers.Inven.Remove(requiredItem.id, requiredItem.count);

            GameObject go = Managers.Resource.Instantiate(buildingData.prefabPath);
            go.transform.position = _goPreview.transform.position;
            go.transform.rotation = _goPreview.transform.rotation;
            Managers.Resource.Destroy(_goPreview);
            _isPreviewActivated = false;
            _goPreview = null;

            Managers.Sound.Play("UISOUND/CraftSucceed");
            //Managers.Map.UpdateNavMesh();
        }
    }

    void CraftPriview(string prefabPath)
    {
        _goPreview = Managers.Resource.Instantiate(prefabPath + "_Preview");
        _isPreviewActivated = true;
    }

    void Cancel()
    {
        if (_isPreviewActivated)
            Managers.Destroy(_goPreview);
        _goPreview = null;
        _isPreviewActivated = false;
    }

    void OnDisable()
    {
        Cancel();
    }
}
