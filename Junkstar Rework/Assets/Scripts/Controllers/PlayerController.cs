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

    public enum EquipType { game, destroy, build, shoot }
    public EquipType equipMode;
    public GameObject projectile;

    public bool isDestroying;
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
            BuildMode();
            ShootMode();

            //Debug tool toggling
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("Cutter equipped");
                equipMode = EquipType.destroy;
                GameController.instance.gameCursor.GetComponent<CursorProps>().cursorType = CursorProps.CursorType.destroy;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("Welder equipped");
                equipMode = EquipType.build;
                GameController.instance.gameCursor.GetComponent<CursorProps>().cursorType = CursorProps.CursorType.building;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("Gun equipped");
                equipMode = EquipType.shoot;
                GameController.instance.gameCursor.GetComponent<CursorProps>().cursorType = CursorProps.CursorType.aim;
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
            GetComponent<SpriteRenderer>().flipX = true;

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).GetComponent<SpriteRenderer>())
                {
                    transform.GetChild(i).GetComponent<SpriteRenderer>().flipX = true;
                }
            }
        }
        else if (mousePos.x > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).GetComponent<SpriteRenderer>())
                {
                    transform.GetChild(i).GetComponent<SpriteRenderer>().flipX = false;
                }
            }
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
    }

    //Rotate the currently equipped tool to aim towards the mouse
    void AimEquipped()
    {
        if (equipped.activeInHierarchy)
        {
            var pos = Input.mousePosition;
            pos.z = transform.position.z - Camera.main.transform.position.z;
            pos = Camera.main.ScreenToWorldPoint(pos);

            equipped.transform.rotation = Quaternion.FromToRotation(Vector3.right, pos - transform.position);
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (mousePos.x < 0)
            {
                equipped.transform.Rotate(0, -180, 0);
            }
            else if (mousePos.x > 0)
            {
                equipped.transform.Rotate(0, 0, 0);
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
                if (objDistance < cutterRange && hitObj.GetComponent<TileProps>())
                {
                    //if there's no objects previously hit, set the current object to that last object and highlight it
                    if (lastHit == null)
                    {
                        lastHit = hitObj;

                        hitObj.GetComponent<TileProps>().ui_tile.SetActive(true);
                    }
                    else
                    {
                        //otherwise if the new object is different to the last object, clear the highlight on the last object, then set the lastObj to the current object and highlight it
                        if (hitObj != lastHit)
                        {
                            lastHit.GetComponent<TileProps>().ui_tile.SetActive(false);
                            lastHit = hitObj;
                            hitObj.GetComponent<TileProps>().ui_tile.SetActive(true);
                        }
                    }

                    //If the player presses the mouse 0, start damaging the object
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        isDestroying = true;
                        //anim.SetBool("isShooting", true);
                    }
                    else
                    {
                        //SetBool("isShooting", false);
                    }

                    if (Input.GetKey(KeyCode.Mouse0) && isDestroying == true)
                    {
                        hit.transform.GetComponent<TileProps>().TakeDamage(cutterDamage, false);
                    }
                    else
                    {
                        isDestroying = false;
                    }

                }
                else
                {
                    //If the player goes out of range of the object, clear the highlight on that object
                    if (lastHit != null)
                    {
                        lastHit.GetComponent<TileProps>().ui_tile.SetActive(false);
                    }
                }
            }
        }

        //Clear the highlight on the object if they're not in destroy mode
        else
        {
            if(lastHit != null)
            {
                lastHit.GetComponent<TileProps>().ui_tile.SetActive(false);
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

            if (hit && hit.transform.gameObject.GetComponent<TileProps>() && hit.transform.tag == "ShipTileFloor" && !hit.transform.gameObject.GetComponent<TileProps>().isOccupied && buildingObject != null)
            {
                GameObject hitObj = hit.transform.gameObject;

                float objDistance = Vector2.Distance(transform.position, hitObj.transform.position);

                //if the hit obj is within than the players range
                if (objDistance < 2)
                {
                    //if there's no objects previously hit, set the current object to that last object and highlight it
                    if (lastHit == null)
                    {
                        lastHit = hitObj;

                        hitObj.GetComponent<TileProps>().ui_BuildTile.SetActive(true);
                    }
                    else
                    {
                        //otherwise if the new object is different to the last object, clear the highlight on the last object, then set the lastObj to the current object and highlight it
                        if (hitObj != lastHit)
                        {
                            lastHit.GetComponent<TileProps>().ui_BuildTile.SetActive(false);
                            lastHit = hitObj;
                            hitObj.GetComponent<TileProps>().ui_BuildTile.SetActive(true);
                            //hitObj.GetComponent<TileProps>().blueprintCursor.GetComponent<SpriteRenderer>().sprite = buildObj.GetComponent<Buildable>().buildingBlueprint;
                        }
                    }

                    //work out if the player can afford it and color the blueprint accordingly
                    if(hitObj.GetComponent<TileProps>().ui_BuildTile != null)
                    {
                        if (InventoryController.instance.HasResources(buildingObject) == true)
                        {
                            hitObj.GetComponent<TileProps>().buildBlueprint.GetComponent<Image>().sprite = buildingObject.GetComponent<BuildingProps>().buildingBlueprint;
                            hitObj.GetComponent<TileProps>().buildBlueprint.GetComponent<Image>().color = Color.white;
                        }
                        else
                        {
                            hitObj.GetComponent<TileProps>().buildBlueprint.GetComponent<Image>().sprite = buildingObject.GetComponent<BuildingProps>().buildingBlueprint;
                            hitObj.GetComponent<TileProps>().buildBlueprint.GetComponent<Image>().color = Color.red;
                        }
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

                            lastHit.GetComponent<TileProps>().ui_BuildTile.SetActive(false);
                            //lastHit.GetComponent<TileProps>().canDestroy = false;
                            lastHit.GetComponent<TileProps>().isOccupied = true;
                        }
                    }
                }
                else
                {
                    //If the player goes out of range of the object, clear the highlight on that object
                    if (lastHit != null)
                    {
                        lastHit.GetComponent<TileProps>().ui_BuildTile.SetActive(false);
                    }
                }
            }
        }
        //Clear the highlight on the object if they're not in build mode
        else
        {
            if (lastHit != null && lastHit.GetComponent<TileProps>().ui_BuildTile != null)
            {
                lastHit.GetComponent<TileProps>().ui_BuildTile.SetActive(false);
            }
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
