using Data;
using NUnit.Framework.Interfaces;
using System;
using UnityEngine;
using static Define;

public class ItemInfo
{
    public int TemplateId { get; set; }
    public int Slot { get; set; }
    public int Count { get; set; }
    public int Durability { get; set; }
    public int MaxDurability { get; set; }
    public bool Equipped { get; set; }
}

public class Item
{
    public ItemInfo Info { get; } = new ItemInfo();

    public int TemplateId
    {
        get { return Info.TemplateId; }
        set { Info.TemplateId = value; }
    }

    public int Slot
    {
        get { return Info.Slot; }
        set { Info.Slot = value; }
    }

    public int Count
    {
        get { return Info.Count; }
        set { Info.Count = value; }
    }

    public int Durability
    {
        get { return Info.Durability; }
        set { Info.Durability = value; }
    }

    public int MaxDurability
    {
        get { return Info.MaxDurability; }
        set { Info.MaxDurability = value; }
    }

    public bool Equipped
    {
        get { return Info.Equipped; }
        set { Info.Equipped = value; }
    }

    public ItemType ItemType { get; private set; }
    public bool Stackable { get; protected set; }

    public Item(ItemType itemType)
    {
        ItemType = itemType;
    }

    public static Item MakeItem(ItemInfo itemInfo)
    {
        Item item = null;

        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(itemInfo.TemplateId, out itemData);

        if (itemData == null)
            return null;

        switch (itemData.itemType)
        {
            case ItemType.Equipment:
                item = new Equipment(itemInfo.TemplateId);
                break;
            case ItemType.Consumable:
                item = new Consumable(itemInfo.TemplateId);
                break;
            case ItemType.CraftingMaterial:
                item = new CraftingMaterial(itemInfo.TemplateId);
                break;
			case ItemType.SpecialItem:
				item = new SpecialItem(itemInfo.TemplateId);
				break;
		}

        if (item != null)
        {
            item.Count = itemInfo.Count;
            item.Slot = itemInfo.Slot;
            item.Durability = (itemInfo.Durability == 0) ? item.MaxDurability : itemInfo.Durability;
            item.Equipped = itemInfo.Equipped;
        }

        return item;
    }

    public virtual void DecreaseDurability(int amount)
    {
        Durability -= amount;
        if (Durability <= 0)
        {
            Durability = 0;
            Managers.Inven.RemoveFromSlot(Slot, 1);
            Managers.Sound.Play("UISOUND/DestroyWeapon");
        }
        else
            Managers.Inven.RefreshUI();
	}
}

public class Equipment : Item
{
    public EquipmentType EquipmentType { get; private set; }
	public WeaponType WeaponType { get; private set; }
    public ArmorType ArmorType { get; private set; }
	public int Damage { get; private set; }
	public int Defence { get; private set; }
	public string PrefabPath { get; private set; }
    public int QSkillId { get; private set; }
	public int ESkillId { get; private set; }

	public Equipment(int templateId) : base(ItemType.Equipment)
	{
		Init(templateId);
	}

	void Init(int templateId)
	{
		ItemData itemData = null;
		Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
		if (itemData.itemType != ItemType.Equipment)
			return;

		EquipmentData data = (EquipmentData)itemData;
		{
			TemplateId = data.id;
			Count = 1;
            EquipmentType = data.equipmentType;
			WeaponType = data.weaponType;
			ArmorType = data.armorType;
			Damage = data.damage;
			Defence = data.defence;
			MaxDurability = data.maxDurability;
			PrefabPath = data.prefabPath;
            QSkillId = data.qSkillId;
            ESkillId = data.eSkillId;
			Stackable = false;
		}
	}

	public override void DecreaseDurability(int amount)
	{
		base.DecreaseDurability(amount);

        if (Durability <= 0 && Equipped)
        {
            // sound here
            PlayerController player = Managers.Object.GetPlayer();
            if (EquipmentType == EquipmentType.Weapon)
				player.UnEquipWeapon();
		}
	}
}

//public class Weapon : Item
//{
//    public WeaponType WeaponType { get; private set; }
//    public int Damage { get; private set; }
//    public string PrefabPath { get; private set; }

//    public Weapon(int templateId) : base(ItemType.Weapon)
//    {
//        Init(templateId);
//    }

//    void Init(int templateId)
//    {
//        ItemData itemData = null;
//        Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
//        if (itemData.itemType != ItemType.Weapon)
//            return;

//        WeaponData data = (WeaponData)itemData;
//        {
//            TemplateId = data.id;
//            Count = 1;
//            WeaponType = data.weaponType;
//            Damage = data.damage;
//            MaxDurability = data.maxDurability;
//            PrefabPath = data.prefabPath;
//            Stackable = false;
//        }
//    }
//}

//public class Armor : Item
//{
//    public ArmorType ArmorType { get; private set; }
//    public int Defence { get; private set; }

//    public Armor(int templateId) : base(ItemType.Armor)
//    {
//        Init(templateId);
//    }

//    void Init(int templateId)
//    {
//        ItemData itemData = null;
//        Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
//        if (itemData.itemType != ItemType.Armor)
//            return;

//        ArmorData data = (ArmorData)itemData;
//        {
//            TemplateId = data.id;
//            Count = 1;
//            ArmorType = data.armorType;
//            Defence = data.defence;
//            MaxDurability = data.maxDurability;
//            Stackable = false;
//        }
//    }
//}

public class Consumable : Item
{
    public ConsumableType ConsumableType { get; private set; }
    public int MaxCount { get; private set; }

    public Consumable(int templateId) : base(ItemType.Consumable)
    {
        Init(templateId);
    }

    void Init(int templateId)
    {
        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
        if (itemData.itemType != ItemType.Consumable)
            return;

        ConsumableData data = (ConsumableData)itemData;
        {
            TemplateId = data.id;
			ConsumableType = data.consumableType;
			Count = 1;
            MaxCount = data.maxCount;
            Stackable = (data.maxCount > 1);
        }
    }
}

public class CraftingMaterial : Item
{
    public CraftingMaterialType CraftingMaterialType { get; private set; }
    public int MaxCount { get; private set; }
    public bool Fuelable { get; private set; }
	public float FuelEfficiency { get; private set; }

	public CraftingMaterial(int templateId) : base(ItemType.CraftingMaterial)
    {
        Init(templateId);
    }

    void Init(int templateId)
    {
        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
        if (itemData.itemType != ItemType.CraftingMaterial)
            return;

		CraftingMaterialData data = (CraftingMaterialData)itemData;
        {
            TemplateId = data.id;
			CraftingMaterialType = data.craftingMaterialType;
			Count = 1;
            MaxCount = data.maxCount;
            Fuelable = data.fuelable;
            FuelEfficiency = data.fuelEfficiency;
            Stackable = (data.maxCount > 1);
        }
    }
}

public class SpecialItem : Item
{
	public SpecialItemType SpecialItemType { get; private set; }
	public int MaxCount { get; private set; }

	public SpecialItem(int templateId) : base(ItemType.SpecialItem)
	{
		Init(templateId);
	}

	void Init(int templateId)
	{
		ItemData itemData = null;
		Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
		if (itemData.itemType != ItemType.SpecialItem)
			return;

		SpecialItemData data = (SpecialItemData)itemData;
		{
			TemplateId = data.id;
			SpecialItemType = data.specialItemType;
			Count = 1;
			MaxCount = data.maxCount;
			Stackable = (data.maxCount > 1);
		}
	}
}