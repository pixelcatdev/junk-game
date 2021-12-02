using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipProjectileProps : MonoBehaviour
{
    public enum ProjectileType { player, enemy }
    public ProjectileType projectileType;
    public GameObject targetObject;
    public bool willHitTarget;
    public bool explodeOnImpact;
    public GameObject explosionDamage;
    private bool hitIsChecked;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == targetObject)
        {

            if (!hitIsChecked)
            {
                if (willHitTarget)
                {
                    collision.gameObject.GetComponent<TileProps>().DestroyObject(true, true);
                    Destroy(gameObject);

                    if (explodeOnImpact)
                    {
                        Instantiate(explosionDamage, transform.position, transform.rotation);
                    }
                }

                //If it's a player projectile, call the Enemy's turn after it hits
                if (projectileType == ProjectileType.player)
                {
                    ShipCombatController.instance.EnemyTurn();
                }

                hitIsChecked = true;
            }


        }
    }
}
