using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootProps : MonoBehaviour
{
    public float destroyTimer;
    public Sprite lootSprite;
    public string lootName;
    public string lootTxt;
    public int lootQty;
    public int lootStack;
    public int lootValue;

    private bool playerNearby;
    public bool vacuumLoot;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("DestroyLoot");
        lootSprite = transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
    }

    private void Update()
    {
        //Move the item towards the player when spawned
        if (playerNearby == true)
        {
            vacuumLoot = true;
        }

        if (vacuumLoot == true)
        {
            transform.position = Vector2.MoveTowards(transform.position, PlayerController.instance.transform.position, 5 * Time.deltaTime);
            float distance = Vector2.Distance(transform.position, PlayerController.instance.transform.position);
            if (distance < 0.25f)
            {
                InventoryController.instance.AddItem(gameObject, lootSprite, lootName, lootTxt, lootStack);
                vacuumLoot = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            playerNearby = false;
        }
    }

    IEnumerator DestroyLoot()
    {
        yield return new WaitForSeconds(destroyTimer);
        Destroy(gameObject);
    }
}
