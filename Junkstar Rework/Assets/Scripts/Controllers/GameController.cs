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

    public enum GameState { menu, game, overworld }
    public GameState gameState;

    public GameObject ui_overworld;
    public GameObject ui_game;

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

        gameState = GameState.game;
    }

    public void SwitchToOverworld()
    {
        GameController.instance.gameState = GameController.GameState.overworld;
        Camera.main.GetComponent<CameraProps>().target = overWorldMap;

        ui_overworld.SetActive(true);
        ui_game.SetActive(false);
    }

    public void SwitchToGame()
    {
        GameController.instance.gameState = GameController.GameState.game;
        Camera.main.GetComponent<CameraProps>().target = player.gameObject;

        ui_overworld.SetActive(false);
        ui_game.SetActive(true);
    }
}
