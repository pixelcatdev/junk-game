using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used to initiate trade with a trader
public class TradeController : MonoBehaviour
{
    public List<GameObject> lootRefs;

    // Start is called before the first frame update
    void Start()
    {
        RandomiseStock();
    }

    public void RandomiseStock()
    {
        InventoryProps tradeInventory = GetComponent<InventoryProps>();

        for (int i = 0; i < tradeInventory.inventorySlots.Count; i++)
        {
            //get a random item from the lootRefs and randomise the quantity
            GameObject randomLoot = GetRandomLoot();
            tradeInventory.inventorySlots[i].slotSprite = randomLoot.GetComponent<LootProps>().lootSprite;
            tradeInventory.inventorySlots[i].slotName = randomLoot.GetComponent<LootProps>().lootName;            
            tradeInventory.inventorySlots[i].slotTxt = randomLoot.GetComponent<LootProps>().lootTxt;
            tradeInventory.inventorySlots[i].slotQty = Random.Range(1, 10);
            tradeInventory.inventorySlots[i].slotStack = randomLoot.GetComponent<LootProps>().lootStack;
        }
    }

    //Gets a random game object for each trade inventory generation
    GameObject GetRandomLoot()
    {
        GameObject loot = lootRefs[Random.Range(0, lootRefs.Count -1)];
        return loot;
    }

    public void StartTrade()
    {
        Debug.Log("Starting Trade");
        RandomiseStock();
    }

    public void BuyLoot()
    {
        //If there's qty available
        Debug.Log("Adding item to buy");
        //Deduct quantity from trade inventory
        //Add quantity to buy basket
        //Update Slot UI
    }

    public void SellLoot()
    {
        //If there's qty available
        Debug.Log("Adding item to sell");
        //Deduct quantity from trade inventory
        //Add quantity to buy basket
        //Update Slot UI
    }

}
