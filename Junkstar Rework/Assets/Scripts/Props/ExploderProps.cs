﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Explodes within x radius, destroys whatever it touches
public class ExploderProps : MonoBehaviour
{
    public float explodeRadius;
    public float cleanupTimer;

    private void Start()
    {
        GetComponent<CircleCollider2D>().radius = explodeRadius;
    }

    //apply the exploderDamage to any objects in the trigger radius that CAN take damage
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.transform.name);
        if (collision.GetComponent<TileProps>())
        {
            Debug.Log("Destroying object");
            if (collision.GetComponent<TileProps>().canDestroy == true)
            {
                collision.GetComponent<TileProps>().DestroyTile(false, true);
            }
        }
        StartCoroutine("ClearUpEffect");
    }

    IEnumerator ClearUpEffect()
    {
        yield return new WaitForSeconds(cleanupTimer);
        Destroy(gameObject);
    }
}
