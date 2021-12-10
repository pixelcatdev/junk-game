using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcProps : MonoBehaviour
{
    public bool isTrader;
    public bool isMechanic;
    public bool isTechnician;
    public bool isContractGiver;

    public bool hasRandomDialogue;
    public bool isWanderer;

    public GameObject body;
    public GameObject hair;
    
    public List<Color> skinTones;
    public List<Color> hairColors;
    public List<Sprite> hairStyles;

    private void Start()
    {
        GenNpc();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenNpc();
        }
    }

    public void GenNpc()
    {
        //Set skin tone
        body.GetComponent<SpriteRenderer>().color = skinTones[Random.Range(0, skinTones.Count)];

        //Set hair type and color
        hair.GetComponent<SpriteRenderer>().sprite = hairStyles[Random.Range(0, hairStyles.Count)];
        hair.GetComponent<SpriteRenderer>().color = hairColors[Random.Range(0, hairColors.Count)];

        //Generate Inventory
        if (isTrader)
        {
            RandomiseInventory();
        }        
    }

    public void Activate()
    {
        //Start trade
        if (isTrader)
        {
            TradeController.instance.tradeInventory = GetComponent<InventoryProps>();
            TradeController.instance.StartTrade();
        }

        //Start Ship Upgrade
        else if (isMechanic)
        {

        }

        //Start Player Upgrade
        else if (isTechnician)
        {

        }

        //Start Contract UI
        else if (isContractGiver)
        {

        }

        //Call random speech bubble
        else if (hasRandomDialogue)
        {

        }
    }

    public void RandomiseInventory()
    {
        for (int i = 0; i < GetComponent<InventoryProps>().inventorySlots.Count; i++)
        {
            //get a random item from the lootRefs and randomise the quantity
            GameObject randomLoot = TradeController.instance.GetRandomLoot();
            GetComponent<InventoryProps>().inventorySlots[i].slotSprite = randomLoot.GetComponent<LootProps>().lootSprite;
            GetComponent<InventoryProps>().inventorySlots[i].slotName = randomLoot.GetComponent<LootProps>().lootName;
            GetComponent<InventoryProps>().inventorySlots[i].slotTxt = randomLoot.GetComponent<LootProps>().lootTxt;
            GetComponent<InventoryProps>().inventorySlots[i].slotQty = UnityEngine.Random.Range(1, randomLoot.GetComponent<LootProps>().lootStack);
            GetComponent<InventoryProps>().inventorySlots[i].slotStack = randomLoot.GetComponent<LootProps>().lootStack;
            GetComponent<InventoryProps>().inventorySlots[i].slotValue = randomLoot.GetComponent<LootProps>().lootValue;
        }
    }
}
