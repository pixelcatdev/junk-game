using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject dungeonWorld;
    public GameObject overWorld;

    public Transform player;

    //public bool playerShipIsDocked;
    public GameObject overWorldMap;
    public GameObject playerShipMap;
    public GameObject targetShipMap;

    public static GameController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!GameController.instance)
        {
            GameController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

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
}
