using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Player Input and stats
public class PlayerController : MonoBehaviour
{
    public float speed;
    public float health;
    public int cutterDamage;
    private Vector2 aimDirection;
    private Rigidbody2D rb;
    private Animator animator;

    private GameObject lastHit;
    private bool tileHit;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        PlayerInput();
        Animator();
    }

    void PlayerInput()
    {
        //Get left input
        //Move Player transform
        float dirX = Input.GetAxisRaw("Horizontal");
        float dirY = Input.GetAxisRaw("Vertical");

        rb.velocity = new Vector2(dirX * speed, dirY * speed);

        //Flip the Player
        if(dirX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else if (dirX > 0)
        {            
            GetComponent<SpriteRenderer>().flipX = false;
        }
        

        //Get right input
        if (Input.GetKey(KeyCode.UpArrow))
        {
            aimDirection = Vector2.up;
            CastRay();
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            aimDirection = Vector2.down;
            CastRay();
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            aimDirection = Vector2.left;
            CastRay();
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            aimDirection = Vector2.right;
            CastRay();
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
    void CastRay()
    {
        Vector3 startPos = aimDirection.normalized * 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position + startPos, aimDirection, 0.05f);
        Debug.DrawRay(transform.position + startPos, aimDirection * 0.25f, Color.green);

        if (hit)
        {

            if(lastHit == null)
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
            if (hit.transform.gameObject.GetComponent<TileProps>().canDestroy)
            {
                hit.transform.gameObject.GetComponent<TileProps>().curHealth -= cutterDamage * Time.deltaTime;
                hit.transform.gameObject.GetComponent<TileProps>().isTakingDamage = true;
            }
        }
        
    }

    ////Clears the last hit target if there's no input
    //void ClearLastHit()
    //{
    //    if (!tileHit)
    //    {
    //        lastHit.gameObject.GetComponent<TileProps>().ui_tile.SetActive(false);
    //    }
    //}

    void FireWeapon()
    {

    }

    void Animator()
    {
        //run
        if(rb.velocity.magnitude > 0)
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
