using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject target;

    public static CameraController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!CameraController.instance)
        {
            CameraController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        transform.position = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);

        //float distance = Vector2.Distance(PlayerController.instance.transform.position, transform.position);
        //float targetDist = 0.25f;

        //if(distance > targetDist)
        //{
        //    transform.position = Vector2.MoveTowards(transform.position, PlayerController.instance.transform.position, 1 * Time.deltaTime);
        //}
    }
}
