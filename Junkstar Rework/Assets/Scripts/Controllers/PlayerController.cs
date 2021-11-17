using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Player Input and stats
public class PlayerController : MonoBehaviour
{
    public float speed;
    public float health;
    public int cutterDamage;
    public float gunCooldown;
    private float gunTimer;
    private Vector2 aimDirection;
    private Rigidbody2D rb;
    private Animator animator;

    public GameObject helmet;
    public GameObject hair;
    public GameObject outfit;

    public enum EquipType { cutter, welder, gun }
    public EquipType equipped;
    public GameObject projectile;

    public bool isDestroying;
    private Vector2 targetDirection;
    private GameObject lastHit;
    private bool tileHit;

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


        //Get right input
        if (equipped == EquipType.cutter)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                aimDirection = Vector2.up;
                isDestroying = true;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                aimDirection = Vector2.down;
                isDestroying = true;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                aimDirection = Vector2.left;
                isDestroying = true;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                aimDirection = Vector2.right;
                isDestroying = true;
            }
            else
            {
                isDestroying = false;
            }
        }
        else if (equipped == EquipType.gun)
        {
            FireWeapon();
        }

        if (isDestroying)
        {
            FireCutter();
        }
        else
        {
            if (lastHit != null)
            {
                lastHit.gameObject.GetComponent<TileProps>().ui_tile.SetActive(false);
            }
        }

        //Get interaction input
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f);
            foreach (Collider2D collider in colliders)
            {
                GameObject targetObj = collider.gameObject;

                //if (targetObj.tag == "Interactive")
                //{                   
                //If the object has an InteractorProps attached, call Activate()
                if (targetObj.GetComponent<InteractorProps>())
                {
                    targetObj.GetComponent<InteractorProps>().Activate();
                }
                //}
            }
        }
    }

    //Cast a ray in the input direction to cut away tiles
    void FireCutter()
    {
        Vector3 startPos = aimDirection.normalized * 0.5f;

        LayerMask hitLayer = LayerMask.GetMask("ShipTile", "Object");
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position + startPos, aimDirection, 0.05f, hitLayer);
        Debug.DrawRay(transform.position + startPos, aimDirection * 0.25f, Color.green);

        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (h1, h2) => h2.transform.gameObject.layer.CompareTo(h1.transform.gameObject.layer));
            GameObject hit = hits[0].transform.gameObject;

            if (lastHit == null)
            {
                lastHit = hit.transform.gameObject;
            }
            else
            {
                if (lastHit != hit.transform.gameObject)
                {
                    lastHit.gameObject.GetComponent<TileProps>().ui_tile.SetActive(false);
                    hit.transform.gameObject.GetComponent<TileProps>().ui_tile.SetActive(true);
                    lastHit = hit.transform.gameObject;
                }
                else
                {
                    lastHit.gameObject.GetComponent<TileProps>().ui_tile.SetActive(true);
                }
            }

            //Apply cutterDamage
            try
            {
                if (hit.transform.gameObject.GetComponent<TileProps>().canDestroy)
                {
                    hit.transform.gameObject.GetComponent<TileProps>().curHealth -= cutterDamage * Time.deltaTime;
                    hit.transform.gameObject.GetComponent<TileProps>().isTakingDamage = true;
                }
            }
            catch (System.Exception)
            {
                Debug.Log(hit.transform.name);
                throw;
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
