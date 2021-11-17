using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryController : MonoBehaviour
{
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public List<GameObject> lootRefs;

    public List<GameObject> inventorySlotsUI;

    public static InventoryController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!InventoryController.instance)
        {
            InventoryController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    //Add item to next slot, or stack if space exists
    public void AddItem(GameObject loot, Sprite lootSprite, string lootName, string lootTxt, int lootStack)
    {
        int lootQty = 1;

        bool hasItem = false;

        //Loop through each items in the inventorySlots
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            //If item already exists in the inventorySlots
            if (inventorySlots[i].slotName == lootName && inventorySlots[i].slotQty < inventorySlots[i].slotStack)
            {
                //If the existing slot amount + the item amount is less than the stack maximum, add it to the existing stack
                if (inventorySlots[i].slotQty + lootQty <= inventorySlots[i].slotStack)
                {
                    inventorySlots[i].addAmount(lootQty);
                    UpdateSlotUI(i);
                    Destroy(loot);
                    hasItem = true;
                    break;
                }

                //Else get the difference, and add the difference to the existing stack, then if there is a spare slot, create a new stack with the remaining amount
                else
                {

                    int diff = inventorySlots[i].slotStack - inventorySlots[i].slotQty;
                    int amountRemaining = lootQty - diff;

                    inventorySlots[i].addAmount(diff);

                    //Get the next available null slot
                    int emptySlot = GetEmptySlot();

                    if (emptySlot >= 0)
                    {
                        inventorySlots[emptySlot].slotSprite = lootSprite;
                        inventorySlots[emptySlot].slotName = lootName;
                        inventorySlots[emptySlot].slotTxt = lootTxt;
                        inventorySlots[emptySlot].slotQty = amountRemaining;
                        inventorySlots[emptySlot].slotStack = lootStack;
                        UpdateSlotUI(emptySlot);
                        Destroy(loot);
                    }

                    //Else if there's no slots left, don't pick it up and set vacuumLoot to false
                    else
                    {
                        loot.GetComponent<LootProps>().vacuumLoot = false;
                    }

                    hasItem = true;
                    break;
                }
            }
        }

        //if the item doesn't exist in the invetory, find an empty slot and assign it there
        if (hasItem == false)
        {
            //Get the next available null slot
            int emptySlot = GetEmptySlot();
            if (emptySlot >= 0)
            {
                inventorySlots[emptySlot].slotSprite = lootSprite;
                inventorySlots[emptySlot].slotName = lootName;
                inventorySlots[emptySlot].slotTxt = lootTxt;
                inventorySlots[emptySlot].slotQty = lootQty;
                inventorySlots[emptySlot].slotStack = lootStack;
                UpdateSlotUI(emptySlot);
                Destroy(loot);
            }

            hasItem = true;
        }
    }

    //Drop target slot at current position
    public void DropItem()
    {

    }

    //Get the next available slot in inventoryinventorySlots
    int GetEmptySlot()
    {
        int slot = -1;

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (String.IsNullOrEmpty(inventorySlots[i].slotName))
            {
                slot = i;
                break;
            }
        }

        return slot;
    }

    //Clear each slot out completely
    private void ClearinventorySlots()
    {

    }

    //Update Slot UI, setting sprite and text
    void UpdateSlotUI(int slotIndex)
    {
        inventorySlotsUI[slotIndex].GetComponent<InventorySlotProps>().slotContents.SetActive(true);
        inventorySlotsUI[slotIndex].GetComponent<InventorySlotProps>().slotSprite.sprite = inventorySlots[slotIndex].slotSprite;
        inventorySlotsUI[slotIndex].GetComponent<InventorySlotProps>().slotQty.text = "X" + inventorySlots[slotIndex].slotQty.ToString();        
    }

}

[System.Serializable]
public class InventorySlot
{
    public Sprite slotSprite;
    public string slotName;
    public string slotTxt;
    public int slotQty;
    public int slotStack;

    public InventorySlot(Sprite _lootsprite, string _lootName, string _lootTxt, int _lootQty, int _lootStack)
    {
        slotSprite = _lootsprite;
        slotName = _lootName;
        slotTxt = _lootTxt;
        slotQty = _lootQty;
        slotStack = _lootStack;
    }

    public void addAmount(int value)
    {
        slotQty += value;
    }

    public void dropAmount(int value)
    {
        slotQty -= value;
    }
}