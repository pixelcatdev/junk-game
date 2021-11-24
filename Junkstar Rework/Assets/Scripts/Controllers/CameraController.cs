using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject target;
    public float parallaxSpeed;

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
        ChaseCam();
        ParallaxBackground();
    }

    void ChaseCam()
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

    void ParallaxBackground()
    {
        if (TargetShipController.instance.playerIsBoarded)
        {
            float dirX = Input.GetAxisRaw("Horizontal");
            float dirY = Input.GetAxisRaw("Vertical");

            Transform parallaxBackground = TargetShipController.instance.background.transform;

            if (dirX < 0)
            {                
                TargetShipController.instance.background.transform.position = new Vector2(parallaxBackground.position.x + parallaxSpeed * Time.deltaTime, parallaxBackground.position.y);
            }
            else if (dirX > 0)
            {
                TargetShipController.instance.background.transform.position = new Vector2(parallaxBackground.position.x - parallaxSpeed * Time.deltaTime, parallaxBackground.position.y);
            }

            if (dirY < 0)
            {
                TargetShipController.instance.background.transform.position = new Vector2(parallaxBackground.position.x, parallaxBackground.position.y + parallaxSpeed * Time.deltaTime);
            }
            else if (dirY > 0)
            {
                TargetShipController.instance.background.transform.position = new Vector2(parallaxBackground.position.x, parallaxBackground.position.y - parallaxSpeed * Time.deltaTime);
            }
        }
    }
}
