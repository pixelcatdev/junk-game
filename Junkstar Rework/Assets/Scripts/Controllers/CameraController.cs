using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum CameraMode { stationary, chase }

    public GameObject target;
    public float parallaxSpeed;
    public Camera cameraObj;

    public float zoomAmount;
    public bool isZooming;

    public GameObject effect_starfield;

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
        OverlayEffects();

        if (isZooming)
        {
            Debug.Log(cameraObj.GetComponent<Camera>().orthographicSize);

            while (cameraObj.GetComponent<Camera>().orthographicSize < zoomAmount)
            {
                
                cameraObj.GetComponent<Camera>().orthographicSize += 1;
            }           

        }
    }

    void OverlayEffects()
    {
        if (OverworldController.instance.isTravelling) // && GameController.instance.gameState == GameController.GameState.game)
        {
            if (!effect_starfield.GetComponent<ParticleSystem>().isPlaying)
            {
                effect_starfield.GetComponent<ParticleSystem>().Play();
            }            
        }
        else
        {
            if (effect_starfield.GetComponent<ParticleSystem>().isPlaying)
            {
                effect_starfield.GetComponent<ParticleSystem>().Stop();
            }
        }
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

    public void ZoomCamera(float size)
    {
        StartCoroutine(resizeRoutine(cameraObj.GetComponent<Camera>().orthographicSize, (size / 50) * 70, 0.5f));
    }

    private IEnumerator resizeRoutine(float oldSize, float newSize, float time)
    {
        float elapsed = 0;
        while (elapsed <= time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);

            cameraObj.GetComponent<Camera>().orthographicSize = Mathf.Lerp(oldSize, newSize, t);
            yield return null;
        }
    }

}
