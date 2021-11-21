﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

//Used to trade with traders
public class TradeController : MonoBehaviour
{
    public List<GameObject> lootRefs;
    private InventoryProps tradeInventory;
    private InventoryProps playerInventory;

    public List<InventorySlot> sellInventory = new List<InventorySlot>();
    public List<InventorySlot> buyInventory = new List<InventorySlot>();

    public GameObject ui_trade;
    public List<GameObject> traderSlotsUI;
    public List<GameObject> playerSlotsUI;
    public List<GameObject> buySlotsUI;
    public List<GameObject> sellSlotsUI;

    private int slotIndex;
    private int buyTotal;
    private int sellTotal;

    public TextMeshProUGUI txt_total;

    // Start is called before the first frame update
    void Start()
    {
        tradeInventory = GetComponent<InventoryProps>();
        playerInventory = PlayerController.instance.GetComponent<InventoryProps>();
        RandomiseStock();
        UpdateTradeUI();
    }

    public void RandomiseStock()
    {
        for (int i = 0; i < tradeInventory.inventorySlots.Count; i++)
        {
            //get a random item from the lootRefs and randomise the quantity
            GameObject randomLoot = GetRandomLoot();
            tradeInventory.inventorySlots[i].slotSprite = randomLoot.GetComponent<LootProps>().lootSprite;
            tradeInventory.inventorySlots[i].slotName = randomLoot.GetComponent<LootProps>().lootName;
            tradeInventory.inventorySlots[i].slotTxt = randomLoot.GetComponent<LootProps>().lootTxt;
            tradeInventory.inventorySlots[i].slotQty = UnityEngine.Random.Range(1, randomLoot.GetComponent<LootProps>().lootStack);
            tradeInventory.inventorySlots[i].slotStack = randomLoot.GetComponent<LootProps>().lootStack;
            tradeInventory.inventorySlots[i].slotValue = randomLoot.GetComponent<LootProps>().lootValue;
        }
    }

    //Gets a random game object for each trade inventory generation
    GameObject GetRandomLoot()
    {
        GameObject loot = lootRefs[UnityEngine.Random.Range(0, lootRefs.Count - 1)];
        return loot;
    }

    public void StartTrade()
    {
        Debug.Log("Starting Trade");
        RandomiseStock();
        UpdateTradeUI();
        ui_trade.SetActive(true);
    }

