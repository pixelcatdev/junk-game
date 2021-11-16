using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Holds all generated map info; structure, power, enemy type etc
public class TargetShipController : MonoBehaviour
{
    public float mapCurHealth;
    public float mapMaxHealth;
    public float mapCritHealth;
    public float mapPower;
    public Image img_healthBar;

    public List<GameObject> shipTiles;
    public Transform playerAirlock;

    public string shipName;
    public string size;
    public string quality;
    private float qualityPer;
    public string enemy;

    public bool playerShipIsDocked;
    public bool playerIsBoarded;
    private bool isCollapsing;
    public float collapseInterval;
    private float collapseTimer;

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
            CollapseMap();
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
            tile.GetComponent<TileProps>().DestroyTile(false, false);
        }

        mapCritHealth = Random.Range(10, 20); //Mathf.RoundToInt(mapCurHealth * 0.05f);
        mapCurHealth = shipTiles.Count;

        Debug.Log(shipName + ": Quality damage at " + qualityPer * 100f + "%, total Ship Health is " + mapMaxHealth + ", destroying " + mapMaxHealth * qualityPer + " tiles. Ship will break apart if less than " + mapCritHealth + "tiles remain");

        //spawn x objects based on quality

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
                if (!isCollapsing)
                {
                    isCollapsing = true;
                }
            }
            img_healthBar.fillAmount = (mapCurHealth / mapMaxHealth);
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
        if (playerShipIsDocked)
        {
            PlayerController.instance.transform.position = GameObject.FindGameObjectWithTag("ShipAirlock").transform.position;
            playerIsBoarded = true;
        }
        else
        {
            Debug.Log("Cannot use airlock when ship is not docked");
        }        
    }

    //Move player to airlock on player ship
    public void ExitShip()
    {
        if (isCollapsing)
        {
            isCollapsing = false;
            MapClear();
            playerShipIsDocked = false;
        }
        PlayerController.instance.transform.position = playerAirlock.position;
        playerIsBoarded = false;
    }

    //Destroy the map tile by tile
    void CollapseMap()
    {
        if (isCollapsing)
        {
            if (collapseTimer < collapseInterval)
            {
                collapseTimer += Time.deltaTime;
            }
            else
            {
                //Every second, destroy a remaining ship tile
                if (mapCurHealth > 0)
                {
                    //select a random tile
                    int randomTile = Random.Range(0, shipTiles.Count - 1);
                    GameObject tile = shipTiles[randomTile];
                    tile.GetComponent<TileProps>().DestroyTile(false, true);
                    collapseTimer = 0;
                }
                //if there are no ship tiles left, if the player is boarded, kill them, otherwise destroy the map
                else
                {
                    if (!playerIsBoarded)
                    {
                        isCollapsing = false;
                        MapClear();
                    }
                    else
                    {
                        Debug.Log("Game Over - didn't escape the ship in time");
                    }
                }
            }
        }
    }
}
