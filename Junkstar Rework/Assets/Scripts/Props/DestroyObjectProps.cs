using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Very basic props class used to destroy objects on instantiation with a timer
public class DestroyObjectProps : MonoBehaviour
{
    public float destroyTimer;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Destroyer", destroyTimer);
    }

    IEnumerator Destroyer(float destroyTimer)
    {
        yield return new WaitForSeconds(destroyTimer);
        Destroy(gameObject);
    }
}
