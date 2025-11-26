using Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class UI_Required_Item : UI_Base
{
    [SerializeField]
    Image _icon = null;

    [SerializeField]
    Text _countText = null;

	[SerializeField]
	Text _nameText = null;

    public int TemplateId { get; private set; }

    public override void Init()
    {
        
    }

    public void SetItem(RequiredItem requiredItem)
    {
        Data.ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(requiredItem.id, out itemData);

        TemplateId = requiredItem.id;
        int hasItemCount = Managers.Inven.GetItemCount(requiredItem.id);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _icon.sprite = icon;
        _countText.text = hasItemCount + "/" + requiredItem.count;
        _nameText.text = itemData.name;

        if (hasItemCount >= requiredItem.count)
        {
            _countText.color = Color.green;
			_nameText.color = Color.green;
		}
        else
		{
			_countText.color = Color.red;
			_nameText.color = Color.red;
		}
    }
}
