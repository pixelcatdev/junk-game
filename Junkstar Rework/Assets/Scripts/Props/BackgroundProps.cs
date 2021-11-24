using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundProps : MonoBehaviour
{
    private float randomRotation;
    private float randomScale;

    // Start is called before the first frame update
    void Start()
    {
        randomRotation = Random.Range(-10, 10);
        randomScale = Random.Range(0.5f, 2f);

        transform.localScale *= randomScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, randomRotation * Time.deltaTime);
    }
}
