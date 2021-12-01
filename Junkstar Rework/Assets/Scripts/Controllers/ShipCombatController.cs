using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipCombatController : MonoBehaviour
{
    public float turnRate;
    private float turnTimer;
    public bool isBounty;
    public List<GameObject> lootList;
    public float escapeTimer;
    public Image escapeBar;
    public List<GameObject> enemyShips;

    public GameObject ui_ShipCombat;

    public GameObject enemyShip;
    public GameObject playerShip;
    private ShipMapProps enemyShipProps;
    private ShipMapProps playerShipProps;

    private bool inCombat;
    private int attackStage;

    public GameObject cameraTarget;

    private void Start()
    {
        enemyShipProps = enemyShip.GetComponent<ShipMapProps>();
        playerShipProps = playerShip.GetComponent<ShipMapProps>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Debug run ship combat");
            CombatSetup();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            CombatEnd();
        }

        if (inCombat)
        {
            Debug.Log("Attack stage: " + attackStage);
            SelectWeapon();
            SelectTarget();
        }
    }

    public void CombatSetup()
    {
        inCombat = true;
        ui_ShipCombat.SetActive(true);
        GameController.instance.gameState = GameController.GameState.spacecombat;
        GameController.instance.gameCursor.GetComponent<CursorProps>().cursorType = CursorProps.CursorType.shipCombat;
        //Scale the camera out
        CameraController.instance.target = cameraTarget;
        CameraController.instance.ZoomCamera(15);

        Debug.Log("Generating combat");
        Instantiate(enemyShips[Random.Range(0, enemyShips.Count - 1)], enemyShip.transform.position, enemyShip.transform.rotation, enemyShip.transform);

        //based on difficulty
        //Set the health of the ship
        //Set the weapon slots 
        //Set the drive level
        //Set the targeting level
        //Randomise who goes first

        Debug.Log("Combat generated");
    }

    public void CombatEnd()
    {
        inCombat = false;
        ui_ShipCombat.SetActive(false);
        Destroy(enemyShip.transform.GetChild(0).gameObject);
        CameraController.instance.ZoomCamera(8);
        CameraController.instance.target = PlayerController.instance.gameObject;
        GameController.instance.gameState = GameController.GameState.game;
        GameController.instance.gameCursor.GetComponent<CursorProps>().cursorType = CursorProps.CursorType.select;
    }

    public void CombatAttack()
    {
        Debug.Log("Select a weapon");
        attackStage = 1;

        //int d20 = Random.Range(1, 20);
        //int turnAttack = d20 + playerShipProps.targeting;

        //if (turnAttack > enemyShipProps.drive)
        //{
        //    //enemyHp -= playerDamage;
        //    Debug.Log("Your blasts hit the enemy");
        //}
        //else
        //{
        //    Debug.Log("Your blasts miss the enemy");
        //}
        //EnemyTurn();
    }

    public void CombatFlee()
    {
        int playerTotal = Random.Range(1, 20) + playerShipProps.drive;
        int enemyTotal = Random.Range(1, 20) + enemyShipProps.drive;
        Debug.Log("Player Flee: " + playerTotal + " Enemy Flee: " + enemyTotal);

        if (playerTotal > enemyTotal)
        {
            Debug.Log("You outrun your enemy.");
            CombatEnd();
        }
        else
        {
            Debug.Log("You fail to outrun your enemy.");
            EnemyTurn();
        }
    }

    private void EnemyTurn()
    {
        if (enemyShipProps.hull > Mathf.RoundToInt(enemyShipProps.hullMax / 4))
        {
            EnemyAttackPlayer();
        }
        else
        {
            //Try to flee
            int fleeChance = Random.Range(0, 100);
            if (fleeChance > 50)
            {
                int playerTotal = Random.Range(1, 20) + playerShipProps.drive;
                int enemyTotal = Random.Range(1, 20) + enemyShipProps.drive;
                Debug.Log("Player Flee: " + playerTotal + " Enemy Flee: " + enemyTotal);

                if (enemyTotal > playerTotal)
                {
                    Debug.Log("The enemy flees.");
                }
                else
                {
                    Debug.Log("The enemy attempts to flee, but fails.");
                }
            }
            else
            {
                EnemyAttackPlayer();
            }
        }
    }

    public void EnemyAttackPlayer()
    {
        int d20 = Random.Range(1, 20);
        int turnAttack = d20 + enemyShipProps.targeting;

        if (turnAttack > playerShipProps.drive)
        {
            Debug.Log("You take a hit!");
        }
        else
        {
            Debug.Log("The enemy misses!");
        }
    }

    void SelectWeapon()
    {
        if (attackStage == 1)
        {
            LayerMask hitLayer = LayerMask.GetMask("Object");
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0f, hitLayer);

            if (hit)
            {
                if (hit.transform.gameObject.GetComponent<ShipWeaponProps>())
                {
                    Debug.Log(hit.transform.name + " selected.");
                    GameController.instance.selectorShipWeapon.SetActive(true);
                    GameController.instance.selectorShipWeapon.transform.position = hit.transform.position;

                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        Debug.Log("Select target");
                        GameController.instance.selectorShipWeapon.SetActive(false);
                        attackStage = 2;
                    }                    
                }
                else
                {
                    GameController.instance.selectorShipWeapon.SetActive(false);
                }
            }
            else
            {
                GameController.instance.selectorShipWeapon.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                attackStage = 0;
                GameController.instance.selectorShipWeapon.SetActive(false);
                Debug.Log("Cancelling Attack");
            }
        }
    }

    void SelectTarget()
    {
        if (attackStage == 2)
        {
            LayerMask hitLayer = LayerMask.GetMask("ShipTile");
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0f, hitLayer);

            if (hit)
            {
                if (hit.transform.tag == "ShipTile")
                {
                    Debug.Log(hit.transform.name + " targeted");
                    GameController.instance.selectorShipTarget.SetActive(true);
                    GameController.instance.selectorShipTarget.transform.position = hit.transform.position;

                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        Debug.Log("Firing");
                        GameController.instance.selectorShipTarget.SetActive(false);
                        //Launch projectile
                        attackStage = 0;
                    }
                }
                else
                {
                    GameController.instance.selectorShipTarget.SetActive(false);
                }
            }
            else
            {
                GameController.instance.selectorShipTarget.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                attackStage = 0;
                GameController.instance.selectorShipTarget.SetActive(false);
                Debug.Log("Cancelling Attack");
            }
        }
    }
}
