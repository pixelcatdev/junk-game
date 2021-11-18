using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Custom class for any object that holds an inventory (Player, ship storage, traders)
public class InventoryProps : MonoBehaviour
{
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
}
