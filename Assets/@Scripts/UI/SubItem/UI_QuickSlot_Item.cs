using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_QuickSlot_Item : UI_Inventory_Item
{
    string[] _gamepadKeyGuide = {
        "",
		"<style=xbox_lt>+<style=xbox_a>",
		"<style=xbox_lt>+<style=xbox_b>",
		"<style=xbox_lt>+<style=xbox_x>",
		"<style=xbox_lt>+<style=xbox_y>",
		"<style=xbox_rt>+<style=xbox_a>",
		"<style=xbox_rt>+<style=xbox_b>",
		"<style=xbox_rt>+<style=xbox_x>",
		"<style=xbox_rt>+<style=xbox_y>",
        ""
	};

    [SerializeField]
    Text _quickSlotNumber = null;
	[SerializeField]
	TextMeshProUGUI _gamepadKeyGuideText = null;

	[SerializeField]
    GameObject _durabilityUI = null;
    [SerializeField]
    Image _durabilityBar = null;

    public int Durability { get; private set; }
    public int MaxDurability { get; private set; }

    UI_QuickSlots _quickSlotsUI;

	public override void Init()
	{
		base.Init();
        _quickSlotsUI = GetComponentInParent<UI_QuickSlots>();
	}

	void Update()
    {
        
    }

    public void SetItem(Item item, int slot, int quickSlotNumber)
    {
        base.SetItem(item, slot);

        if (item == null || item.Durability == 0)
        {
            //_frame.gameObject.SetActive(false);
            _durabilityUI.SetActive(false);
        }
        else
        {
            //_frame.gameObject.SetActive(Equipped);
            _durabilityUI.SetActive(true);
            _durabilityBar.fillAmount = (float)item.Durability / item.MaxDurability;
        }

        _quickSlotNumber.text = quickSlotNumber.ToString();
        _gamepadKeyGuideText.text = _gamepadKeyGuide[Mathf.Clamp(quickSlotNumber, 0, _gamepadKeyGuide.Length - 1)];
	}

	public override void OnPointerEnter(PointerEventData data)
	{
		_quickSlotsUI.ShowItemInfo(TemplateId);
	}

	public override void OnPointerExit(PointerEventData data)
	{
		_quickSlotsUI.CloseItemInfo(TemplateId);
	}
}
