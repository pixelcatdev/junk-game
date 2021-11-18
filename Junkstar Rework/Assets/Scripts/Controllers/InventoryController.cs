using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryController : MonoBehaviour
{
    //public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public GameObject sourceInventory;
    public GameObject targetInventory;
    public List<GameObject> lootRefs;

    [SerializeField] List<GameObject> curinventorySlotsUI;
    public List<GameObject> sourceInventorySlotsUI;
    public List<GameObject> targetInventorySlotsUI;

    public GameObject ui_Selector;
    public GameObject ui_SourceInventory;
    public GameObject ui_TargetInventory;
    private int curTarget;
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
            }

            //Trash slot contents
            if (Input.GetKeyDown(KeyCode.Delete))
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
        for (int i = 0; i < sourceInventory.GetComponent<InventoryProps>().inventorySlots.Count; i++)
        {
            //If item already exists in the inventorySlots
            if (sourceInventory.GetComponent<InventoryProps>().inventorySlots[i].slotName == lootName && sourceInventory.GetComponent<InventoryProps>().inventorySlots[i].slotQty < sourceInventory.GetComponent<InventoryProps>().inventorySlots[i].slotStack)
            {
                //If the existing slot amount + the item amount is less than the stack maximum, add it to the existing stack
                if (sourceInventory.GetComponent<InventoryProps>().inventorySlots[i].slotQty + lootQty <= sourceInventory.GetComponent<InventoryProps>().inventorySlots[i].slotStack)
                {
                    sourceInventory.GetComponent<InventoryProps>().inventorySlots[i].addAmount(lootQty);
                    UpdateSlotUI(i);
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
                    int emptySlot = GetEmptySlot();

                    if (emptySlot >= 0)
                    {
                        sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotSprite = lootSprite;
                        sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotName = lootName;
                        sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotTxt = lootTxt;
                        sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotQty = amountRemaining;
                        sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotStack = lootStack;
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
                sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotSprite = lootSprite;
                sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotName = lootName;
                sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotTxt = lootTxt;
                sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotQty = lootQty;
                sourceInventory.GetComponent<InventoryProps>().inventorySlots[emptySlot].slotStack = lootStack;
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

        for (int i = 0; i < sourceInventory.GetComponent<InventoryProps>().inventorySlots.Count; i++)
        {
            if (String.IsNullOrEmpty(sourceInventory.GetComponent<InventoryProps>().inventorySlots[i].slotName))
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
        sourceInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotSprite = null;
        sourceInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotName = null;
        sourceInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotTxt = null;
        sourceInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotQty = 0;
        sourceInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotStack = 0;
        UpdateSlotUI(slotIndex);
    }

    //Update Slot UI, setting sprite and text
    void UpdateSlotUI(int slotIndex)
    {
        if(sourceInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotName != null)
        {
            sourceInventorySlotsUI[slotIndex].GetComponent<InventorySlotProps>().slotContents.SetActive(true);
            sourceInventorySlotsUI[slotIndex].GetComponent<InventorySlotProps>().slotSprite.sprite = sourceInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotSprite;
            sourceInventorySlotsUI[slotIndex].GetComponent<InventorySlotProps>().slotQty.text = "X" + sourceInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotQty.ToString();
        }
        else
        {
            sourceInventorySlotsUI[slotIndex].GetComponent<InventorySlotProps>().slotContents.SetActive(false);
            sourceInventorySlotsUI[slotIndex].GetComponent<InventorySlotProps>().slotSprite.sprite = null;
            sourceInventorySlotsUI[slotIndex].GetComponent<InventorySlotProps>().slotQty.text = null;
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

        if(curinventorySlotsUI == sourceInventorySlotsUI)
        {
            curinventorySlotsUI = targetInventorySlotsUI;
        }
        else
        {
            curinventorySlotsUI = sourceInventorySlotsUI;
        }

        SwitchSlotTarget();
        Debug.Log("Swapped");
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