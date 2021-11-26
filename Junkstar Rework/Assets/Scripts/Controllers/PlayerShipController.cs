using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController : MonoBehaviour
{
    public Transform airlock;
    public List<GameObject> playerShipTiles;
    private ShipMapProps shipMapProps;

    private void Start()
    {
        NewGameSetup();
    }

    void NewGameSetup()
    {
        shipMapProps = GetComponent<ShipMapProps>();

        //Add all existing player ship tiles to the list
        foreach (GameObject tile in GameObject.FindGameObjectsWithTag("PlayerShipTile"))
        {
            playerShipTiles.Add(tile);
        }

        //Set the health
        shipMapProps.mapMaxHealth = playerShipTiles.Count;
        shipMapProps.mapCurHealth = shipMapProps.mapMaxHealth;
    }

}
