using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileProps : MonoBehaviour
{
    public enum ProjectileType { Enemy, Player };
    public ProjectileType projectileType;

    public float cooldown;
    public float damage;
    public float speed;

    public bool useAmmo;
    public int ammoMax;
    public bool explodeOnImpact;
    public GameObject explosionDamage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (projectileType == ProjectileType.Player)
        {
            if (collision.gameObject.tag == "isEnemy")
            {
                collision.gameObject.GetComponent<EnemyProps>().TakeDamage(damage);
                Destroy(gameObject);

                if (explodeOnImpact)
                {
                    Instantiate(explosionDamage, transform.position, transform.rotation);
                }
            }

            else if (collision.gameObject.tag == "ShipTileWall" || collision.gameObject.tag == "isExploder")
            {
                collision.gameObject.GetComponent<TileProps>().TakeDamage(damage, true);
                Destroy(gameObject);

                if (explodeOnImpact)
                {
                    Instantiate(explosionDamage, transform.position, transform.rotation);
                }
            }
        }

        if (projectileType == ProjectileType.Enemy)
        {
            if (collision.gameObject.tag == "Player")
            {
                //damage the player
                Destroy(collision.gameObject);

                if (explodeOnImpact)
                {
                    Instantiate(explosionDamage, transform.position, transform.rotation);
                }
            }

            if (collision.gameObject.tag == "ShipTileWall" || collision.gameObject.tag == "isExploder")
            {
                collision.gameObject.GetComponent<TileProps>().TakeDamage(damage, true);
                Destroy(gameObject);

                if (explodeOnImpact)
                {
                    Instantiate(explosionDamage, transform.position, transform.rotation);
                }
            }

            if (collision.gameObject.tag == "isHull")
            {
                Destroy(gameObject);

                if (explodeOnImpact)
                {
                    Instantiate(explosionDamage, transform.position, transform.rotation);
                }
            }

        }
    }
}