    //Buy 1 from the clicked slot from the Player Inventory
    public void BuyLoot()
    {
        //Get the slotindex of the button pressed
        slotIndex = traderSlotsUI.IndexOf(EventSystem.current.currentSelectedGameObject.transform.parent.gameObject);
        //Debug.Log(EventSystem.current.currentSelectedGameObject.transform.parent.gameObject + ": " + slotIndex);

        Sprite lootSprite = tradeInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotSprite;
        string lootName = tradeInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotName;
        string lootTxt = tradeInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotTxt;
        int lootStack = tradeInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotStack;
        int lootValue = tradeInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotValue;

        int lootQty = tradeInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotQty;

        bool hasItem = false;

        if(tradeInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotSprite != null)
        {
            //loop through buy basket to see if it exists
            for (int i = 0; i < buyInventory.Count; i++)
            {
                //If the item already exists in the buy list and there's enough room in the stack
                if (lootName == buyInventory[i].slotName && buyInventory[i].slotQty < buyInventory[i].slotStack)
                {
                    if (1 + buyInventory[i].slotQty <= buyInventory[i].slotStack)
                    {
                        //Debug.Log((i + ": " + buyInventory[i].slotQty + 1) + " - " + buyInventory[i].slotStack);
                        buyInventory[i].slotQty += 1;
                        RemoveSlotQty(slotIndex, tradeInventory.GetComponent<InventoryProps>().inventorySlots);
                        buyTotal += tradeInventory.inventorySlots[slotIndex].slotValue;
                        hasItem = true;
                        break;
                    }
                    //Otherwise, see if there's any spare slots in the BuyInventory
                    else
                    {
                        int emptySlot = GetEmptySlot(buyInventory);

                        if (emptySlot >= 0)
                        {
                            buyInventory[emptySlot].slotSprite = lootSprite;
                            buyInventory[emptySlot].slotName = lootName;
                            buyInventory[emptySlot].slotTxt = lootTxt;
                            buyInventory[emptySlot].slotQty = 1;
                            buyInventory[emptySlot].slotStack = lootStack;
                            buyInventory[emptySlot].slotValue = lootValue;
                            RemoveSlotQty(slotIndex, tradeInventory.GetComponent<InventoryProps>().inventorySlots);
                            buyTotal += tradeInventory.inventorySlots[slotIndex].slotValue;
                        }
                        else
                        {
                            Debug.Log("No buy space left");
                            //Indicate to the player no room left
                        }

                        hasItem = true;
                        break;
                    }
                }
            }

            //if the item doesn't exist in the invetory, find an empty slot and assign it there
            if (!hasItem)
            {
                int emptySlot = GetEmptySlot(buyInventory);

                if (emptySlot >= 0)
                {
                    buyInventory[emptySlot].slotSprite = lootSprite;
                    buyInventory[emptySlot].slotName = lootName;
                    buyInventory[emptySlot].slotTxt = lootTxt;
                    buyInventory[emptySlot].slotStack = lootStack;
                    buyInventory[emptySlot].slotQty = 1;
                    buyInventory[emptySlot].slotValue = lootValue;
                    RemoveSlotQty(slotIndex, tradeInventory.GetComponent<InventoryProps>().inventorySlots);
                    buyTotal += tradeInventory.inventorySlots[slotIndex].slotValue;
                }
                else
                {
                    Debug.Log("No buy space left");
                }

                hasItem = true;
            }
        }        

        //Update the buyTotal and add it to the inventory
        UpdateTradeUI();
    }

    //Sell 1 from the clicked slot from the Player Inventory
    public void SellLoot()
    {
        //Get the slotindex of the button pressed
        slotIndex = playerSlotsUI.IndexOf(EventSystem.current.currentSelectedGameObject.transform.parent.gameObject);
        //Debug.Log(EventSystem.current.currentSelectedGameObject.transform.parent.gameObject + ": " + slotIndex);

        Sprite lootSprite = playerInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotSprite;
        string lootName = playerInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotName;
        string lootTxt = playerInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotTxt;
        int lootStack = playerInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotStack;
        int lootValue = playerInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotValue;

        int lootQty = playerInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotQty;

        bool hasItem = false;

        if (playerInventory.GetComponent<InventoryProps>().inventorySlots[slotIndex].slotSprite != null)
        {
            //loop through buy basket to see if it exists
            for (int i = 0; i < sellInventory.Count; i++)
            {
                //If the item already exists in the buy list and there's enough room in the stack
                if (lootName == sellInventory[i].slotName && sellInventory[i].slotQty < sellInventory[i].slotStack)
                {
                    if (1 + sellInventory[i].slotQty <= sellInventory[i].slotStack)
                    {
                        //Debug.Log((i + ": " + buyInventory[i].slotQty + 1) + " - " + buyInventory[i].slotStack);
                        sellInventory[i].slotQty += 1;
                        RemoveSlotQty(slotIndex, playerInventory.GetComponent<InventoryProps>().inventorySlots);
                        sellTotal += playerInventory.inventorySlots[slotIndex].slotValue;
                        hasItem = true;
                        break;
                    }
                    //Otherwise, see if there's any spare slots in the BuyInventory
                    else
                    {
                        int emptySlot = GetEmptySlot(sellInventory);

                        if (emptySlot >= 0)
                        {
                            sellInventory[emptySlot].slotSprite = lootSprite;
                            sellInventory[emptySlot].slotName = lootName;
                            sellInventory[emptySlot].slotTxt = lootTxt;
                            sellInventory[emptySlot].slotQty = 1;
                            sellInventory[emptySlot].slotStack = lootStack;
                            sellInventory[emptySlot].slotValue = lootValue;
                            RemoveSlotQty(slotIndex, playerInventory.GetComponent<InventoryProps>().inventorySlots);
                            sellTotal += playerInventory.inventorySlots[slotIndex].slotValue;
                        }
                        else
                        {
                            Debug.Log("No buy space left");
                            //Indicate to the player no room left
                        }

                        hasItem = true;
                        break;
                    }
                }
            }

            //if the item doesn't exist in the invetory, find an empty slot and assign it there
            if (!hasItem)
            {
                int emptySlot = GetEmptySlot(sellInventory);

                if (emptySlot >= 0)
                {
                    sellInventory[emptySlot].slotSprite = lootSprite;
                    sellInventory[emptySlot].slotName = lootName;
                    sellInventory[emptySlot].slotTxt = lootTxt;
                    sellInventory[emptySlot].slotStack = lootStack;
                    sellInventory[emptySlot].slotQty = 1;
                    sellInventory[emptySlot].slotValue = lootValue;
                    RemoveSlotQty(slotIndex, playerInventory.GetComponent<InventoryProps>().inventorySlots);
                    sellTotal += playerInventory.inventorySlots[slotIndex].slotValue;
                }
                else
                {
                    Debug.Log("No buy space left");
                }

                hasItem = true;
            }
        }

        //Update the buyTotal and add it to the inventory
        UpdateTradeUI();
    }

