using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectorProps : MonoBehaviour
{
    public GameObject targetTile;
    public Image healthBar;
    public Image buildingBlueprint;

    // Update is called once per frame
    void Update()
    {
        //If there's a healthbar, set the fill to the current health percentage of the target Tile
        if (healthBar != null)
        {
            healthBar.fillAmount = targetTile.GetComponent<TileProps>().curHealth / targetTile.GetComponent<TileProps>().maxHealth;
        }        
    }
}
