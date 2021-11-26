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
    public List<GameObject> spawnOnDestroy;
    public List<LootSlot> lootList;

    public bool isOccupied;
    public bool isTakingDamage;
    public float dmgAmount;

    private void Start()
    {
        curHealth = maxHealth;
    }

    private void Update()
    {
        //TakingDamage();
    }

    public void TakeDamage(float damage, bool isShotAt)
    {
        if (canDestroy == true)
        {
            //Deduct object health until destroyed
            if (curHealth - damage > 0)
            {
                curHealth -= damage;

                if (isShotAt == true)
                {
                    //StartCoroutine("FlashDamage");
                }
            }
            else
            {
                DestroyObject(isShotAt, true);
            }
        }
        else
        {
            Debug.Log("Cannot damage");
        }
    }

    public void DestroyObject(bool isShotAt, bool camShake)
    {
        //Clear any UI elements on the object
        //ui_tile.SetActive(false);

        //Spawn object on destroy
        foreach (GameObject spawnObj in spawnOnDestroy)
        {
            GameObject newObj = Instantiate(spawnObj, transform.position, transform.rotation, transform.parent);
        }

        //Spawn loot only if it was destroyed by the cutter
        if (isShotAt == false)
        {
            for (int i = 0; i < lootList.Count; i++)
            {
                int lootQty = Random.Range(lootList[i].min, lootList[i].max);
                for (int qty = 0; qty < lootQty; qty++)
                {
                    Vector2 newPos = new Vector2(transform.position.x, transform.position.y) + Random.insideUnitCircle * 1;
                    GameObject newObj = Instantiate(lootList[i].loot, newPos, transform.rotation, transform.parent);
                }
            }

            //Return loot if it's a player building
            if (GetComponent<BuildingProps>())
            {
                Debug.Log("Returning player loot");
                List<BuildingRecipe> buildingResources = new List<BuildingRecipe>();
                buildingResources = GetComponent<BuildingProps>().buildingRecipe;

                foreach (BuildingRecipe resource in buildingResources)
                {
                    int lootQty = resource.itemCost;
                    for (int qty = 0; qty < lootQty; qty++)
                    {
                        Vector2 newPos = new Vector2(transform.position.x, transform.position.y) + Random.insideUnitCircle * 1;
                        GameObject newObj = Instantiate(resource.lootObject, newPos, transform.rotation, transform.parent);
                    }                               
                }
            }
        }

        //Shake camera
        if (camShake)
        {
            Camera.main.GetComponent<Animator>().Play("Shake_Small", -1, 0f);
        }

        //Remove the tile from the Target Ships tilelist
        TargetShipController.instance.shipTiles.Remove(gameObject);

        if (tag == "ShipTileFloor" || tag == "ShipTileWall")
        {
            TargetShipController.instance.mapCurHealth -= 1;
        }

        //If it's a player built object, return all the loot and remove it from the list of player buildings
        if (GetComponent<BuildingProps>())
        {
            //If this is a player built object, remove it from the list of player built objects and deduct the power cost if possible
            if (PlayerController.instance.playerBuildings.Contains(gameObject))
            {
                PlayerController.instance.playerBuildings.Remove(gameObject);
                transform.parent.GetComponent<TileProps>().canDestroy = true;
                transform.parent.GetComponent<TileProps>().isOccupied = false;
            }
        }

        //Player.Instance.beamObj.SetActive(false);
        GameController.instance.selectorDestroy.SetActive(false);
        PlayerController.instance.isDestroying = false;
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