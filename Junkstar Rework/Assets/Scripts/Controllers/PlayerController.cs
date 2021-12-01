using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Player Input and stats
public class PlayerController : MonoBehaviour
{
    public float speed;
    public float health;
    public float cutterDamage;
    public float cutterRange;
    public float gunCooldown;
    private float gunTimer;
    private Vector2 aimDirection;
    private Rigidbody2D rb;
    private Animator animator;

    public GameObject helmet;
    public GameObject hair;
    public GameObject outfit;
    public GameObject equipped;
    public GameObject equippedDestroyer;
    public GameObject equippedBuilder;
    public GameObject equippedGun;

    public enum EquipType { select, destroy, repair, build, shoot }
    public EquipType equipMode;
    public GameObject projectile;

    public bool isMouseDown;
    private Vector2 targetDirection;
    private GameObject lastHit;
    private bool tileHit;

    private float cooldownTimer;

    public GameObject buildingObject;
    public List<GameObject> playerBuildings;

    public static PlayerController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!PlayerController.instance)
        {
            PlayerController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        SwitchEquipped(1);
    }

    void Update()
    {
        //Player input only if in game mode
        if (GameController.instance.gameState == GameController.GameState.game)
        {
            PlayerInput();
            AimEquipped();
            Animator();

            DestroyMode();
            RepairMode();
            BuildMode();
            ShootMode();

            if (Input.GetKeyDown(KeyCode.Q))
            {
                equipMode = EquipType.select;
                GameController.instance.gameCursor.GetComponent<CursorProps>().cursorType = CursorProps.CursorType.select;
            }
        }
    }

    void PlayerInput()
    {
        //Get left input
        //Move Player transform
        float dirX = Input.GetAxisRaw("Horizontal");
        float dirY = Input.GetAxisRaw("Vertical");

        rb.velocity = new Vector2(dirX * speed, dirY * speed);

        //Flip the Player
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (mousePos.x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (mousePos.x > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        //Get interaction input
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f);
            foreach (Collider2D collider in colliders)
            {
                GameObject targetObj = collider.gameObject;

                //If the object has an InteractorProps attached, call Activate()
                if (targetObj.GetComponent<InteractorProps>())
                {
                    targetObj.GetComponent<InteractorProps>().Activate();
                }
            }
        }

        //Switch to destroy or gun
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchEquipped(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchEquipped(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SwitchEquipped(4);
        }
    }

    //Takes UI input and toggles the relevant mode
    public void SwitchEquipped(int mode)
    {
        GameController.instance.selectorBuild.SetActive(false);
        GameController.instance.selectorDestroy.SetActive(false);
        GameController.instance.selectorRepair.SetActive(false);

        //Destroy
        if (mode == 1)
        {
            equipMode = EquipType.destroy;
            equippedDestroyer.SetActive(true);
            equippedBuilder.SetActive(false);
            equippedGun.SetActive(false);
            GameController.instance.gameCursor.GetComponent<CursorProps>().cursorType = CursorProps.CursorType.destroy;
        }
        //Repair
        else if (mode == 2)
        {
            equipMode = EquipType.repair;
            equippedDestroyer.SetActive(true);
            equippedBuilder.SetActive(false);
            equippedGun.SetActive(false);
            GameController.instance.gameCursor.GetComponent<CursorProps>().cursorType = CursorProps.CursorType.repair;
        }
        //Build
        else if (mode == 3)
        {
            equipMode = EquipType.build;
            equippedDestroyer.SetActive(false);
            equippedBuilder.SetActive(true);
            equippedGun.SetActive(false);
            GameController.instance.gameCursor.GetComponent<CursorProps>().cursorType = CursorProps.CursorType.building;
        }
        //Shoot
        else if (mode == 4)
        {
            equipMode = EquipType.shoot;
            equippedDestroyer.SetActive(false);
            equippedBuilder.SetActive(false);
            equippedGun.SetActive(true);
            GameController.instance.gameCursor.GetComponent<CursorProps>().cursorType = CursorProps.CursorType.aim;
        }
    }

    //Rotate the currently equipped tool to aim towards the mouse
    void AimEquipped()
    {
        if (equipped.activeInHierarchy)
        {
            var pos = Input.mousePosition;
            pos.z = transform.position.z - Camera.main.transform.position.z;
            pos = Camera.main.ScreenToWorldPoint(pos);

            equipped.transform.rotation = Quaternion.FromToRotation(Vector2.right, pos - transform.position);

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (mousePos.x < 0)
            {
                //equipped.transform.rotation = Quaternion.Euler(0, 180, 0);
                //equipped.transform.rotation = Quaternion.FromToRotation(Vector2.up, pos - transform.position);
            }
            else if (mousePos.x > 0)
            {
                //equipped.transform.rotation = Quaternion.Euler(0, 0, 0);
                //equipped.transform.rotation = Quaternion.FromToRotation(Vector2.up, pos - transform.position);

            }
        }
    }

    void DestroyMode()
    {
        if (equipMode == EquipType.destroy)
        {
            LayerMask hitLayer = LayerMask.GetMask("ShipTile", "Object");

            RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0f, hitLayer);

            if (hits.Length > 0)
            {
                System.Array.Sort(hits, (h1, h2) => h2.transform.gameObject.layer.CompareTo(h1.transform.gameObject.layer));
                GameObject hit = hits[0].transform.gameObject;
                GameObject hitObj = hit.transform.gameObject;

                float objDistance = Vector2.Distance(transform.position, hitObj.transform.position);

                //if the hit obj is within than the players range
                if (objDistance < cutterRange && hitObj.GetComponent<TileProps>() && hitObj.GetComponent<TileProps>().canDestroy)
                {
                    GameController.instance.selectorDestroy.SetActive(true);
                    GameController.instance.selectorDestroy.transform.position = hit.transform.position;
                    GameController.instance.selectorDestroy.GetComponent<SelectorProps>().targetTile = hit;

                    //If the player presses the mouse 0, start damaging the object
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        isMouseDown = true;
                    }

                    if (Input.GetKey(KeyCode.Mouse0) && isMouseDown == true)
                    {
                        hit.transform.GetComponent<TileProps>().TakeDamage(cutterDamage, false);
                    }
                    else
                    {
                        isMouseDown = false;
                    }
                }
                else
                {
                    GameController.instance.selectorDestroy.SetActive(false);
                }
            }
            else
            {
                GameController.instance.selectorDestroy.SetActive(false);
            }
        }
    }

    void RepairMode()
    {
        if (equipMode == EquipType.repair)
        {
            LayerMask hitLayer = LayerMask.GetMask("ShipTile", "Object");

            RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0f, hitLayer);

            if (hits.Length > 0)
            {
                //System.Array.Sort(hits, (h1, h2) => h2.transform.gameObject.layer.CompareTo(h1.transform.gameObject.layer));
                GameObject hit = hits[0].transform.gameObject;
                GameObject hitObj = hit.transform.gameObject;

                float objDistance = Vector2.Distance(transform.position, hitObj.transform.position);

                //if the hit obj is within than the players range
                if (objDistance < cutterRange && hitObj.GetComponent<TileProps>() && hitObj.GetComponent<TileProps>().canRepair)
                {
                    GameController.instance.selectorRepair.SetActive(true);
                    GameController.instance.selectorRepair.transform.position = hit.transform.position;
                    GameController.instance.selectorRepair.GetComponent<SelectorProps>().targetTile = hit;
                    GameController.instance.selectorRepair.GetComponent<SelectorProps>().buildingBlueprint.sprite = hit.GetComponent<TileProps>().spawnOnDestroy[0].GetComponent<SpriteRenderer>().sprite;

                    if (InventoryController.instance.HasResources(hit.GetComponent<TileProps>().spawnOnDestroy[0]) == true)
                    {
                        GameController.instance.selectorRepair.GetComponent<SelectorProps>().buildingBlueprint.color = Color.white;
                    }
                    else
                    {
                        GameController.instance.selectorRepair.GetComponent<SelectorProps>().buildingBlueprint.color = Color.red;
                    }

                    //If the player presses the mouse 0, start damaging the object
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        isMouseDown = true;
                    }

                    if (Input.GetKey(KeyCode.Mouse0) && isMouseDown == true)
                    {
                        //if the player has the resources to repair
                        if (InventoryController.instance.HasResources(hit.GetComponent<TileProps>().spawnOnDestroy[0]) == true)
                        {
                            //Deduct the item costs
                            InventoryController.instance.DeductResources(hit.GetComponent<TileProps>().spawnOnDestroy[0]);
                            hit.transform.GetComponent<TileProps>().Repair();
                        }
                    }
                    else
                    {
                        isMouseDown = false;
                    }
                }
                else
                {
                    GameController.instance.selectorRepair.SetActive(false);
                }
            }
            else
            {
                GameController.instance.selectorRepair.SetActive(false);
            }
        }
    }

    void BuildMode()
    {
        if (equipMode == EquipType.build)
        {
            Debug.Log("Build mode");
            //get the sprite of the chosen blueprint

            LayerMask hitLayer = LayerMask.GetMask("ShipTile");
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0f, hitLayer);

            if (hit)
            {
                if (hit.transform.gameObject.GetComponent<TileProps>())
                {
                    TileProps tile = hit.transform.gameObject.GetComponent<TileProps>();

                    GameObject hitObj = hit.transform.gameObject;
                    float objDistance = Vector2.Distance(transform.position, hitObj.transform.position);

                    if (hit.transform.tag == "ShipTileFloor" && !tile.isOccupied && objDistance < 2)
                    {
                        //work out if the player can afford it and color the blueprint accordingly

                        GameController.instance.selectorBuild.SetActive(true);
                        GameController.instance.selectorBuild.transform.position = hit.transform.position;
                        GameController.instance.selectorBuild.GetComponent<SelectorProps>().targetTile = hit.transform.gameObject;
                        GameController.instance.selectorBuild.GetComponent<SelectorProps>().buildingBlueprint.sprite = buildingObject.GetComponent<BuildingProps>().buildingBlueprint;

                        if (InventoryController.instance.HasResources(buildingObject) == true)
                        {
                            GameController.instance.selectorBuild.GetComponent<SelectorProps>().buildingBlueprint.color = Color.white;
                        }
                        else
                        {
                            GameController.instance.selectorBuild.GetComponent<SelectorProps>().buildingBlueprint.color = Color.red;
                        }

                        //If mouse 0 is clicked, build the object
                        if (Input.GetKey(KeyCode.Mouse0) && hitObj.GetComponent<TileProps>().isOccupied == false)
                        {
                            //deduct the building costs
                            if (InventoryController.instance.HasResources(buildingObject) == true)
                            {
                                //Deduct the item costs
                                InventoryController.instance.DeductResources(buildingObject);

                                //instantiate the new building, and child it to the hitObj
                                GameObject newBuild = Instantiate(buildingObject, hitObj.transform.position, hitObj.transform.rotation, hitObj.transform);

                                //Add the building to the list of builtObjects
                                playerBuildings.Add(newBuild.gameObject);

                                //lastHit.GetComponent<TileProps>().ui_BuildTile.SetActive(false);
                                hitObj.GetComponent<TileProps>().canDestroy = false;
                                hitObj.GetComponent<TileProps>().isOccupied = true;
                            }
                        }

                    }
                    else
                    {
                        GameController.instance.selectorBuild.SetActive(false);
                    }
                }
                else
                {
                    GameController.instance.selectorBuild.SetActive(false);
                }
            }
        }
        //Clear the highlight on the object if they're not in build mode
        else
        {
            GameController.instance.selectorBuild.SetActive(false);
        }
    }

    void ShootMode()
    {
        if (equipMode == EquipType.shoot)
        {
            var pos = Input.mousePosition;
            pos.z = transform.position.z - Camera.main.transform.position.z;
            pos = Camera.main.ScreenToWorldPoint(pos);
            //targetCursor.transform.position = pos;

            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (Time.time > cooldownTimer)
                {
                    ProjectileProps projectileProps = projectile.GetComponent<ProjectileProps>();

                    var q = Quaternion.FromToRotation(Vector3.up, pos - transform.position);
                    var go = Instantiate(projectile, transform.position, q);
                    go.GetComponent<Rigidbody2D>().AddForce(go.transform.up * projectileProps.speed, ForceMode2D.Impulse);

                    cooldownTimer = Time.time + projectileProps.cooldown;

                }
            }
        }
    }

    void Animator()
    {
        if (enabled)
        {
            //run
            if (rb.velocity.magnitude > 0)
            {
                animator.Play("Player_Run");
            }
            //idle
            else
            {
                animator.Play("Player_Idle");
            }
        }
    }
}
