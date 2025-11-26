using Data;
using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Linq;
using UnityEngine;
using static Define;
using static UnityEngine.Rendering.VolumeComponent;

public class InventoryManager
{
    public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();
    public int SlotCount { get; set; } = 38; // quickSlotCount + InvenSlotCount;
    public int QuickSlotCount { get; set; } = 8;
    int _storageSlotIndex = 1000; // TEMP

    public int Add(Item newItem)
    {
        return Add(newItem, 0, SlotCount - 1);
    }

    public int Add(Item newItem, int startSlot, int endSlot)
    {
        int gainCnt = 0;
        int restCnt = newItem.Count;

        if (newItem.Stackable)
        {
            foreach (Item item in Items.Values)
            {
                if (item.Slot < startSlot || endSlot < item.Slot)
                    continue;
                if (newItem.TemplateId != item.TemplateId)
                    continue;

                if (item.ItemType == Define.ItemType.Consumable)
                {
                    Consumable conItem = item as Consumable;
                    int count = Mathf.Min(newItem.Count, conItem.MaxCount - conItem.Count);
                    newItem.Count -= count;
                    conItem.Count += count;
                    gainCnt += count;
                }
                else if (item.ItemType == Define.ItemType.CraftingMaterial)
                {
					CraftingMaterial resItem = item as CraftingMaterial;
                    int count = Mathf.Min(newItem.Count, resItem.MaxCount - resItem.Count);
                    newItem.Count -= count;
                    resItem.Count += count;
                    gainCnt += count;
				}
				else if (item.ItemType == Define.ItemType.SpecialItem)
				{
					SpecialItem speItem = item as SpecialItem;
					int count = Mathf.Min(newItem.Count, speItem.MaxCount - speItem.Count);
					newItem.Count -= count;
					speItem.Count += count;
					gainCnt += count;
				}
			}
        }

        if (newItem.Count > 0)
        {
            int? slot = GetEmptySlot(startSlot, endSlot);
            if (slot != null)
            {
                newItem.Slot = (int)slot;
                Items.Add(newItem.Slot, newItem);
                gainCnt += newItem.Count;
            }
        }

        if (gainCnt > 0)
            Managers.Event.ItemEvents.OnGainItem(newItem.TemplateId, gainCnt, startSlot, endSlot);
        RefreshUI();
        return restCnt - gainCnt;
    }

	public void Remove(int templateId, int count)
    {
        Remove(templateId, count, 0, SlotCount - 1);
    }

	public void Remove(int templateId, int count, int startSlot, int endSlot)
    {
        int removeCnt = count;

        List<Item> items = new List<Item>();
        foreach (Item item in Items.Values)
            if (templateId == item.TemplateId && startSlot <= item.Slot && item.Slot <= endSlot)
                items.Add(item);

        items.Sort((a, b) => { return a.Count - b.Count; });

        foreach (Item item in items)
        {
            item.Count -= count;
            count = 0;
            if (item.Count <= 0)
            {
                count = -item.Count;
                if (item.Equipped)
                    HandleEquipItem(item.Slot, false);
				Items.Remove(item.Slot);
            }

            if (count == 0)
                break;
        }

        Managers.Event.ItemEvents.OnRemoveItem(templateId, removeCnt, startSlot, endSlot);

        RefreshUI();
    }

    public void RemoveFromSlot(int slot, int count)
    {
        Item item = GetItemBySlot(slot);
        if (item == null)
            return;

        int templateId = item.TemplateId;

        item.Count -= count;
        if (item.Count <= 0)
		{
			if (item.Equipped)
				HandleEquipItem(item.Slot, false);
			Items.Remove(item.Slot);
		}

		Managers.Event.ItemEvents.OnRemoveItem(templateId, count, slot, slot);

		RefreshUI();
    }

    public Item GetItemBySlot(int slot)
    {
        Item item = null;
        Items.TryGetValue(slot, out item);
        return item;
	}

	public int GetItemCount(int templateId)
	{
        return GetItemCount(templateId, 0, SlotCount - 1);
	}

