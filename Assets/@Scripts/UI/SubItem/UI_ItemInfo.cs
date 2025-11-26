using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class UI_ItemInfo : UI_Base
{
    [SerializeField]
    Image _itemIcon = null;

    [SerializeField]
    Text _itemName = null;

    [SerializeField]
    Text _itemDescription = null;

    public int TemplateId { get; private set; }

    public override void Init()
    {

    }

    public void SetItem(int templateId)
    {
        if (templateId == 0)
            return;

        TemplateId = templateId;

        Data.ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(templateId, out itemData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _itemIcon.sprite = icon;
        _itemName.text = itemData.name;
        _itemDescription.text = itemData.description;
    }
}
