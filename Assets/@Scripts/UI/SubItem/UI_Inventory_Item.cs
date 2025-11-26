using Data;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_Inventory_Item : UI_Base
{
    [SerializeField]
    protected Image _icon = null;

    [SerializeField]
    protected Image _frame = null;

    [SerializeField]
    protected Text _countText = null;

	[SerializeField]
	protected Image _equipIcon = null;

	[SerializeField]
	protected Image _progressBar;

	public int TemplateId { get; private set; }
    public int Count { get; private set; }
    public int Slot { get; private set; }
    public bool Equipped { get; private set; }
    public bool Dropable { get; private set; }

    Toggle _toggle;

    UI_Inventory _inventoryUI;

    public override void Init()
    {
        gameObject.BindEvent(OnPointerEnter, UIEvent.PointerEnter);
		gameObject.BindEvent(OnPointerExit, UIEvent.PointerExit);
		gameObject.BindEvent(OnClick);
        gameObject.BindEvent(OnPointerUp, UIEvent.PointerUp);
        _toggle = GetComponent<Toggle>();
        _toggle.group = transform.parent.GetComponent<ToggleGroup>();
        _inventoryUI = GetComponentInParent<UI_Inventory>(true);

	}

    void Update()
    {

	}

    public void SetItem(Item item, int slot)
    {
        if (item == null)
        {
            TemplateId = 0;
            Count = 0;
            Slot = slot;
            Equipped = false;

            _icon.gameObject.SetActive(false);
            _countText.gameObject.SetActive(false);
			_frame.gameObject.SetActive(false);
			_equipIcon.gameObject.SetActive(false);

            gameObject.GetOrAddComponent<UI_EventHandler>().OnDragHandler = null;
		}
        else
        {
            TemplateId = item.TemplateId;
            Count = item.Count;
            Slot = item.Slot;
            Equipped = item.Equipped;

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

            Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
            _icon.sprite = icon;

            Dropable = itemData.dropable;

			_icon.gameObject.SetActive(true);
			_countText.text = Count.ToString();
            _countText.gameObject.SetActive(item.Stackable);
			_frame.gameObject.SetActive(Equipped);
			_equipIcon.gameObject.SetActive(Equipped);

			gameObject.BindEvent(OnDrag, UIEvent.Drag);
		}
    }

    public void SetProgressRatio(float ratio)
    {
        if (_progressBar == null)
            return;
        _progressBar.fillAmount = ratio;
    }

    public void UseItem()
    {
        Data.ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);
        if (itemData == null)
            return;

        // TODO
        if (itemData.itemType == ItemType.Consumable)
        {
            ConsumableData consumableData = itemData as ConsumableData;
            PlayerController player = Managers.Object.GetPlayer();
            player.Hp = Mathf.Min(player.Hp + consumableData.hp, player.MaxHp);
            player.Mental = Mathf.Min(player.Mental + consumableData.mental, player.MaxMental);
            player.Hunger = Mathf.Min(player.Hunger + consumableData.hunger, player.MaxHunger);
            Managers.Event.ItemEvents.OnUseItem(TemplateId);
            Managers.Inven.RemoveFromSlot(Slot, 1);
            // sound here
            Managers.Sound.Play("UISOUND/EatItem");
            return;
        }
        else if (itemData.detailedDescription != null && itemData.detailedDescription.Length > 0)
        {
            UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
            if (ui != null)
                ui.ShowDetailedDescriptionUI(TemplateId);
			Managers.Event.ItemEvents.OnUseItem(TemplateId);
		}
        else if (TemplateId == 600) // 별조각
        {
            PlayerController player = Managers.Object.GetPlayer();
            GameObject go = Managers.Resource.Instantiate("Item/RewardObject");
            go.transform.position = player.transform.position;
            go.GetComponent<RewardObject>().Init(TemplateId, 1);
            go.GetComponentInChildren<StarPiece>().Connect();

            Managers.Sound.Play("UISOUND/DropItem");
            Managers.Event.ItemEvents.OnUseItem(TemplateId);
            Managers.Inven.RemoveFromSlot(Slot, 1);
            return;
        }

        Managers.Inven.HandleEquipItem(Slot, !Equipped);
    }

    public virtual void OnPointerEnter(PointerEventData data)
	{
		_inventoryUI.ShowItemInfo(TemplateId);
	}

	public virtual void OnPointerExit(PointerEventData data)
	{
		_inventoryUI.CloseItemInfo(TemplateId);
	}

	public virtual void OnClick(PointerEventData data)
    {
        if (TemplateId == 0)
            return;

        if (data.clickCount == 2) 
            UseItem();
        else if (data.button == PointerEventData.InputButton.Right)
        {
            UseItem();
        }
        else if (data.button == PointerEventData.InputButton.Middle)
        {
            Managers.Inven.SplitItem(Slot);
		}


  //      _toggle.isOn = true;
		//UI_InventoryAndCrafting invenUI = GetComponentInParent<UI_InventoryAndCrafting>(true);
  //      if (invenUI != null)
  //          invenUI.ShowItemInfo(TemplateId);
    }

    public virtual void OnDrag(PointerEventData data)
    {
        if (TemplateId == 0)
            return;

        _icon.transform.position = data.position;
        Canvas canvas = _icon.GetOrAddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;
    }

    public virtual void OnPointerUp(PointerEventData data)
    {
        if (TemplateId == 0)
            return;

        _icon.transform.localPosition = Vector3.zero;
        _icon.GetOrAddComponent<Canvas>().overrideSorting = false;

        List<RaycastResult> rrList = new List<RaycastResult>();
        GraphicRaycaster gr = Managers.UI.SceneUI.GetComponent<GraphicRaycaster>();
        gr.Raycast(data, rrList);

        if (rrList.Count == 0 && Dropable)
        {
			PlayerController player = Managers.Object.GetPlayer();
			GameObject go = Managers.Resource.Instantiate("Item/RewardObject");
			go.transform.position = player.transform.position;
			go.GetComponent<RewardObject>().Init(TemplateId, Count);

			Managers.Sound.Play("UISOUND/DropItem");
			Managers.Inven.RemoveFromSlot(Slot, Count);
			return;
        }

        UI_Inventory_Item invenItem = rrList[0].gameObject.GetComponentInParent<UI_Inventory_Item>();
        if (invenItem != null)
        {
            Managers.Inven.SwapItems(Slot, invenItem.Slot);
        }
    }

    void OnDisable()
    {
        if (_toggle != null)
            _toggle.isOn = false;
    }
}
