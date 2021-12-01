using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipProjectileProps : MonoBehaviour
{
    public GameObject targetObject;
    public bool willHitTarget;
    public bool explodeOnImpact;
    public GameObject explosionDamage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == targetObject && willHitTarget)
        {           
            collision.gameObject.GetComponent<TileProps>().DestroyObject(true,true);
            Destroy(gameObject);

            if (explodeOnImpact)
            {
                Instantiate(explosionDamage, transform.position, transform.rotation);
            }

            ShipCombatController.instance.EnemyTurn();
        }

    }
}
