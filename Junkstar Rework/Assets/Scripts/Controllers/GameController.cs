using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject dungeonWorld;
    public GameObject overWorld;

    public Transform player;

    public bool playerShipIsDocked;
    public GameObject playerShipMap;
    public GameObject targetShipMap;

    public GameObject generatedMap;
    public List<GameObject> shipsSmall;
    public List<GameObject> shipsMedium;
    public List<GameObject> shipsLarge;

    //If dungeon is active, disable it and enable overworld and vice versa
    public void ToggleWorlds()
    {
        if (dungeonWorld.activeInHierarchy)
        {
            dungeonWorld.SetActive(false);
            overWorld.SetActive(true);
        }
        else
        {
            dungeonWorld.SetActive(true);
            overWorld.SetActive(false);
        }
    }

    //Toggles between the Junkstar and the target shipwreck
    public void EnterShip()
    {
        playerShipMap.SetActive(false);
        targetShipMap.SetActive(true);
        player.position = GameObject.FindGameObjectWithTag("ShipAirlock").transform.position;
    }

    public void ExitShip()
    {
        //Move player to airlock on player ship
        playerShipMap.SetActive(true);
        targetShipMap.SetActive(false);
        player.position = playerShipMap.GetComponent<PlayerShipController>().airlock.position;
    }

    //Generate a map, place it inside the targetShipMap reference
    public void MapGen()
    {
        generatedMap = Instantiate(shipsSmall[Random.Range(0, shipsSmall.Count - 1)], transform.position, transform.rotation, targetShipMap.transform);
        playerShipIsDocked = true;
    }

    public void MapClear()
    {
        Destroy(generatedMap);
    }
}
