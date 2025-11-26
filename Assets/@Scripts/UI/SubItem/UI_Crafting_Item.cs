using System.Collections.Generic;
using UnityEngine;
using static Define;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Data;

public class UI_Crafting_Item : UI_Base
{
    [SerializeField]
    Image _icon = null;

    //[SerializeField]
    //Image _frame = null;

    [SerializeField]
    Text _itemName = null;

	[SerializeField]
	Image _progressBar;

    public int TemplateId { get; private set; }
    public Define.ItemType Type { get; private set; }
    public List<RequiredItem> RequiredItem { get; private set; } = new List<RequiredItem>();

    Toggle _toggle;

    UI_Crafting _craftingUI;

    public override void Init()
	{
		gameObject.BindEvent(OnPointerEnter, UIEvent.PointerEnter);
		gameObject.BindEvent(OnPointerExit, UIEvent.PointerExit);
		gameObject.BindEvent(OnClick);
        _toggle = GetComponent<Toggle>();
        _toggle.group = transform.parent.GetComponent<ToggleGroup>();
        _craftingUI = GetComponentInParent<UI_Crafting>(true);

	}

    void Update()
    {

        if (_craftingUI.CraftSystem.CraftingItemId == TemplateId)
            _progressBar.fillAmount = _craftingUI.CraftSystem.ProgressRatio;
        else
            _progressBar.fillAmount = 0.0f;

	}

    public void SetItem(int itemId)
    {
        if (itemId == 0)
        {
            TemplateId = 0;
            Type = ItemType.None;

            _icon.gameObject.SetActive(false);
            //_frame.gameObject.SetActive(false);
            _itemName.gameObject.SetActive(false);
        }
        else
        {
            TemplateId = itemId;

            Data.CraftingData craftingData = null;
            Managers.Data.CraftingDict.TryGetValue(TemplateId, out craftingData);
            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

            Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
            _icon.sprite = icon;
            _itemName.text = itemData.name;

            _icon.gameObject.SetActive(true);
            //_frame.gameObject.SetActive(Equipped);
            //_countText.gameObject.SetActive(item.Stackable);

            Type = itemData.itemType;
        }
	}

	public void OnPointerEnter(PointerEventData data)
	{
		_craftingUI.ShowCraftingInfo(TemplateId);
	}

	public void OnPointerExit(PointerEventData data)
	{
		_craftingUI.CloseCraftingInfo(TemplateId);
	}

	public void OnClick(PointerEventData data)
    {
        if (TemplateId == 0)
            return;

        if (data.button == PointerEventData.InputButton.Left)
        {
            if (_craftingUI.CraftSystem.Craft(TemplateId))
				Managers.Sound.Play("UISOUND/CraftClick");
            else
				Managers.Sound.Play("UISOUND/CraftFail");
        }
        if (data.button == PointerEventData.InputButton.Right)
        {
            if (_craftingUI.CraftSystem.CraftingItemId == TemplateId)
                _craftingUI.CraftSystem.Cancel(TemplateId);
		}

    }

    void OnDisable()
    {
        if (_toggle != null)
            _toggle.isOn = false;
    }
}
