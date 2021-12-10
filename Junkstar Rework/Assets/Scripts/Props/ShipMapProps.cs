using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipMapProps : MonoBehaviour
{
    public string shipName;

    public float mapCurHealth;
    public float mapMaxHealth;
    public float mapCritHealth;
    public float mapPower;
    public float mapCurFuel;

    public int hull;
    public int hullMax;
    public float fuel;
    public List<GameObject> weaponSlots;
    //public GameObject weaponSlot1;
    //public GameObject weaponSlot2;
    public int evade;
    public int scanner;
    public int jumpdrive;
    public bool canFlee;
}
