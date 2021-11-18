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

    public GameObject ui_Selector;
    private int curTarget;
    private bool canSwitchTarget;

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

    private void Start()
    {
        canSwitchTarget = true;
    }

    private void Update()
    {
        //Switching to Inventory Input
        if (GameController.instance.gameState == GameController.GameState.game)
        {
            if (Input.GetKey(KeyCode.I))
            {
                GameController.instance.gameState = GameController.GameState.inventory;
                curTarget = 0;
                SwitchSlotTarget();
                ui_Selector.SetActive(true);
            }
        }

        //Inventory Input
        else if (GameController.instance.gameState == GameController.GameState.inventory)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                GameController.instance.gameState = GameController.GameState.game;
                ui_Selector.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                ClearInventorySlot(curTarget);
            }

            NavigateInventory();
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

    //Clear target slot out completely
    private void ClearInventorySlot(int slotIndex)
    {
        inventorySlots[slotIndex].slotSprite = null;
        inventorySlots[slotIndex].slotName = null;
        inventorySlots[slotIndex].slotTxt = null;
        inventorySlots[slotIndex].slotQty = 0;
        inventorySlots[slotIndex].slotStack = 0;
        UpdateSlotUI(slotIndex);
    }

    //Update Slot UI, setting sprite and text
    void UpdateSlotUI(int slotIndex)
    {
        if(inventorySlots[slotIndex].slotName != null)
        {
            inventorySlotsUI[slotIndex].GetComponent<InventorySlotProps>().slotContents.SetActive(true);
            inventorySlotsUI[slotIndex].GetComponent<InventorySlotProps>().slotSprite.sprite = inventorySlots[slotIndex].slotSprite;
            inventorySlotsUI[slotIndex].GetComponent<InventorySlotProps>().slotQty.text = "X" + inventorySlots[slotIndex].slotQty.ToString();
        }
        else
        {
            inventorySlotsUI[slotIndex].GetComponent<InventorySlotProps>().slotContents.SetActive(false);
            inventorySlotsUI[slotIndex].GetComponent<InventorySlotProps>().slotSprite.sprite = null;
            inventorySlotsUI[slotIndex].GetComponent<InventorySlotProps>().slotQty.text = null;
        }
        
    }

    //Navigate through the Inventory Slots
    private void NavigateInventory()
    {
        if (canSwitchTarget)
        {
            float dirX = Input.GetAxisRaw("Horizontal");

            //Navigate right
            if (dirX > 0)
            {
                if (curTarget + 1 > inventorySlots.Count - 1)
                {
                    curTarget = 0;
                }
                else
                {
                    curTarget += 1;
                }

                SwitchSlotTarget();

            }

            //Navigate right
            else if (dirX < 0)
            {
                if (curTarget - 1 < 0)
                {
                    curTarget = inventorySlots.Count - 1;
                }
                else
                {
                    curTarget -= 1;
                }

                SwitchSlotTarget();
            }
        }        
    }

    void SwitchSlotTarget()
    {
        ui_Selector.transform.position = inventorySlotsUI[curTarget].transform.position;        
        canSwitchTarget = false;
        StartCoroutine("SwitchTargetReset");
    }

    //Stops the shipwreck selector spamming through all available wrecks
    IEnumerator SwitchTargetReset()
    {
        yield return new WaitForSeconds(0.25f);
        canSwitchTarget = true;
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