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
    public GameObject ui_shipCombatDisable;
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
    private bool enemyExit;

    public GameObject effect_shipExplode;
    public GameObject effect_cloud;

    private int evadeBonus;

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
            CombatEnd(false);
        }

        if (inCombat)
        {
            Debug.Log("Attack stage: " + attackStage);
            SelectWeapon();
            SelectTarget();
            ExitEnemyShip();
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
        List<GameObject> enemyShipTiles = new List<GameObject>();

        foreach (GameObject tile in GameObject.FindGameObjectsWithTag("ShipTile"))
        {
            enemyShipTiles.Add(tile);
        }

        GameObject.FindGameObjectsWithTag("ShipTile");
        enemyShipProps.mapMaxHealth = enemyShipTiles.Count;
        Debug.Log("Hp: " + enemyShipTiles.Count);
        enemyShipProps.mapCurHealth = enemyShipProps.mapMaxHealth;

        //Set the weapon slots 
        enemyShipProps.weaponSlot1 = GameObject.FindGameObjectWithTag("EnemyWeaponSlot1");
        //enemyShipProps.weaponSlot2 = GameObject.FindGameObjectWithTag("EnemyWeaponSlot2");

        //Set the drive level
        //Set the targeting level
        //Randomise who goes first

        Debug.Log("Combat generated");
    }

    public void CombatEnd(bool hasWon)
    {
        //If the player has won, blow up the target ship
        if (hasWon)
        {
            StartCoroutine("EnemyShipExplode");
        }
        else
        {
            //Make ship fly away
            StartCoroutine("EnemyShipFlee");
        }
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
            CombatEnd(false);
        }
        else
        {
            Debug.Log("You fail to outrun your enemy.");
            //EnemyTurn();
        }
    }

    public void EnemyTurn()
    {
        if (attackStage == 3)
        {
            if (enemyShipProps.mapCurHealth / enemyShipProps.mapMaxHealth > 0.5f)
            {
                EnemyAttackPlayer();
            }
            else
            {
                if (enemyShipProps.canFlee)
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
                            StartCoroutine("EnemyShipFlee");
                        }
                        else
                        {
                            Debug.Log("The enemy attempts to flee, but fails.");
                            EndEnemyTurn();
                        }
                    }
                    else
                    {
                        EnemyAttackPlayer();
                    }
                }
                else
                {
                    EnemyAttackPlayer();
                }
            }
        }
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

        try
        {
            if (turnAttack > playerShipProps.drive)
            {
                willHitTarget = true;
            }
            else
            {
                willHitTarget = false;
            }

            GameObject refProjectile = enemyChosenWeapon.GetComponent<ShipWeaponProps>().projectile;
            Vector2 weaponPos = enemyChosenWeapon.transform.position;

            var q = Quaternion.FromToRotation(Vector3.up, enemyTargetObject.transform.position - enemyChosenWeapon.transform.position);
            GameObject enemyProjectile = Instantiate(refProjectile, weaponPos, q, playerShip.transform);

            enemyProjectile.GetComponent<ShipProjectileProps>().targetObject = enemyTargetObject;
            enemyProjectile.GetComponent<ShipProjectileProps>().willHitTarget = willHitTarget;
            enemyProjectile.GetComponent<ShipProjectileProps>().projectileType = ShipProjectileProps.ProjectileType.enemy;
            enemyProjectile.GetComponent<Rigidbody2D>().AddForce(enemyProjectile.transform.up * enemyChosenWeapon.GetComponent<ShipWeaponProps>().speed, ForceMode2D.Impulse);
        }
        catch (System.Exception)
        {
            Debug.Log(enemyTargetObject.name);
            throw;
        }

        

        attackStage = 0;

        playerShipProps.drive -= evadeBonus;
        evadeBonus = 0;        
    }

    void SelectWeapon()
    {
        if (attackStage == 1)
        {
            ui_shipCombatDisable.SetActive(false);
            LayerMask hitLayer = LayerMask.GetMask("Object");
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0f, hitLayer);

            if (hit)
            {
                if (hit.transform.gameObject.GetComponent<ShipWeaponProps>())
                {
                    if (hit.transform.gameObject.GetComponent<ShipWeaponProps>().curCooldown == 0 && hit.transform.gameObject.GetComponent<ShipWeaponProps>().canSelect)
                    {
                        Debug.Log(hit.transform.name + " selected.");
                        GameController.instance.selectorShipWeapon.SetActive(true);
                        GameController.instance.selectorShipWeapon.transform.position = hit.transform.position;
                        txt_SelectedWeapon.text = "(" + hit.transform.gameObject.GetComponent<ShipWeaponProps>().weaponName + ")";

                        if (Input.GetKeyDown(KeyCode.Mouse0))
                        {
                            GameController.instance.selectorShipWeapon.SetActive(false);
                            playerChosenWeapon = hit.transform.gameObject;
                            txt_SelectedWeapon.text = "(SELECT TARGET)";
                            attackStage = 2;
                        }
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
                txt_SelectedWeapon.text = "(CHOOSE ACTION)";
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
                    //Debug.Log(hit.transform.name + " targeted");
                    GameController.instance.selectorShipTarget.SetActive(true);
                    GameController.instance.selectorShipTarget.transform.position = hit.transform.position;

                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        //Debug.Log("Firing");
                        ui_shipCombatDisable.SetActive(true);

                        if (playerChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().cooldown > 0)
                        {
                            playerChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().ui_cooldown.SetActive(true);
                            playerChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().curCooldown = playerChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().cooldown;
                            playerChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().txt_cooldown.text = playerChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().cooldown.ToString();
                        }                        

                        GameController.instance.selectorShipTarget.SetActive(false);

                        int d20 = Random.Range(1, 20);
                        int turnAttack = d20 + playerShipProps.targeting;
                        bool willHitTarget = false;
                        playerTargetObject = hit.transform.gameObject;

                        if (turnAttack > enemyShipProps.drive)
                        {
                            willHitTarget = true;
                        }
                        else
                        {
                            willHitTarget = false;
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
                txt_SelectedWeapon.text = "(CHOOSE ACTION)";
                //Debug.Log("Cancelling Attack");
            }
        }
    }

    public void CombatEvade()
    {
        txt_SelectedWeapon.text = "(TAKING EVASIVE MANOUVRES)";
        evadeBonus = 5;
        playerShipProps.drive += evadeBonus;
        attackStage = 3;
        //EnemyTurn();
        EndPlayerTurn();
    }

    public void EndPlayerTurn()
    {
        StartCoroutine("PlayerTurnDelay", 1.5f);
    }

    public void EndEnemyTurn()
    {
        Debug.Log("Ending enemy turn");
        UpdateWeaponCooldowns();
        StartCoroutine("EnemyTurnDelay", 1.5f);
    }

    void UpdateWeaponCooldowns()
    {
        ShipWeaponProps playerShipWeapon1 = playerShipProps.weaponSlot1.gameObject.GetComponent<ShipWeaponProps>();
        ShipWeaponProps playerShipWeapon2 = playerShipProps.weaponSlot2.gameObject.GetComponent<ShipWeaponProps>();

        Debug.Log(playerShipWeapon1);
        Debug.Log(playerShipWeapon2);

        if (playerShipProps.weaponSlot1 != null)
        {
            if (playerShipWeapon1.curCooldown > 1)
            {
                Debug.Log("Reducing weapon 1 cooldown");
                playerShipWeapon1.curCooldown -= 1;
                playerShipWeapon1.txt_cooldown.text = playerShipWeapon1.curCooldown.ToString();
            }
            else
            {
                playerShipWeapon1.curCooldown = 0;
                playerShipWeapon1.ui_cooldown.SetActive(false);
            }
        }

        if (playerShipProps.weaponSlot2 != null)
        {
            if (playerShipWeapon2.curCooldown > 1)
            {
                Debug.Log("Reducing weapon 2 cooldown");
                playerShipWeapon2.curCooldown -= 1;
                playerShipWeapon2.txt_cooldown.text = playerShipWeapon2.curCooldown.ToString();
            }
            else
            {
                playerShipWeapon2.curCooldown = 0;
                playerShipWeapon2.ui_cooldown.SetActive(false);
            }
        }
    }

    void ExitEnemyShip()
    {
        if (enemyExit)
        {
            Debug.Log("X");
            Vector2 exitPos = new Vector2(100f, 0f);
            enemyShip.transform.GetChild(0).transform.position = Vector2.MoveTowards(enemyShip.transform.GetChild(0).transform.position, exitPos, 25 * Time.deltaTime);
        }        
    }
   
    IEnumerator PlayerTurnDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnemyTurn();
    }

    IEnumerator EnemyTurnDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        //Resetting back to player turn
        txt_SelectedWeapon.text = "(CHOOSE ACTION)";
        ui_shipCombatDisable.SetActive(false);
        attackStage = 0;
    }

    IEnumerator EnemyShipExplode()
    {
        for (int i = 0; i < 5; i++)
        {

            Vector2 newPos = new Vector2(enemyShip.transform.position.x, enemyShip.transform.position.y) + Random.insideUnitCircle * 6f;
            GameObject explosionEffect = Instantiate(effect_cloud, newPos, transform.rotation, transform.GetChild(0));
            explosionEffect.transform.localScale *= 7.5f;
            explosionEffect.GetComponent<DestroyObjectProps>().destroyTimer = 0.25f;
            yield return new WaitForSeconds(0.15f);
        }
        Destroy(enemyShip.transform.GetChild(0).gameObject);
        Instantiate(effect_shipExplode, enemyShip.transform.position, transform.rotation, transform);

        yield return new WaitForSeconds(1.5f);

        inCombat = false;
        ui_ShipCombat.SetActive(false);
        txt_SelectedWeapon.text = null;

        CameraController.instance.ZoomCamera(8);
        CameraController.instance.target = PlayerController.instance.gameObject;
        CameraController.instance.JumpToTarget();
        GameController.instance.gameState = GameController.GameState.game;
        GameController.instance.gameCursor.GetComponent<CursorProps>().cursorType = CursorProps.CursorType.select;
    }

    IEnumerator EnemyShipFlee()
    {
        Debug.Log("Enemy Ship fleeing");
        enemyExit = true;
        yield return new WaitForSeconds(2f);

        Destroy(enemyShip.transform.GetChild(0).gameObject);

        inCombat = false;
        ui_ShipCombat.SetActive(false);
        txt_SelectedWeapon.text = null;

        CameraController.instance.ZoomCamera(8);
        CameraController.instance.target = PlayerController.instance.gameObject;
        CameraController.instance.JumpToTarget();
        GameController.instance.gameState = GameController.GameState.game;
        GameController.instance.gameCursor.GetComponent<CursorProps>().cursorType = CursorProps.CursorType.select;
        enemyExit = false;
    }

}