	public int GetItemCount(int templateId, int startSlot, int endSlot)
    {
        int count = 0;
        foreach (Item item in Items.Values)
        {
            if (item.Slot < startSlot || endSlot < item.Slot)
                continue;
            if (templateId != item.TemplateId)
                continue;

            count += item.Count;
        }

        return count;
    }

    public int GetCountSlotsFilled()
    {
        return GetCountSlotsFilled(0, SlotCount - 1);
    }

    public int GetCountSlotsFilled(int startSlot, int endSlot)
    {
        int count = 0;

        for (int i = startSlot; i <= endSlot; i++)
        {
            if (Items.ContainsKey(i))
                count++;
		}
        return count;
    }

    public Item Find(Func<Item, bool> condition)
    {
        foreach (Item item in Items.Values)
        {
            if (condition.Invoke(item))
                return item;
        }
        return null;
    }

    public List<Item> FindAll(Func<Item, bool> condition)
    {
        List<Item> ret = new List<Item>();

        foreach (Item item in Items.Values)
        {
            if (condition.Invoke(item))
                ret.Add(item);
        }
        return ret;
    }

    public int? GetEmptySlot()
    {
        return GetEmptySlot(0, SlotCount - 1);
    }

    public int? GetEmptySlot(int curSlot)
    {
        return GetEmptySlot(0, SlotCount - 1, curSlot);
    }

    public int? GetEmptySlot(int startSlot, int endSlot)
	{
		for (int slot = startSlot; slot <= endSlot; slot++)
		{
			Item item = Items.Values.FirstOrDefault(i => i.Slot == slot);
			if (item == null)
				return slot;
		}
		return null;
    }

    public int? GetEmptySlot(int startSlot, int endSlot, int curSlot)
	{
        if (curSlot < startSlot || endSlot < curSlot)
            return null;

        for (int slot = curSlot; slot <= endSlot; slot++)
        {
            Item item = Items.Values.FirstOrDefault(i => i.Slot == slot);
            if (item == null)
                return slot;
		}
		for (int slot = startSlot; slot < curSlot; slot++)
		{
			Item item = Items.Values.FirstOrDefault(i => i.Slot == slot);
			if (item == null)
				return slot;
		}
		return null;
	}

    public void SplitItem(int slot)
    {
        if (slot < 0 || SlotCount <= slot)
            return;
        if (GetEmptySlot() == null)
            return;

        Item item = GetItemBySlot(slot);
        int halfCount = item.Count / 2;
        if (halfCount > 0)
        {
            item.Count -= halfCount;
			ItemInfo itemInfo = new ItemInfo
			{
				TemplateId = item.TemplateId,
				Count = halfCount,
				Equipped = false
			};
			Item newItem = Item.MakeItem(itemInfo);

			int? newSlot = GetEmptySlot(slot);
			if (newSlot != null)
			{
				newItem.Slot = (int)newSlot;
				Items.Add(newItem.Slot, newItem);
			}
		}

		RefreshUI();
	}

    public void SwapItems(int slot1, int slot2)
    {
        if (slot1 == slot2)
            return;

        Item item1 = null;
        Item item2 = null;
        Items.TryGetValue(slot1, out item1);
        Items.TryGetValue(slot2, out item2);

        Items.Remove(slot1);
        Items.Remove(slot2);

        if (item1 != null && item2 != null && item1.TemplateId == item2.TemplateId && item2.Stackable)
        {
			if (item1.ItemType == Define.ItemType.Consumable)
			{
				Consumable conItem = item2 as Consumable;
				int count = Mathf.Min(item1.Count, conItem.MaxCount - conItem.Count);
				item1.Count -= count;
				conItem.Count += count;
			}
			else if (item1.ItemType == Define.ItemType.CraftingMaterial)
			{
				CraftingMaterial resItem = item2 as CraftingMaterial;
				int count = Mathf.Min(item1.Count, resItem.MaxCount - resItem.Count);
				item1.Count -= count;
				resItem.Count += count;
			}
			else if (item1.ItemType == Define.ItemType.SpecialItem)
			{
				SpecialItem speItem = item2 as SpecialItem;
				int count = Mathf.Min(item1.Count, speItem.MaxCount - speItem.Count);
				item1.Count -= count;
				speItem.Count += count;
			}
			if (item1.Count > 0)
                Items.Add(item1.Slot, item1);
            if (item2.Count > 0)
                Items.Add(item2.Slot, item2);
        }
        else
		{
			if (item1 != null)
			{
				item1.Slot = slot2;
				Items.Add(item1.Slot, item1);
				if (item1.Slot >= SlotCount && item1.Equipped)
					HandleEquipItem(item1.Slot, false);
			}
			if (item2 != null)
			{
				item2.Slot = slot1;
				Items.Add(item2.Slot, item2);
				if (item2.Slot >= SlotCount && item2.Equipped)
					HandleEquipItem(item2.Slot, false);
			}
		}

        if (item1 != null && item1.Count > 0)
			Managers.Event.ItemEvents.OnGainItem(item1.TemplateId, item1.Count, item1.Slot, item1.Slot);
        if (item2 != null && item2.Count > 0)
			Managers.Event.ItemEvents.OnGainItem(item2.TemplateId, item2.Count, item2.Slot, item2.Slot);

		RefreshUI();
    }

