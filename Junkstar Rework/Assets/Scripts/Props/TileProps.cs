using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Environmental tiles that can be damaged
public class TileProps : MonoBehaviour
{
    public bool canDestroy;
    public float curHealth;
    public float maxHealth;
    public GameObject ui_tile;
    public Image healthBar;
    public List<GameObject> spawnOnDestroy;
    public List<GameObject> lootList;

    public bool isTakingDamage;
    public float dmgAmount;

    private void Start()
    {
        curHealth = maxHealth;
    }

    private void Update()
    {
        TakingDamage();
    }

    //Damage the tile, destroy if below zero and place whatever objects are found in spawnOnDestroy
    public void TakingDamage()
    {
        if (canDestroy && isTakingDamage)
        {
            //ui_tile.SetActive(true);
            healthBar.fillAmount = curHealth / maxHealth;

            if (curHealth - dmgAmount > 0)
            {
                curHealth -= dmgAmount;
                
            }
            else
            {
                foreach (GameObject spawnObj in spawnOnDestroy)
                {
                    GameObject newObj = Instantiate(spawnObj, transform.position, transform.rotation, transform.parent);
                }

                Destroy(gameObject);
            }
        }
        else
        {
            //ui_tile.SetActive(false);
        }
    }
}