    //Remove the Qty and check if it's cleared out
    void RemoveSlotQty(int slotIndex, List<InventorySlot> refSlots)
    {
        if(refSlots[slotIndex].slotQty - 1 <= 0)
        {
            ClearInventorySlot(slotIndex, refSlots);
        }
        else
        {
            refSlots[slotIndex].slotQty -= 1;
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
    private void ClearInventorySlot(int slotIndex, List<InventorySlot> refSlots)
    {
        refSlots[slotIndex].slotSprite = null;
        refSlots[slotIndex].slotName = null;
        refSlots[slotIndex].slotTxt = null;
        refSlots[slotIndex].slotQty = 0;
        refSlots[slotIndex].slotStack = 0;
        UpdateTradeUI();
    }

    //Completes the trade
    public void CompleteTrade()
    {
        //Is the player trading enough value against what's being bought?
        if (buyTotal <= sellTotal)
        {
            //Complete Trade
            Debug.Log("Value accepted");
            //Add buy items to Players Inventory
            //Remove buy items from buyinventory
            //Add sell items to Trader Inventory
            //Remove sell items from buyinventory
            UpdateTradeUI();
        }
        else
        {
            Debug.Log("Value denied");
            //Advise player
        }
    }

    //Clear out the buy and sell slots
    public void ClearTrade()
    {
        //return sellinventory to player inventory
        //return buyinventory to trader inventory
        //clear sellinventory
        //clear buyinventory

        UpdateTradeUI();
    }

    public void CancelTrade()
    {
        ui_trade.SetActive(false);
    }

    //Update Trade slot graphics
    void UpdateTradeUI()
    {
        //loop through all player inventory slots, set the sprite, qty
        List<InventorySlot> playerSlots = PlayerController.instance.GetComponent<InventoryProps>().inventorySlots;

        for (int i = 0; i < playerSlotsUI.Count; i++)
        {
            if (playerSlots[i].slotSprite != null)
            {
                playerSlotsUI[i].GetComponent<InventorySlotProps>().slotContents.SetActive(true);
                playerSlotsUI[i].GetComponent<InventorySlotProps>().slotSprite.sprite = playerSlots[i].slotSprite;
                playerSlotsUI[i].GetComponent<InventorySlotProps>().slotValue.text = playerSlots[i].slotValue.ToString();
                playerSlotsUI[i].GetComponent<InventorySlotProps>().slotQty.text = "X" + playerSlots[i].slotQty.ToString();
            }
            else
            {
                playerSlotsUI[i].GetComponent<InventorySlotProps>().slotContents.SetActive(false);
                playerSlotsUI[i].GetComponent<InventorySlotProps>().slotSprite.sprite = null;
                playerSlotsUI[i].GetComponent<InventorySlotProps>().slotValue.text = null;
                playerSlotsUI[i].GetComponent<InventorySlotProps>().slotQty.text = null;
            }
        }

        //loop through all trader inventory slots, set the sprite, qty
        List<InventorySlot> traderSlots = GetComponent<InventoryProps>().inventorySlots;

        for (int i = 0; i < traderSlotsUI.Count; i++)
        {
            if (traderSlots[i].slotSprite != null)
            {
                traderSlotsUI[i].GetComponent<InventorySlotProps>().slotContents.SetActive(true);
                traderSlotsUI[i].GetComponent<InventorySlotProps>().slotSprite.sprite = traderSlots[i].slotSprite;
                traderSlotsUI[i].GetComponent<InventorySlotProps>().slotValue.text = traderSlots[i].slotValue.ToString();
                traderSlotsUI[i].GetComponent<InventorySlotProps>().slotQty.text = "X" + traderSlots[i].slotQty.ToString();
            }
            else
            {
                traderSlotsUI[i].GetComponent<InventorySlotProps>().slotContents.SetActive(false);
                traderSlotsUI[i].GetComponent<InventorySlotProps>().slotSprite.sprite = null;
                traderSlotsUI[i].GetComponent<InventorySlotProps>().slotValue.text = null;
                traderSlotsUI[i].GetComponent<InventorySlotProps>().slotQty.text = null;
            }
        }

        ////loop through all buy inventory slots, set the sprite, qty
        for (int i = 0; i < buySlotsUI.Count; i++)
        {
            if (buyInventory[i].slotSprite != null)
            {
                buySlotsUI[i].GetComponent<InventorySlotProps>().slotContents.SetActive(true);
                buySlotsUI[i].GetComponent<InventorySlotProps>().slotSprite.sprite = buyInventory[i].slotSprite;
                buySlotsUI[i].GetComponent<InventorySlotProps>().slotValue.text = buyInventory[i].slotValue.ToString();
                buySlotsUI[i].GetComponent<InventorySlotProps>().slotQty.text = "X" + buyInventory[i].slotQty.ToString();
            }
            else
            {
                buySlotsUI[i].GetComponent<InventorySlotProps>().slotContents.SetActive(false);
                buySlotsUI[i].GetComponent<InventorySlotProps>().slotSprite.sprite = null;
                buySlotsUI[i].GetComponent<InventorySlotProps>().slotValue.text = null;
                buySlotsUI[i].GetComponent<InventorySlotProps>().slotQty.text = null;
            }
        }

        ////loop through all sell inventory slots, set the sprite, qty
        for (int i = 0; i < sellSlotsUI.Count; i++)
        {
            if (sellInventory[i].slotSprite != null)
            {
                sellSlotsUI[i].GetComponent<InventorySlotProps>().slotContents.SetActive(true);
                sellSlotsUI[i].GetComponent<InventorySlotProps>().slotSprite.sprite = sellInventory[i].slotSprite;
                sellSlotsUI[i].GetComponent<InventorySlotProps>().slotValue.text = sellInventory[i].slotValue.ToString();
                sellSlotsUI[i].GetComponent<InventorySlotProps>().slotQty.text = "X" + sellInventory[i].slotQty.ToString();
            }
            else
            {
                sellSlotsUI[i].GetComponent<InventorySlotProps>().slotContents.SetActive(false);
                sellSlotsUI[i].GetComponent<InventorySlotProps>().slotSprite.sprite = null;
                sellSlotsUI[i].GetComponent<InventorySlotProps>().slotValue.text = null;
                sellSlotsUI[i].GetComponent<InventorySlotProps>().slotQty.text = null;
            }
        }

        //update txt_total value
        txt_total.text = "TOTAL: " + (buyTotal - sellTotal);
    }

}