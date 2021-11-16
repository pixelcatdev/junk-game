using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Holds all generated map info; structure, power, enemy type etc
public class TargetShipController : MonoBehaviour
{
    public int mapCurHealth;
    public int mapMaxHealth;
    public int mapCritHealth;
    public float mapPower;

    public List<GameObject> shipTiles;
    public Transform playerAirlock;

    public string shipName;
    public string size;
    public string quality;
    private float qualityPer;
    public string enemy;

    public bool playerShipIsDocked;
    public bool playerIsBoarded;

    public GameObject generatedMap;
    public List<GameObject> shipsSmall;
    public List<GameObject> shipsMedium;
    public List<GameObject> shipsLarge;

    public static TargetShipController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!TargetShipController.instance)
        {
            TargetShipController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        MapGen();
    }

    //Debug
    void Update()
    {
        if (GameController.instance.gameState == GameController.GameState.game)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                MapClear();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                MapGen();
            }

            MapHealth();
        }
    }

    //Generate a map, place it inside the targetShipMap reference
    public void MapGen()
    {
        //Instantiate right size ship       
        if (size == "small")
        {
            generatedMap = Instantiate(shipsSmall[Random.Range(0, shipsSmall.Count - 1)], transform.position, transform.rotation, transform);
        }
        else if (size == "medium")
        {
            generatedMap = Instantiate(shipsMedium[Random.Range(0, shipsMedium.Count - 1)], transform.position, transform.rotation, transform);
        }
        else if (size == "large")
        {
            generatedMap = Instantiate(shipsLarge[Random.Range(0, shipsLarge.Count - 1)], transform.position, transform.rotation, transform);
        }

        //Get the map health by adding all ShipTile tagged objects to the shipTiles list
        foreach (GameObject tile in GameObject.FindGameObjectsWithTag("ShipTile"))
        {
            shipTiles.Add(tile);
        }

        mapMaxHealth = shipTiles.Count;

        //Quality - poor, damage 75% of tiles / average - 50%, good - 25% (testing at 50%)
        if (quality == "poor")
        {
            qualityPer = 0.75f;
        }
        else if (quality == "average")
        {
            qualityPer = 0.5f;
        }
        else if (quality == "good")
        {
            qualityPer = 0.25f;
        }

        for (int i = 0; i < mapMaxHealth * qualityPer; i++)
        {
            //select a random tile
            int randomTile = Random.Range(0, shipTiles.Count - 1);
            GameObject tile = shipTiles[randomTile];
            //shipTiles.RemoveAt(randomTile);
            tile.GetComponent<TileProps>().DestroyTile(false);
        }       

        mapCritHealth = Random.Range(2, 10); //Mathf.RoundToInt(mapCurHealth * 0.05f);
        mapCurHealth = shipTiles.Count;

        Debug.Log(shipName + ": Quality damage at " + qualityPer * 100f + "%, total Ship Health is " + mapMaxHealth + ", destroying " + mapMaxHealth * qualityPer + " tiles. Ship will break apart if less than " + mapCritHealth + "tiles remain");

        //spawn x loot based on quality
        playerShipIsDocked = true;
        Debug.Log("Map generated");
    }

    //Monitor ships structure and power only when the player is aboard the target ship, trigger collapse or meltdown if either reaches critical
    public void MapHealth()
    {
        if (playerIsBoarded)
        {
            if (mapCurHealth < mapCritHealth)
            {
                Debug.Log("Map will break apart shortly");
            }
        }        
    }

    //Remove the instantiated ship map, and clear down any variables and lists relating to it
    public void MapClear()
    {
        Destroy(generatedMap);
        shipTiles.Clear();
        Debug.Log("Map cleared");
    }

    //Move player to airlock on target ship
    public void EnterShip()
    {
        PlayerController.instance.transform.position = GameObject.FindGameObjectWithTag("ShipAirlock").transform.position;
        playerIsBoarded = true;
    }

    //Move player to airlock on player ship
    public void ExitShip()
    {
        PlayerController.instance.transform.position = playerAirlock.position;
        playerIsBoarded = false;
    }    
}
