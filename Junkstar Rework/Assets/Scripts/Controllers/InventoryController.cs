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
    public GameObject sourceInventory;
    public GameObject targetInventory;
    public List<GameObject> lootRefs;

    [SerializeField] List<GameObject> curinventorySlotsUI;
    public List<GameObject> sourceInventorySlotsUI;
    public List<GameObject> targetInventorySlotsUI;

    public GameObject ui_Selector;
    public GameObject ui_SourceInventory;
    public GameObject ui_TargetInventory;
    public int curTarget;
    private bool canSwitchTarget;
    public bool targetInventoryEnabled;

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
        curinventorySlotsUI = sourceInventorySlotsUI;
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
                sourceInventory = PlayerController.instance.gameObject;
                targetInventory = null;
            }

            //Trash slot contents
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                ClearInventorySlot(curTarget,sourceInventory);
            }

            //Transfer contents to other inventory if open
            if (targetInventory != null && targetInventory.activeInHierarchy)
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    TransferSlot();
                }
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
        for (int i = 0; i < sourceInventory.GetComponent<InventoryProps>().inventorySlots.Count; i++)
        {
            //If item already exists in the inventorySlots
            if (sourceInventory.GetComponent<InventoryProps>().inventorySlots[i].slotName == lootName && sourceInventory.GetComponent<InventoryProps>().inventorySlots[i].slotQty < sourceInventory.GetComponent<InventoryProps>().inventorySlots[i].slotStack)
            {
                //If the existing slot amount + the item amount is less than the stack maximum, add it to the existing stack
                if (sourceInventory.GetComponent<InventoryProps>().inventorySlots[i].slotQty + lootQty <= sourceInventory.GetComponent<InventoryProps>().inventorySlots[i].slotStack)
                {
                    sourceInventory.GetComponent<InventoryProps>().inventorySlots[i].addAmount(lootQty);
                    Destroy(loot);
                    hasItem = true;
                    break;
                }

                //Else get the difference, and add the difference to the existing stack, then if there is a spare slot, create a new stack with the remaining amount
                else
                {

                    int diff = sourceInventory.GetComponent<InventoryProps>().inventorySlots[i].slotStack - sourceInventory.GetComponent<InventoryProps>().inventorySlots[i].slotQty;
                    int amountRemaining = lootQty - diff;

                    sourceInventory.GetComponent<InventoryProps>().inventorySlots[i].addAmount(diff);

                    //Get the next available null slot
                    int emptySlot = GetEmptySlot(sourceInventory);

                    if (emptySlot >= 0)
                    {
                        sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotSprite = lootSprite;
                        sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotName = lootName;
                        sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotTxt = lootTxt;
                        sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotQty = amountRemaining;
                        sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotStack = lootStack;
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
            int emptySlot = GetEmptySlot(sourceInventory);
            if (emptySlot >= 0)
            {
                sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotSprite = lootSprite;
                sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotName = lootName;
                sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotTxt = lootTxt;
                sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotQty = lootQty;
                sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotStack = lootStack;
                Destroy(loot);
            }

            hasItem = true;
        }

        UpdateSlotUI();

    }

    //Get the next available slot in inventoryinventorySlots
    int GetEmptySlot(GameObject refInventory)
    {
        int slot = -1;

        for (int i = 0; i < refInventory.GetComponent<InventoryProps>().inventorySlots.Count; i++)
        {
            if (String.IsNullOrEmpty(refInventory.GetComponent<InventoryProps>().inventorySlots[i].slotName))
            {
                slot = i;
                break;
            }
        }

        return slot;
    }

    //Clear target slot out completely
    private void ClearInventorySlot(int slotIndex, GameObject refInventory)
    {
        refInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotSprite = null;
        refInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotName = null;
        refInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotTxt = null;
        refInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotQty = 0;
        refInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotStack = 0;
        UpdateSlotUI();
    }

    //Transfer selected slot to the other inventory
    void TransferSlot()
    {
        if(sourceInventory.GetComponent<InventoryProps>().inventorySlots[curTarget].slotSprite != null)
        {
            int lootQty = sourceInventory.GetComponent<InventoryProps>().inventorySlots[curTarget].slotQty;
            List<InventorySlot> sourceSlots = sourceInventory.GetComponent<InventoryProps>().inventorySlots;
            List<InventorySlot> targetSlots = targetInventory.GetComponent<InventoryProps>().inventorySlots;

            bool hasItem = false;

            //Loop through each slot in the inventorySlots
            for (int i = 0; i < targetSlots.Count; i++)
            {
                //If item already exists in the inventorySlots
                if (targetSlots[i].slotName == sourceSlots[curTarget].slotName && targetSlots[i].slotQty < targetSlots[i].slotStack)
                {
                    //If the existing slot amount + the item amount is less than the stack maximum, add it to the existing stack
                    if (lootQty + targetSlots[i].slotQty <= targetSlots[i].slotStack)
                    {
                        targetSlots[i].addAmount(lootQty);
                        ClearInventorySlot(curTarget, sourceInventory);
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
                        int emptySlot = GetEmptySlot(targetInventory);

                        if (emptySlot >= 0)
                        {
                            targetSlots[emptySlot].slotSprite = sourceSlots[curTarget].slotSprite;
                            targetSlots[emptySlot].slotName = sourceSlots[curTarget].slotName;
                            targetSlots[emptySlot].slotTxt = sourceSlots[curTarget].slotTxt;
                            targetSlots[emptySlot].slotQty = amountRemaining;
                            targetSlots[emptySlot].slotStack = sourceSlots[curTarget].slotStack;
                            ClearInventorySlot(curTarget, sourceInventory);
                        }
                        //Else if there's no slots left, add only what it can, leave the remaining qty on the player
                        else
                        {
                            Debug.Log("No space left");
                            sourceSlots[curTarget].slotQty = amountRemaining;
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
                int emptySlot = GetEmptySlot(targetInventory);
                if (emptySlot >= 0)
                {
                    targetSlots[emptySlot].slotSprite = sourceSlots[curTarget].slotSprite;
                    targetSlots[emptySlot].slotName = sourceSlots[curTarget].slotName;
                    targetSlots[emptySlot].slotTxt = sourceSlots[curTarget].slotTxt;
                    targetSlots[emptySlot].slotQty = sourceSlots[curTarget].slotQty;
                    targetSlots[emptySlot].slotStack = sourceSlots[curTarget].slotStack;
                    ClearInventorySlot(curTarget, sourceInventory);
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

    //Update Slot UI based on index and reference object, setting sprite and text (or I could try looping through all 36 objects)
    void UpdateSlotUI()
    {
        List<InventorySlot> sourceSlots = sourceInventoryStatic.GetComponent<InventoryProps>().inventorySlots;

        Debug.Log("source: " + sourceInventory);

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
        SwitchSlotTarget();
        ui_Selector.SetActive(true);
    }

    //Navigate through the Inventory Slots
    private void NavigateInventory()
    {
        if (canSwitchTarget)
        {
            float dirX = Input.GetAxisRaw("Horizontal");
            float dirY = Input.GetAxisRaw("Vertical");

            //Navigate right
            if (dirX > 0)
            {
                if (curTarget + 1 > sourceInventory.GetComponent<InventoryProps>().inventorySlots.Count - 1)
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
                    curTarget = sourceInventory.GetComponent<InventoryProps>().inventorySlots.Count - 1;
                }
                else
                {
                    curTarget -= 1;
                }

                SwitchSlotTarget();
            }

            //Swap to target inventory if one is open
            if (targetInventory != null && targetInventory.activeInHierarchy)
            {
                //Navigate right
                if (dirY > 0)
                {
                    SwapInventories();
                }

                //Navigate right
                else if (dirY < 0)
                {
                    SwapInventories();
                }
            }
        }
    }

    void SwitchSlotTarget()
    {
        ui_Selector.transform.position = curinventorySlotsUI[curTarget].transform.position;
        canSwitchTarget = false;
        StartCoroutine("SwitchTargetReset");
    }

    //Swap target and source inventories around
    void SwapInventories()
    {
        GameObject source = sourceInventory;
        GameObject target = targetInventory;
        sourceInventory = target;
        targetInventory = source;
        Debug.Log("Current slots UI: " + curinventorySlotsUI);

        if (curinventorySlotsUI == sourceInventorySlotsUI)
        {
            curinventorySlotsUI = targetInventorySlotsUI;
        }
        else
        {
            curinventorySlotsUI = sourceInventorySlotsUI;
        }

        SwitchSlotTarget();
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

    //public void DeductResources(GameObject buildObj)
    //{
    //    List<BuildRecipe> resourcesNeeded = new List<BuildRecipe>();
    //    resourcesNeeded = buildObj.GetComponent<Buildable>().buildingRecipe;

    //    foreach (BuildRecipe resource in resourcesNeeded)
    //    {
    //        int itemTotal = resource.itemCost;

    //        if (itemTotal > 0)
    //        {
    //            for (int i = 0; i < GetComponent<Inventory>().Slots.Count; i++)
    //            {
    //                if (GetComponent<Inventory>().Slots[i].slotName == resource.itemName)
    //                {
    //                    //If the first slot found has more than what's needed, deduct the quantity from that slot
    //                    if (GetComponent<Inventory>().Slots[i].slotQty >= resource.itemCost)
    //                    {
    //                        GetComponent<Inventory>().Slots[i].slotQty -= resource.itemCost;
    //                        break;
    //                    }
    //                    else
    //                    {
    //                        int amountToSubstract = Mathf.Clamp(GetComponent<Inventory>().Slots[i].slotQty, 0, itemTotal);
    //                        GetComponent<Inventory>().Slots[i].slotQty -= amountToSubstract;
    //                        itemTotal -= amountToSubstract;
    //                        break;
    //                    }
    //                }
    //            }
    //        }

    //    }

    //    //Clean up each Slot and check if there's anything left, removing it if not
    //    for (int i = 0; i < GetComponent<Inventory>().Slots.Count; i++)
    //    {
    //        if (GetComponent<Inventory>().Slots[i].slotQty <= 0)
    //        {
    //            GetComponent<Inventory>().ClearSlot(gameObject, i);
    //        }
    //    }
    //}

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