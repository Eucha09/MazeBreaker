using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class UI_InventoryAndCrafting : UI_Base
{
    enum Buttons
    { 
        InventoryButton,
        CraftingButton,
        //BuildingButton,
    }

    [SerializeField]
    UI_Inventory _inventoryUI;
    [SerializeField]
    UI_Crafting _craftingUI;
    [SerializeField]
    UI_Building _buildingUI;

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        GetButton((int)Buttons.InventoryButton).gameObject.BindEvent((evt) => { OnClickButton(Buttons.InventoryButton); });
        GetButton((int)Buttons.CraftingButton).gameObject.BindEvent((evt) => { OnClickButton(Buttons.CraftingButton); });
        //GetButton((int)Buttons.BuildingButton).gameObject.BindEvent((evt) => { OnClickButton(Buttons.BuildingButton); });

        //PlayerController player = Managers.Object.GetPlayer();
		//_craftingUI.SetCraftSystem(Managers.Object.GetPlayer().GetComponent<CraftSystem>());
		//_buildingUI.SetBuildingSystem(player.GetComponent<BuildingSystem>());
		OnClickButton(Buttons.InventoryButton);
    }

	public void SetCraftSystem(CraftSystem craftSystem)
    {
		_craftingUI.SetCraftSystem(craftSystem);
	}

	void OnClickButton(Buttons button)
    {
        _inventoryUI.gameObject.SetActive(false);
        _craftingUI.gameObject.SetActive(false);
        //_buildingUI.gameObject.SetActive(false);

        Util.FindChild(GetButton((int)Buttons.InventoryButton).gameObject, "SelectedButton").SetActive(false);
        Util.FindChild(GetButton((int)Buttons.CraftingButton).gameObject, "SelectedButton").SetActive(false);
        //Util.FindChild(GetButton((int)Buttons.BuildingButton).gameObject, "SelectedButton").SetActive(false);

        if (button == Buttons.InventoryButton)
        {
            _inventoryUI.gameObject.SetActive(true);
            Util.FindChild(GetButton((int)Buttons.InventoryButton).gameObject, "SelectedButton").SetActive(true);
        }
        else if (button == Buttons.CraftingButton)
        {
            _craftingUI.gameObject.SetActive(true);
            Util.FindChild(GetButton((int)Buttons.CraftingButton).gameObject, "SelectedButton").SetActive(true);
        }
        //else if (button == Buttons.BuildingButton)
        //{
        //    _buildingUI.gameObject.SetActive(true);
        //    Util.FindChild(GetButton((int)Buttons.BuildingButton).gameObject, "SelectedButton").SetActive(true);
        //}
    }

	public void RefreshUI()
    {
        _inventoryUI.RefreshUI();
        _craftingUI.RefreshUI();
        //_buildingUI.RefreshUI();
    }
}
