using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Holds all generated map info; structure, power, enemy type etc
public class TargetShipController : MonoBehaviour
{
    public float mapCurHealth;
    public float mapMaxHealth;
    public float mapPower;
    public enum EnemyType { raider, bot, mutant }
    public EnemyType enemy;

    public List<GameObject> shipTiles;
    public Transform playerAirlock;

    //Spawn Map
    //Small, Medium or Large
    //Apply damage based on quality
    //Apply crates based on quality
    //Apply explodables based on quality
    //Place spawners and set the enemy type accordingly

    public bool playerShipIsDocked;

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
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (playerShipIsDocked)
            {
                MapClear();
            }
            MapGen();
            Debug.Log("New target ship generated");
        }
    }

    //Generate a map, place it inside the targetShipMap reference
    public void MapGen()
    {
        generatedMap = Instantiate(shipsSmall[Random.Range(0, shipsSmall.Count - 1)], transform.position, transform.rotation, transform);
        //Get the map health by adding all ShipTile tagged objects to the shipTiles list
        foreach (GameObject tile in GameObject.FindGameObjectsWithTag("ShipTile"))
        {
            shipTiles.Add(tile);
        }
        mapMaxHealth = shipTiles.Count;

        //damage x tiles based on quality
        //Quality - poor, damage 75% of tiles / average - 50%, good - 25% (testing at 50%)
        for (int i = 0; i < mapMaxHealth / 2; i++)
        {
            //select a random tile
            //destroy it
            //reduce the current health by 1 each time
        }

        //spawn x loot based on quality
        playerShipIsDocked = true;
    }

    public void MapClear()
    {
        Destroy(generatedMap);
    }

    //Move player to airlock on target ship
    public void EnterShip()
    {
        PlayerController.instance.transform.position = GameObject.FindGameObjectWithTag("ShipAirlock").transform.position;
    }

    //Move player to airlock on player ship
    public void ExitShip()
    {
        PlayerController.instance.transform.position = playerAirlock.position;
    }

}
