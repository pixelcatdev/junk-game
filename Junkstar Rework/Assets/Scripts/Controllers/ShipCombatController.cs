using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public TextMeshProUGUI txt_SelectedWeapon;

    public GameObject enemyShip;
    public GameObject playerShip;
    private ShipMapProps enemyShipProps;
    private ShipMapProps playerShipProps;

    private bool inCombat;
    private int attackStage;
    public GameObject playerChosenWeapon;
    public GameObject enemyChosenWeapon;
    private GameObject playerTargetObject;
    private GameObject enemyTargetObject;

    private float evadeBonus;

    public GameObject cameraTarget;

    public static ShipCombatController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!ShipCombatController.instance)
        {
            ShipCombatController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

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
        enemyShipProps.weaponSlot1 = GameObject.FindGameObjectWithTag("EnemyWeaponSlot1");
        //enemyShipProps.weaponSlot2 = GameObject.FindGameObjectWithTag("EnemyWeaponSlot2");

        //Set the drive level
        //Set the targeting level
        //Randomise who goes first
        EnemyAttackPlayer();

        Debug.Log("Combat generated");
    }

    public void CombatEnd()
    {
        inCombat = false;
        ui_ShipCombat.SetActive(false);
        txt_SelectedWeapon.text = null;
        Destroy(enemyShip.transform.GetChild(0).gameObject);
        CameraController.instance.ZoomCamera(8);
        CameraController.instance.target = PlayerController.instance.gameObject;
        GameController.instance.gameState = GameController.GameState.game;
        GameController.instance.gameCursor.GetComponent<CursorProps>().cursorType = CursorProps.CursorType.select;
    }

    public void CombatAttack()
    {
        txt_SelectedWeapon.text = "(SELECT WEAPON SLOT)";
        attackStage = 1;
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
            //EnemyTurn();
        }
    }

    public void EnemyTurn()
    {
        if(attackStage == 3)
        {
            Debug.Log("Enemy turn");
            EnemyAttackPlayer();
        }        

        //if (enemyShipProps.hull > Mathf.RoundToInt(enemyShipProps.hullMax / 4))
        //{
        //    EnemyAttackPlayer();
        //}
        //else
        //{


        //    //Try to flee
        //    int fleeChance = Random.Range(0, 100);
        //    if (fleeChance > 50)
        //    {
        //        int playerTotal = Random.Range(1, 20) + playerShipProps.drive;
        //        int enemyTotal = Random.Range(1, 20) + enemyShipProps.drive;
        //        Debug.Log("Player Flee: " + playerTotal + " Enemy Flee: " + enemyTotal);

        //        if (enemyTotal > playerTotal)
        //        {
        //            Debug.Log("The enemy flees.");
        //        }
        //        else
        //        {
        //            Debug.Log("The enemy attempts to flee, but fails.");
        //        }
        //    }
        //    else
        //    {
        //        EnemyAttackPlayer();
        //    }
        //}
    }

    public void EnemyAttackPlayer()
    {
        //Select a weapon
        //enemyShipProps.weaponSlot1

        enemyTargetObject = null;

        enemyChosenWeapon = enemyShipProps.weaponSlot1;

        GameController.instance.selectorShipTarget.SetActive(false);

        int d20 = Random.Range(1, 20);
        int turnAttack = d20 + playerShipProps.targeting;
        bool willHitTarget = false;

        //Find a random player ship tile
        enemyTargetObject = PlayerShipController.instance.playerShipTiles[Random.Range(0, PlayerShipController.instance.playerShipTiles.Count - 1)];

        if (turnAttack > playerShipProps.drive)
        {
            willHitTarget = true;
            Debug.Log("Enemy blast will hit you");
        }
        else
        {
            willHitTarget = false;
            Debug.Log("Enemy blast will miss you");
        }

        GameObject refProjectile = enemyChosenWeapon.GetComponent<ShipWeaponProps>().projectile;
        Vector2 weaponPos = enemyChosenWeapon.transform.position;

        var q = Quaternion.FromToRotation(Vector3.up, enemyTargetObject.transform.position - enemyChosenWeapon.transform.position);
        GameObject enemyProjectile = Instantiate(refProjectile, weaponPos, q, playerShip.transform);

        enemyProjectile.GetComponent<ShipProjectileProps>().targetObject = enemyTargetObject;
        enemyProjectile.GetComponent<ShipProjectileProps>().willHitTarget = willHitTarget;
        enemyProjectile.GetComponent<ShipProjectileProps>().projectileType = ShipProjectileProps.ProjectileType.enemy;
        enemyProjectile.GetComponent<Rigidbody2D>().AddForce(enemyProjectile.transform.up * enemyChosenWeapon.GetComponent<ShipWeaponProps>().speed, ForceMode2D.Impulse);

        attackStage = 0;
        evadeBonus = 0;
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
                    txt_SelectedWeapon.text = "(" + hit.transform.gameObject.GetComponent<ShipWeaponProps>().weaponName + ")";

                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        GameController.instance.selectorShipWeapon.SetActive(false);
                        playerChosenWeapon = hit.transform.gameObject;
                        attackStage = 2;
                    }                    
                }
                else
                {
                    //GameController.instance.selectorShipWeapon.SetActive(false);
                    //txt_SelectedWeapon.text = null;
                }
            }
            else
            {
                //GameController.instance.selectorShipWeapon.SetActive(false);
                //txt_SelectedWeapon.text = null;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                attackStage = 0;
                GameController.instance.selectorShipWeapon.SetActive(false);
                txt_SelectedWeapon.text = "(SELECT WEAPON SLOT)";
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

                        int d20 = Random.Range(1, 20);
                        int turnAttack = d20 + playerShipProps.targeting;
                        bool willHitTarget = false;
                        playerTargetObject = hit.transform.gameObject;

                        if (turnAttack > enemyShipProps.drive)
                        {
                            willHitTarget = true;
                            Debug.Log("Your blast will hit the enemy");
                        }
                        else
                        {
                            willHitTarget = false;
                            Debug.Log("Your blast will miss the enemy");
                        }

                        GameObject refProjectile = playerChosenWeapon.GetComponent<ShipWeaponProps>().projectile;
                        Vector2 weaponPos = playerChosenWeapon.transform.position;

                        var q = Quaternion.FromToRotation(Vector3.up, playerTargetObject.transform.position - playerChosenWeapon.transform.position);

                        for (int i = 0; i < playerChosenWeapon.GetComponent<ShipWeaponProps>().shotQty; i++)
                        {
                            GameObject projectile = Instantiate(refProjectile, weaponPos, q, playerShip.transform);

                            projectile.GetComponent<ShipProjectileProps>().targetObject = playerTargetObject;
                            projectile.GetComponent<ShipProjectileProps>().willHitTarget = willHitTarget;
                            projectile.GetComponent<ShipProjectileProps>().projectileType = ShipProjectileProps.ProjectileType.player;
                            projectile.GetComponent<Rigidbody2D>().AddForce(projectile.transform.up * playerChosenWeapon.GetComponent<ShipWeaponProps>().speed, ForceMode2D.Impulse);
                        }                        

                        attackStage = 3;
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
                txt_SelectedWeapon.text = "(SELECT WEAPON SLOT)";
                Debug.Log("Cancelling Attack");
            }
        }
    }

    public void CombatEvade()
    {
        txt_SelectedWeapon.text = "(TAKING EVASIVE MANOUVRES)";
        evadeBonus = 5;
        attackStage = 3;
        EnemyTurn();
    }

}
