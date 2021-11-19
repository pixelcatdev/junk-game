using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public enum EquipType { destroy, build, shoot }
    public EquipType equipped;
    public GameObject projectile;

    public bool isDestroying;
    private Vector2 targetDirection;
    private GameObject lastHit;
    private bool tileHit;

    public GameObject buildingObject;

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
            Animator();

            DestroyMode();
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
        if (dirX < 0)
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
        else if (dirX > 0)
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
        else if (equipped == EquipType.build)
        {
            //work out if the player can afford it and color the blueprint accordingly
            if (Input.GetKeyDown(KeyCode.B))
            {
                if (InventoryController.instance.HasResources(buildingObject) == true)
                {
                    Debug.Log("Enough loot, can build");
                    //build object
                }
                else
                {
                    Debug.Log("Not enough loot, cannot build");
                    //don't build object
                }
            }            
        }
        else if (equipped == EquipType.shoot)
        {
            FireWeapon();
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

    void DestroyMode()
    {
        if (equipped == EquipType.destroy)
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
    }

    void FireWeapon()
    {
        ////Get right input
        //if (Input.GetKey(KeyCode.UpArrow))
        //{
        //    aimDirection = Vector2.up;
        //}
        //else if (Input.GetKey(KeyCode.DownArrow))
        //{
        //    aimDirection = Vector2.down;
        //}
        //else if (Input.GetKey(KeyCode.LeftArrow))
        //{
        //    aimDirection = Vector2.left;
        //}
        //else if (Input.GetKey(KeyCode.RightArrow))
        //{
        //    aimDirection = Vector2.right;
        //}

        //if (gunTimer > 0)
        //{
        //    gunTimer -= Time.deltaTime;
        //}
        //else
        //{
        //    GameObject newProjectile = Instantiate(projectile, transform.position, transform.rotation);
        //    newProjectile.GetComponent<Rigidbody2D>().AddForce(aimDirection * newProjectile.GetComponent<ProjectileProps>().speed, ForceMode2D.Impulse);
        //    gunTimer = projectile.GetComponent<ProjectileProps>().cooldown;
        //}
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