    public void Sort()
    {
        List<Item> items = FindAll((i) => { return QuickSlotCount <= i.Slot && i.Slot < SlotCount; });
        foreach (Item item in items)
            Items.Remove(item.Slot);
        items.Sort((left, right) => { return left.TemplateId - right.TemplateId; });
        int slot = QuickSlotCount;
        foreach (Item item in items)
        {
            item.Slot = slot++;
            Items.Add(item.Slot, item);
        }
    }

    public void Clear()
    {
        //Items.Clear();
    }

    public void RefreshUI()
    {
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        if (gameSceneUI != null)
        {
            gameSceneUI.InvenCraftingUI.RefreshUI();
            gameSceneUI.QuickSlotsUI.RefreshUI();
            gameSceneUI.BuildingUI.RefreshUI();
            gameSceneUI.CraftingStationUI.RefreshUI();
			gameSceneUI.RefinementStationUI.RefreshUI();
            gameSceneUI.StorageUI.RefreshUI();
            gameSceneUI.RestingUI.RefreshUI();
		}
    }


    public void HandleEquipItem(int slot, bool equipped)
    {
        Item item = GetItemBySlot(slot);
        if (item == null)
            return;

		Equipment equipmentItem = item as Equipment;
		if (equipmentItem == null)
            return;

        // 착용 요청이라면, 겹치는 부위 해제
        if (equipped)
        {
            Item unequipItem = null;

            if (equipmentItem.EquipmentType == EquipmentType.Weapon)
            {
                unequipItem = Find(
                    i => i.Equipped && ((Equipment)i).EquipmentType == EquipmentType.Weapon);
            }
            else if (equipmentItem.EquipmentType == EquipmentType.Armor)
            {
                ArmorType armorType = equipmentItem.ArmorType;
                unequipItem = Find(
                    i => i.Equipped && ((Equipment)i).EquipmentType == EquipmentType.Armor
                        && ((Equipment)i).ArmorType == armorType);
            }

            if (unequipItem != null)
            {
                unequipItem.Equipped = false;
				Managers.Event.ItemEvents.OnEquipItem(unequipItem.TemplateId, ((Equipment)unequipItem).EquipmentType, false);
			}
        }

        equipmentItem.Equipped = equipped;

        // TODO
        PlayerController player = Managers.Object.GetPlayer();
        if (player != null)
        {
            if (equipmentItem.EquipmentType == EquipmentType.Weapon)
            {
                if (equipmentItem.Equipped)
                    player.ChangeWeapon(equipmentItem);
                else
                    player.UnEquipWeapon();
            }
        }

        Managers.Event.ItemEvents.OnEquipItem(equipmentItem.TemplateId, equipmentItem.EquipmentType, equipped);

        RefreshUI();
    }

    public void CreateStorage(int storageSize, out int startIndex, out int endIndex)
    {
        startIndex = _storageSlotIndex;
        endIndex = _storageSlotIndex + storageSize - 1;
        _storageSlotIndex += storageSize;
    }
}