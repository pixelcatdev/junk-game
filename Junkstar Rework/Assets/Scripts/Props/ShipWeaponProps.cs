using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipWeaponProps : MonoBehaviour
{
    public GameObject projectile;
    public string weaponName;
    public string weaponTxt;
    public float accuracy;
    public float speed;
    public int cooldown;
    public int curCooldown;
    public int shotQty;
    public GameObject ui_cooldown;
    public TextMeshProUGUI txt_cooldown;
}
