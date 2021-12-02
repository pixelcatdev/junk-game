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
                    ShipMapProps enemyShipProps = ShipCombatController.instance.enemyShip.GetComponent<ShipMapProps>();

                    //Destroy the enemy ship and end combat if it's less than the critical value
                    if (enemyShipProps.mapCurHealth / enemyShipProps.mapMaxHealth < 0.5f)
                    {
                        ShipCombatController.instance.CombatEnd(true);
                    }
                    else
                    {
                        ShipCombatController.instance.StartCoroutine("TurnDelay", 2f);
                    }                    
                }
                else if (projectileType == ProjectileType.enemy)
                {
                    ShipMapProps playerShipProps = PlayerShipController.instance.GetComponent<ShipMapProps>();

                    //Destroy the enemy ship and end combat if it's less than the critical value
                    if (playerShipProps.mapCurHealth / playerShipProps.mapMaxHealth < 0.5f)
                    {
                        Debug.Log("GameOver");
                    }
                }

                hitIsChecked = true;
            }


        }
    }
}
