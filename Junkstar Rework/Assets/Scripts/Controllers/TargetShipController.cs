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
    public List<GameObject> shipFloorTiles;
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

    public bool isStation;
    public GameObject stationMap;
    public GameObject generatedMap;    
    public List<GameObject> shipsSmall;
    public List<GameObject> shipsMedium;
    public List<GameObject> shipsLarge;

    public List<GameObject> shipObjects;

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
            MapHealth();
            CollapseMap();
        }
    }

    //Generate a map, place it inside the targetShipMap reference
    public void MapGen()
    {
        //If its the spacestation, spawn that
        if (isStation)
        {
            generatedMap = Instantiate(stationMap, transform.position, transform.rotation, transform);
        }
        else
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
            //Floors
            foreach (GameObject tile in GameObject.FindGameObjectsWithTag("ShipTileFloor"))
            {
                shipTiles.Add(tile);
                shipFloorTiles.Add(tile);
            }
            //Walls
            foreach (GameObject tile in GameObject.FindGameObjectsWithTag("ShipTileWall"))
            {
                shipTiles.Add(tile);
            }

            mapMaxHealth = shipTiles.Count;

            //Quality - poor, damage 75% of tiles / average - 50%, good - 25% (testing at 50%)
            if (quality == "poor")
            {
                qualityPer = 0.5f;
            }
            else if (quality == "average")
            {
                qualityPer = 0.25f;
            }
            else if (quality == "good")
            {
                qualityPer = 0.05f;
            }

            for (int i = 0; i < mapMaxHealth * qualityPer; i++)
            {
                //select a random tile
                int randomTile = Random.Range(0, shipTiles.Count - 1);
                GameObject tile = shipTiles[randomTile];
                //shipTiles.RemoveAt(randomTile);
                tile.GetComponent<TileProps>().DestroyObject(true, false);
            }

            //Spawn ship Objects on floor ShipTiles (clear after for optimization)
            for (int i = 0; i < mapMaxHealth * (qualityPer / 8); i++)
            {
                //select a random tile
                int randomTile = Random.Range(0, shipFloorTiles.Count - 1);
                GameObject tile = shipFloorTiles[randomTile];
                Instantiate(shipObjects[Random.Range(0, shipObjects.Count - 1)], tile.transform.position, tile.transform.rotation);
                shipFloorTiles.RemoveAt(randomTile);
            }
            shipFloorTiles.Clear();

            mapCritHealth = Random.Range(10, 20); //Mathf.RoundToInt(mapCurHealth * 0.05f);
            mapCurHealth = shipTiles.Count;

            //Debug.Log(shipName + ": Quality damage at " + qualityPer * 100f + "%, total Ship Health is " + mapMaxHealth + ", destroying " + mapMaxHealth * qualityPer + " tiles. Ship will break apart if less than " + mapCritHealth + "tiles remain");

            //spawn x enemy based on ship size and enemy type
        }


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
            CameraController.instance.JumpToTarget();
            //Enable helmet if not a station
            if (!isStation)
            {
                PlayerController.instance.helmet.SetActive(true);
                PlayerController.instance.hair.SetActive(false);
            }                     
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
        CameraController.instance.JumpToTarget();
        PlayerController.instance.helmet.SetActive(false);
        PlayerController.instance.hair.SetActive(true);
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
                    tile.GetComponent<TileProps>().TakeDamage(500f, true);
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
