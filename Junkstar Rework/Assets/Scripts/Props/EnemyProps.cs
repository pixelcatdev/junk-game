using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProps : MonoBehaviour
{
    public enum EnemyType { sentry, ranged, melee }
    public EnemyType enemyType;

    public string enemyName;
    private float curHealth;
    public float maxHealth;
    public float speed;
    public GameObject projectile;
    public float environmentDamage;
    public float attackDamage;
    public float attackRate;

    public List<GameObject> spawnOnDestroy;
    public GameObject parentSpawner;

    private Rigidbody2D rb;
    private Animator anim;

    private void Start()
    {
        curHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Move();

    }

    void Move()
    {
        if (TargetShipController.instance.playerIsBoarded)
        {
            transform.position = Vector2.MoveTowards(transform.position, PlayerController.instance.transform.position, speed * Time.deltaTime);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<TileProps>() && collision.gameObject.tag == "ShipTileWall" || collision.gameObject.tag == "isDestructable" )
        {
            collision.gameObject.GetComponent<TileProps>().TakeDamage(environmentDamage, true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<TileProps>())
        {

        }
    }

    public void TakeDamage(float damage)
    {
        //Deduct object health until destroyed
        if (curHealth - damage > 0)
        {
            curHealth -= damage;
        }
        else
        {
            //Spawn object on destroy
            foreach (GameObject spawnObj in spawnOnDestroy)
            {
                GameObject newObj = Instantiate(spawnObj, transform.position, transform.rotation, transform.parent);
            }

            Destroy(gameObject);
        }

        //Draw healthbar
        //healthBar.fillAmount = curHealth / maxHealth;
    }
}
