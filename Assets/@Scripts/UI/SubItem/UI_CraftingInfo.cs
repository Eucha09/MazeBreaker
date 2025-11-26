using Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_CraftingInfo : UI_Base
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

    public override void Init()
    {

    }

    public void SetItem(int templateId)
    {
        if (templateId == 0)
            return;

        _templateId = templateId;

        Data.ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
        Data.CraftingData craftingData = null;
        Managers.Data.CraftingDict.TryGetValue(templateId, out craftingData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _itemIcon.sprite = icon;
        _itemName.text = itemData.name;
        _itemDescription.text = itemData.description;

        foreach (Transform child in _requiredItems.transform)
            Destroy(child.gameObject);
        foreach (RequiredItem requiredItem in craftingData.requiredItems)
        {
            Managers.UI.MakeSubItem<UI_Required_Item>(_requiredItems.transform).SetItem(requiredItem);
        }
    }

    public void RefreshUI()
    {
        if (_templateId == 0)
            return;

        Data.CraftingData craftingData = null;
        Managers.Data.CraftingDict.TryGetValue(_templateId, out craftingData);

        foreach (Transform child in _requiredItems.transform)
            Destroy(child.gameObject);
        foreach (RequiredItem requiredItem in craftingData.requiredItems)
        {
            Managers.UI.MakeSubItem<UI_Required_Item>(_requiredItems.transform).SetItem(requiredItem);
        }
    }
}
