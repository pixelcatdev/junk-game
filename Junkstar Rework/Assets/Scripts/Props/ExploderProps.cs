using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Explodes within x radius, destroys whatever it touches
public class ExploderProps : MonoBehaviour
{
    public float explodeRadius;

    public void Explode()
    {
        //Cast an overlap circle all at the centre
        //destroy everything it hits 

        LayerMask hitLayer = LayerMask.GetMask("ShipTile", "Object");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explodeRadius, hitLayer);

        if (colliders.Length > 0)
        {
            //System.Array.Sort(colliders, (h1, h2) => h2.transform.gameObject.layer.CompareTo(h1.transform.gameObject.layer));
            //GameObject hit = colliders[0].transform.gameObject;

            //if (hit.GetComponent<TileProps>())
            //{
            //    hit.GetComponent<TileProps>().DestroyTile(false, true);
            //}
        }
    }
}
