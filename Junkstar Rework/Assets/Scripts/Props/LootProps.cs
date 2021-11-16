using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootProps : MonoBehaviour
{
    public float destroyTimer;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("DestroyLoot");
    }

    IEnumerator DestroyLoot()
    {
        yield return new WaitForSeconds(destroyTimer);
        Destroy(gameObject);
    }
}
