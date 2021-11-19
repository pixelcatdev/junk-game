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

        JumpToTarget();
    }

    //Instantly snap to the target
    public void JumpToTarget()
    {
        transform.position = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);
    }

    private void Update()
    {
        if (target == PlayerController.instance.gameObject)
        {
            float distance = Vector2.Distance(PlayerController.instance.transform.position, transform.position);
            float targetDist = 0.05f;

            if (distance > targetDist)
            {
                transform.position = Vector2.MoveTowards(transform.position, PlayerController.instance.transform.position, 2f * Time.deltaTime);
            }
        }
        else
        {
            transform.position = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);
        }
    }
}
