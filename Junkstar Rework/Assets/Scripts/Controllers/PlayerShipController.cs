using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerShipController : MonoBehaviour
{
    public Transform airlock;
    public List<GameObject> playerShipTiles;
    private ShipMapProps shipMapProps;
    public TextMeshProUGUI txt_ShipHealth;
    public TextMeshProUGUI txt_ShipFuel;

    public List<GameObject> effects_ion;

    public static PlayerShipController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!PlayerShipController.instance)
        {
            PlayerShipController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

    }

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

        //Demolish X random tiles
        for (int i = 0; i < 5; i++)
        {
            //select a random tile
            int randomTile = Random.Range(0, playerShipTiles.Count - 1);
            GameObject tile = playerShipTiles[randomTile];
            tile.GetComponent<TileProps>().DestroyObject(true, false);
        }
    }

    private void Update()
    {
        txt_ShipHealth.text = "SHIP: " + Mathf.RoundToInt((shipMapProps.mapCurHealth / shipMapProps.mapMaxHealth) * 100).ToString() + "%";
        txt_ShipFuel.text = "FUEL: " + Mathf.RoundToInt((shipMapProps.mapCurFuel / 100) * 100).ToString() + "%";
    }

    void ShipHealth()
    {
        if(shipMapProps.mapCurHealth <= 0)
        {
            Debug.Log("Game over - your ship was destroyed");
        }
    }

}
