using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryController : MonoBehaviour
{
    //public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public GameObject sourceInventoryStatic;
    public GameObject targetInventoryStatic;
    public InventoryProps playerInventory;
    public GameObject targetInventory;
    public List<GameObject> lootRefs;

    [SerializeField] List<GameObject> curinventorySlotsUI;
    public List<GameObject> sourceInventorySlotsUI;
    public List<GameObject> targetInventorySlotsUI;

    public GameObject ui_Selector;
    public GameObject ui_SourceInventory;
    public GameObject ui_TargetInventory;
    public int curTarget;
    public int lootQty;

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
        curinventorySlotsUI = sourceInventorySlotsUI;
        playerInventory = PlayerController.instance.GetComponent<InventoryProps>();
    }

    private void Update()
    {
        //Switching to Inventory Input
        if (GameController.instance.gameState == GameController.GameState.game)
        {
            if (Input.GetKey(KeyCode.I))
            {
                OpenInventoryUI(false);
            }
        }

        //Inventory Input
        else if (GameController.instance.gameState == GameController.GameState.inventory)
        {
            //Quit Inventory and close the target inventory if it's open and reset the source inventory back to the player
            if (Input.GetKeyDown(KeyCode.Q))
            {
                GameController.instance.gameState = GameController.GameState.game;
                ui_TargetInventory.SetActive(false);
                ui_Selector.SetActive(false);
            }
        }
    }

    //Add item to next slot, or stack if space exists
    public void AddItem(GameObject loot, Sprite lootSprite, string lootName, string lootTxt, int lootStack, int lootValue)
    {
        int lootQty = 1;

        bool hasItem = false;

        //Loop through each items in the inventorySlots
        for (int i = 0; i < playerInventory.GetComponent<InventoryProps>().inventorySlots.Count; i++)
        {
            //If item already exists in the inventorySlots
            if (playerInventory.GetComponent<InventoryProps>().inventorySlots[i].slotName == lootName && playerInventory.GetComponent<InventoryProps>().inventorySlots[i].slotQty < playerInventory.GetComponent<InventoryProps>().inventorySlots[i].slotStack)
            {
                //If the existing slot amount + the item amount is less than the stack maximum, add it to the existing stack
                if (playerInventory.GetComponent<InventoryProps>().inventorySlots[i].slotQty + lootQty <= playerInventory.GetComponent<InventoryProps>().inventorySlots[i].slotStack)
                {
                    playerInventory.GetComponent<InventoryProps>().inventorySlots[i].addAmount(lootQty);
                    Destroy(loot);
                    hasItem = true;
                    break;
                }

                //Else get the difference, and add the difference to the existing stack, then if there is a spare slot, create a new stack with the remaining amount
                else
                {

                    int diff = playerInventory.GetComponent<InventoryProps>().inventorySlots[i].slotStack - playerInventory.GetComponent<InventoryProps>().inventorySlots[i].slotQty;
                    int amountRemaining = lootQty - diff;

                    playerInventory.GetComponent<InventoryProps>().inventorySlots[i].addAmount(diff);

                    //Get the next available null slot
                    int emptySlot = GetEmptySlot(playerInventory.inventorySlots);

                    if (emptySlot >= 0)
                    {
                        playerInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotSprite = lootSprite;
                        playerInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotName = lootName;
                        playerInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotTxt = lootTxt;
                        playerInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotQty = amountRemaining;
                        playerInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotStack = lootStack;
                        playerInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotValue = lootValue;
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
            int emptySlot = GetEmptySlot(playerInventory.inventorySlots);
            if (emptySlot >= 0)
            {
                playerInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotSprite = lootSprite;
                playerInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotName = lootName;
                playerInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotTxt = lootTxt;
                playerInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotQty = lootQty;
                playerInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotStack = lootStack;
                playerInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotValue = lootValue;
                Destroy(loot);
            }

            hasItem = true;
        }

        UpdateSlotUI();

    }

    //Transfer selected slot to the other inventory
    public void TransferSlot(int slotIndex, List<InventorySlot> sourceSlots, List<InventorySlot> targetSlots, bool transferAll)
    {
        if (sourceSlots[slotIndex].slotSprite != null)
        {
            //If single qty is true, transfer only one item rather than the whole stack
            if (transferAll)
            {
                lootQty = sourceSlots[slotIndex].slotQty;
            }
            else
            {
                lootQty = 1;
            }

            Debug.Log(lootQty);
            

            bool hasItem = false;

            //Loop through each slot in the inventorySlots
            for (int i = 0; i < targetSlots.Count; i++)
            {
                //If item already exists in the inventorySlots
                if (targetSlots[i].slotName == sourceSlots[slotIndex].slotName && targetSlots[i].slotQty < targetSlots[i].slotStack)
                {
                    //If the existing slot amount + the item amount is less than the stack maximum, add it to the existing stack
                    if (lootQty + targetSlots[i].slotQty <= targetSlots[i].slotStack)
                    {
                        targetSlots[i].addAmount(lootQty);
                        if (transferAll)
                        {
                            ClearInventorySlot(slotIndex, sourceSlots);
                        }
                        else
                        {
                            if (sourceSlots[slotIndex].slotQty > 1)
                            {
                                sourceSlots[slotIndex].slotQty -= 1;
                            }
                            else
                            {
                                ClearInventorySlot(slotIndex, sourceSlots);
                            }
                        }
                        hasItem = true;
                        break;
                    }

                    //Else get the difference, and add the difference to the existing stack, then if there is a spare slot, create a new stack with the remaining amount
                    else
                    {

                        int diff = targetSlots[i].slotStack - targetSlots[i].slotQty;
                        int amountRemaining = lootQty - diff;
                        targetSlots[i].addAmount(diff);

                        //Get the next available null slot
                        int emptySlot = GetEmptySlot(targetSlots);

                        if (emptySlot >= 0)
                        {
                            targetSlots[emptySlot].slotSprite = sourceSlots[slotIndex].slotSprite;
                            targetSlots[emptySlot].slotName = sourceSlots[slotIndex].slotName;
                            targetSlots[emptySlot].slotTxt = sourceSlots[slotIndex].slotTxt;
                            targetSlots[emptySlot].slotQty = amountRemaining;
                            targetSlots[emptySlot].slotStack = sourceSlots[slotIndex].slotStack;
                            targetSlots[emptySlot].slotValue = sourceSlots[slotIndex].slotValue;
                            if (transferAll)
                            {
                                ClearInventorySlot(slotIndex, sourceSlots);
                            }
                            else
                            {
                                if (sourceSlots[slotIndex].slotQty > 1)
                                {
                                    sourceSlots[slotIndex].slotQty -= 1;
                                }
                                else
                                {
                                    ClearInventorySlot(slotIndex, sourceSlots);
                                }
                            }
                        }
                        //Else if there's no slots left, add only what it can, leave the remaining qty on the player
                        else
                        {
                            Debug.Log("No space left");
                            sourceSlots[slotIndex].slotQty = amountRemaining;
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
                int emptySlot = GetEmptySlot(targetSlots);
                if (emptySlot >= 0)
                {
                    targetSlots[emptySlot].slotSprite = sourceSlots[slotIndex].slotSprite;
                    targetSlots[emptySlot].slotName = sourceSlots[slotIndex].slotName;
                    targetSlots[emptySlot].slotTxt = sourceSlots[slotIndex].slotTxt;
                    targetSlots[emptySlot].slotQty = sourceSlots[slotIndex].slotQty;
                    targetSlots[emptySlot].slotStack = sourceSlots[slotIndex].slotStack;
                    targetSlots[emptySlot].slotValue = sourceSlots[slotIndex].slotValue;
                    if (transferAll)
                    {
                        ClearInventorySlot(slotIndex, sourceSlots);
                    }
                    else
                    {
                        if(sourceSlots[slotIndex].slotQty > 1)
                        {
                            sourceSlots[slotIndex].slotQty -= 1;
                        }
                        else
                        {
                            ClearInventorySlot(slotIndex, sourceSlots);
                        }
                    }
                }

                hasItem = true;
            }

            UpdateSlotUI();

        }
        else
        {
            Debug.Log("Nothing to transfer");
        }

    }

    //Get the next available slot in inventoryinventorySlots
    public int GetEmptySlot(List<InventorySlot> refSlots)
    {
        int slot = -1;

        for (int i = 0; i < refSlots.Count; i++)
        {
            if (String.IsNullOrEmpty(refSlots[i].slotName))
            {
                slot = i;
                break;
            }
        }

        return slot;
    }

    //Clear target slot out completely
    public void ClearInventorySlot(int slotIndex, List<InventorySlot> refSlots)
    {
        refSlots[slotIndex].slotSprite = null;
        refSlots[slotIndex].slotName = null;
        refSlots[slotIndex].slotTxt = null;
        refSlots[slotIndex].slotQty = 0;
        refSlots[slotIndex].slotStack = 0;
        refSlots[slotIndex].slotValue = 0;
    }

    //Update Slot UI based on index and reference object, setting sprite and text (or I could try looping through all 36 objects)
    void UpdateSlotUI()
    {
        List<InventorySlot> sourceSlots = sourceInventoryStatic.GetComponent<InventoryProps>().inventorySlots;

        for (int i = 0; i < sourceInventorySlotsUI.Count; i++)
        {
            //Loop through all Source Inventory slots
            if (sourceSlots[i].slotSprite != null)
            {
                sourceInventorySlotsUI[i].GetComponent<InventorySlotProps>().slotContents.SetActive(true);
                sourceInventorySlotsUI[i].GetComponent<InventorySlotProps>().slotSprite.sprite = sourceSlots[i].slotSprite;
                sourceInventorySlotsUI[i].GetComponent<InventorySlotProps>().slotQty.text = "X" + sourceSlots[i].slotQty.ToString();
            }
            else
            {
                sourceInventorySlotsUI[i].GetComponent<InventorySlotProps>().slotContents.SetActive(false);
                sourceInventorySlotsUI[i].GetComponent<InventorySlotProps>().slotSprite.sprite = null;
                sourceInventorySlotsUI[i].GetComponent<InventorySlotProps>().slotQty.text = null;
            }
        }

        //Loop through all Target Inventory slots (only if it's open)
        if (targetInventory != null)
        {
            List<InventorySlot> targetSlots = targetInventoryStatic.GetComponent<InventoryProps>().inventorySlots;

            for (int i = 0; i < targetInventorySlotsUI.Count; i++)
            {
                if (targetSlots[i].slotSprite != null)
                {
                    targetInventorySlotsUI[i].GetComponent<InventorySlotProps>().slotContents.SetActive(true);
                    targetInventorySlotsUI[i].GetComponent<InventorySlotProps>().slotSprite.sprite = targetSlots[i].slotSprite;
                    targetInventorySlotsUI[i].GetComponent<InventorySlotProps>().slotQty.text = "X" + targetSlots[i].slotQty.ToString();
                }
                else
                {
                    targetInventorySlotsUI[i].GetComponent<InventorySlotProps>().slotContents.SetActive(false);
                    targetInventorySlotsUI[i].GetComponent<InventorySlotProps>().slotSprite.sprite = null;
                    targetInventorySlotsUI[i].GetComponent<InventorySlotProps>().slotQty.text = null;
                }
            }
        }
    }

    //Open the Inventory UI
    public void OpenInventoryUI(bool showTargetInventory)
    {
        if (showTargetInventory)
        {
            ui_TargetInventory.SetActive(true);
        }

        curinventorySlotsUI = sourceInventorySlotsUI;
        GameController.instance.gameState = GameController.GameState.inventory;
        curTarget = 0;
        ui_Selector.SetActive(true);
    }

    //Checks the player inventory to see if they've got enough of the loot required to build the target object
    public bool HasResources(GameObject buildable)
    {
        List<BuildingRecipe> resourcesNeeded = new List<BuildingRecipe>();
        resourcesNeeded = buildable.GetComponent<BuildingProps>().buildingRecipe;

        int itemsFulfilled = 0;
        InventoryProps inventory = PlayerController.instance.GetComponent<InventoryProps>();

        foreach (BuildingRecipe resource in resourcesNeeded)
        {
            int itemTotal = 0;

            for (int i = 0; i < inventory.inventorySlots.Count; i++)
            {
                string itemName = inventory.inventorySlots[i].slotName;
                if (inventory.inventorySlots[i].slotName == resource.lootObject.GetComponent<LootProps>().lootName)
                {
                    //take the itemname and the slotqty and accumulate it
                    itemTotal += inventory.inventorySlots[i].slotQty;
                }

            }
            //Debug.Log(resource.itemName + ": " + itemTotal);
            //if the accumulatedqty of the itemname > resource.needed, flag the resource as complete
            if (itemTotal >= resource.itemCost)
            {
                //resource.canAfford = true;
                itemsFulfilled += 1;
            }

        }

        if (itemsFulfilled >= resourcesNeeded.Count)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public void DeductResources(GameObject buildingObject)
    {
        List<BuildingRecipe> resourcesNeeded = new List<BuildingRecipe>();
        resourcesNeeded = buildingObject.GetComponent<BuildingProps>().buildingRecipe;
        InventoryProps inventory = PlayerController.instance.GetComponent<InventoryProps>();

        foreach (BuildingRecipe resource in resourcesNeeded)
        {
            int itemTotal = resource.itemCost;

            if (itemTotal > 0)
            {
                for (int i = 0; i < inventory.inventorySlots.Count; i++)
                {
                    if (inventory.inventorySlots[i].slotName == resource.lootObject.GetComponent<LootProps>().lootName)
                    {
                        //If the first slot found has more than what's needed, deduct the quantity from that slot
                        if (inventory.inventorySlots[i].slotQty >= resource.itemCost)
                        {
                            inventory.inventorySlots[i].slotQty -= resource.itemCost;
                            break;
                        }
                        else
                        {
                            int amountToSubstract = Mathf.Clamp(inventory.inventorySlots[i].slotQty, 0, itemTotal);
                            inventory.inventorySlots[i].slotQty -= amountToSubstract;
                            itemTotal -= amountToSubstract;
                            break;
                        }
                    }
                }
            }

        }

        //Clean up each Slot and check if there's anything left, removing it if not
        for (int i = 0; i < inventory.inventorySlots.Count; i++)
        {
            if (inventory.inventorySlots[i].slotQty <= 0)
            {
                ClearInventorySlot(i, inventory.inventorySlots);
            }
        }
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
    public int slotValue;

    public InventorySlot(Sprite _lootsprite, string _lootName, string _lootTxt, int _lootQty, int _lootStack, int _lootValue)
    {
        slotSprite = _lootsprite;
        slotName = _lootName;
        slotTxt = _lootTxt;
        slotQty = _lootQty;
        slotStack = _lootStack;
        slotValue = _lootValue;
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