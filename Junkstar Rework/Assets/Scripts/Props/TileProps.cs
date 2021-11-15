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
    public List<LootSlot> lootList;

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
                DestroyTile();
            }
        }
        else
        {
            //ui_tile.SetActive(false);
        }
    }

    //Destroy the tile and spawn any loot/trigger any effects
    public void DestroyTile()
    {
        Debug.Log("Destroying");
        //Spawn object on destroy
        foreach (GameObject spawnObj in spawnOnDestroy)
        {
            GameObject newObj = Instantiate(spawnObj, transform.position, transform.rotation, transform.parent);
        }

        //Spawn loot on destroy
        for (int i = 0; i < lootList.Count; i++)
        {
            Debug.Log(i);
            int lootQty = Random.Range(lootList[i].min, lootList[i].max);
            for (int qty = 0; qty < lootQty; qty++)
            {
                Vector2 newPos = new Vector2(transform.position.x,transform.position.y) + Random.insideUnitCircle * 1;
                GameObject newObj = Instantiate(lootList[i].loot, newPos, transform.rotation, transform.parent);
            }            
        }

        //GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().isDestroying = false;
        Destroy(gameObject);
    }
}

[System.Serializable]
public class LootSlot
{
    public GameObject loot;
    public int min;
    public int max;
}