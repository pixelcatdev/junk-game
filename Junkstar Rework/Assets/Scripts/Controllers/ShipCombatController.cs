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
    public GameObject ui_enemyShipStats;
    public TextMeshProUGUI txt_EnemyShipName;
    public TextMeshProUGUI txt_EnemyShipHealth;

    public TextMeshProUGUI txt_SelectedWeapon;

    public List<Transform> spawnPositions;
    private Vector2 enemySpawnPos;
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
        if (inCombat)
        {
            SelectWeapon();
            SelectTarget();
            ExitEnemyShip();
            DisplayEnemyStats();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            CombatSetup();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            CombatEnd(false);
        }
    }

    public void DisplayEnemyStats()
    {
        //Only display if the players scanners are upgraded enough
        Debug.Log("Scanner: " + playerShipProps.scanner);
        if(playerShipProps.scanner > 50)
        {
            ui_enemyShipStats.SetActive(true);
            txt_EnemyShipName.text = enemyShipProps.shipName.ToUpper();
            Debug.Log("Enemy Ship HP: " + Mathf.RoundToInt((enemyShipProps.mapCurHealth / enemyShipProps.mapMaxHealth) * 100));
            txt_EnemyShipHealth.text = "SHIP: " + Mathf.RoundToInt((enemyShipProps.mapCurHealth / enemyShipProps.mapMaxHealth) * 100).ToString() + "%";
        }
        else
        {
            ui_enemyShipStats.SetActive(false);
        }
    }

    public void CombatSetup()
    {
        inCombat = true;
        ui_ShipCombat.SetActive(true);
        GameController.instance.SwitchToShipCombat();       

        Debug.Log("Generating combat");
        //randomise left or right
        Transform spawnPos = spawnPositions[Random.Range(0, spawnPositions.Count)];
        enemySpawnPos = spawnPos.position;

        //Scale the camera out

        Vector2 cameraTarget = new Vector2((enemySpawnPos.x - playerShip.transform.position.x) / 2, 0);
        CameraController.instance.target = playerShip;
        CameraController.instance.ZoomCamera(15);

        Instantiate(enemyShips[Random.Range(0, enemyShips.Count - 1)], enemySpawnPos, enemyShip.transform.rotation, enemyShip.transform);

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
        enemyShipProps.weaponSlots = enemyShip.transform.GetChild(0).GetComponent<ShipMapProps>().weaponSlots;

        //Set the evade level
        enemyShipProps.evade = playerShipProps.evade + Random.Range(-1, 2);
        //Randomise who goes first

        ui_shipCombatDisable.SetActive(false);
        attackStage = 0;

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
        int playerTotal = Random.Range(1, 20) + playerShipProps.evade;
        int enemyTotal = Random.Range(1, 20) + enemyShipProps.evade;
        Debug.Log("Player Flee: " + playerTotal + " Enemy Flee: " + enemyTotal);

        if (playerTotal > enemyTotal)
        {
            Debug.Log("You outrun your enemy.");
            CombatEnd(false);
        }
        else
        {
            Debug.Log("You fail to outrun your enemy.");
            ui_shipCombatDisable.SetActive(true);
            attackStage = 3;
            EndPlayerTurn();
        }
    }

    public void CombatEvade()
    {
        txt_SelectedWeapon.text = "(TAKING EVASIVE MANOUVRES)";
        evadeBonus = 5;
        playerShipProps.evade += evadeBonus;
        attackStage = 3;
        ui_shipCombatDisable.SetActive(true);
        EndPlayerTurn();
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
                        int playerTotal = Random.Range(1, 20) + playerShipProps.evade;
                        int enemyTotal = Random.Range(1, 20) + enemyShipProps.evade;
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

        //Randomise Enemy Weapon
        enemyChosenWeapon = EnemyRandomiseWeapon();

        GameController.instance.selectorShipTarget.SetActive(false);

        int d20 = Random.Range(1, 20);
        int turnAttack = d20 + enemyChosenWeapon.GetComponent<ShipWeaponProps>().accuracy;
        bool willHitTarget = false;

        //Find a random player ship tile
        enemyTargetObject = PlayerShipController.instance.playerShipTiles[Random.Range(0, PlayerShipController.instance.playerShipTiles.Count - 1)];

        //Apply any weapon cooldowns
        if (enemyChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().cooldown > 0)
        {
            enemyChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().ui_cooldown.SetActive(true);
            enemyChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().curCooldown = enemyChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().cooldown;
            enemyChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().txt_cooldown.text = enemyChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().cooldown.ToString();
        }

        try
        {
            if (turnAttack > playerShipProps.evade)
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

        playerShipProps.evade -= evadeBonus;
        evadeBonus = 0;        
    }

    GameObject EnemyRandomiseWeapon()
    {
        List<GameObject> enemyWeaponSlots = new List<GameObject>(enemyShipProps.weaponSlots);

        for (int i = 0; i < enemyWeaponSlots.Count; i++)
        {
            if(enemyWeaponSlots[i].GetComponent<ShipWeaponProps>().curCooldown > 0)
            {
                enemyWeaponSlots.RemoveAt(i);
            }
        }

        GameObject RandomWeapon = enemyWeaponSlots[Random.Range(0, enemyWeaponSlots.Count)];

        return RandomWeapon;
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
                    GameController.instance.selectorShipTarget.SetActive(true);
                    GameController.instance.selectorShipTarget.transform.position = hit.transform.position;

                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        ui_shipCombatDisable.SetActive(true);

                        if (playerChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().cooldown > 0)
                        {
                            playerChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().ui_cooldown.SetActive(true);
                            playerChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().curCooldown = playerChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().cooldown;
                            playerChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().txt_cooldown.text = playerChosenWeapon.gameObject.GetComponent<ShipWeaponProps>().cooldown.ToString();
                        }                        

                        GameController.instance.selectorShipTarget.SetActive(false);

                        int d20 = Random.Range(1, 20);
                        int turnAttack = d20 + playerChosenWeapon.GetComponent<ShipWeaponProps>().accuracy; //playerShipProps.targeting;
                        Debug.Log("Player attack is " + d20 + " + " + playerChosenWeapon.GetComponent<ShipWeaponProps>().accuracy);
                        bool willHitTarget = false;
                        playerTargetObject = hit.transform.gameObject;

                        if (turnAttack > enemyShipProps.evade)
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
            }
        }
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
        //Debug.Log("Total weapons: " + playerShipProps.weaponSlots.Count);
        for (int i = 0; i < playerShipProps.weaponSlots.Count; i++)
        {
            ShipWeaponProps playerWeaponProps = playerShipProps.weaponSlots[i].gameObject.GetComponent<ShipWeaponProps>();

            if (playerWeaponProps.curCooldown > 1)
            {
                playerWeaponProps.curCooldown -= 1;
                playerWeaponProps.txt_cooldown.text = playerWeaponProps.curCooldown.ToString();
            }
            else
            {
                playerWeaponProps.curCooldown = 0;
                playerWeaponProps.ui_cooldown.SetActive(false);
            }
        }

        //Debug.Log("Total weapons: " + playerShipProps.weaponSlots.Count);
        for (int i = 0; i < enemyShipProps.weaponSlots.Count; i++)
        {
            ShipWeaponProps enemyWeaponProps = enemyShipProps.weaponSlots[i].gameObject.GetComponent<ShipWeaponProps>();

            if (enemyWeaponProps.curCooldown > 1)
            {
                enemyWeaponProps.curCooldown -= 1;
                enemyWeaponProps.txt_cooldown.text = enemyWeaponProps.curCooldown.ToString();
            }
            else
            {
                enemyWeaponProps.curCooldown = 0;
                enemyWeaponProps.ui_cooldown.SetActive(false);
            }
        }
    }

    void ClearWeaponCooldowns()
    {
        //Debug.Log("Total weapons: " + playerShipProps.weaponSlots.Count);
        for (int i = 0; i < playerShipProps.weaponSlots.Count; i++)
        {
            ShipWeaponProps weaponProps = playerShipProps.weaponSlots[i].gameObject.GetComponent<ShipWeaponProps>();
            weaponProps.curCooldown = 0;
            weaponProps.ui_cooldown.SetActive(false);
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

    void MoveEnemyShip(Vector2 pos)
    {
        //Move enemy to target position (use for entering combat, exiting, fleeing etc)
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

            Vector2 newPos = new Vector2(enemySpawnPos.x, enemySpawnPos.y) + Random.insideUnitCircle * 6f;
            GameObject explosionEffect = Instantiate(effect_cloud, newPos, transform.rotation, transform.GetChild(0));
            explosionEffect.transform.localScale *= 7.5f;
            explosionEffect.GetComponent<DestroyObjectProps>().destroyTimer = 0.25f;
            yield return new WaitForSeconds(0.15f);
        }
        Destroy(enemyShip.transform.GetChild(0).gameObject);
        Instantiate(effect_shipExplode, enemySpawnPos, transform.rotation, transform);
        ui_enemyShipStats.SetActive(false);

        yield return new WaitForSeconds(1.5f);

        inCombat = false;
        ui_ShipCombat.SetActive(false);
        txt_SelectedWeapon.text = null;

        ClearWeaponCooldowns();

        CameraController.instance.ZoomCamera(8);
        CameraController.instance.target = PlayerController.instance.gameObject;
        CameraController.instance.JumpToTarget();
        GameController.instance.gameState = GameController.GameState.game;
        GameController.instance.gameCursor.GetComponent<CursorProps>().cursorType = CursorProps.CursorType.select;
        OverworldController.instance.isTravelPaused = false;
    }

    IEnumerator EnemyShipFlee()
    {
        Debug.Log("Enemy Ship fleeing");
        enemyExit = true;
        ui_enemyShipStats.SetActive(false);
        yield return new WaitForSeconds(2f);

        Destroy(enemyShip.transform.GetChild(0).gameObject);

        inCombat = false;
        ui_ShipCombat.SetActive(false);
        txt_SelectedWeapon.text = null;

        ClearWeaponCooldowns();

        CameraController.instance.ZoomCamera(8);
        CameraController.instance.target = PlayerController.instance.gameObject;
        CameraController.instance.JumpToTarget();
        GameController.instance.gameState = GameController.GameState.game;
        GameController.instance.gameCursor.GetComponent<CursorProps>().cursorType = CursorProps.CursorType.select;
        enemyExit = false;

        OverworldController.instance.isTravelPaused = false;
    }

}
