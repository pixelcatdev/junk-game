using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretProps : MonoBehaviour
{
    public enum TurretType { Enemy, Player };
    public TurretType turretType;
    public float aggroRadius;
    public Transform aggroCollider;
    public GameObject projectile;
    private float cooldownTimer;
    public float gunCooldown;
    public List<GameObject> currentTargets;

    public Transform turretObj;
    private Transform targetObj;

    //private PowerProps powerProps;

    private void Start()
    {
        //powerProps = GetComponent<PowerProps>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (powerProps.isPowered() == true)
        //{
            GetTargets();
        //}
    }

    public void GetTargets()
    {
        if (currentTargets.Count > 0)
        {
            //Get the current targets position (if it exists - it might've been destroyed by another turret)
            if (currentTargets[0] != null)
            {
                targetObj = currentTargets[0].transform;

                //rotate to face the current target and track it
                Vector2 currentPos = turretObj.position;
                Vector2 targetPos = targetObj.position;

                Vector2 direction = currentPos - targetPos;
                direction.Normalize();
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                turretObj.rotation = Quaternion.Euler(Vector3.forward * (angle));
            }
        }

        //shoot at the most recent enemy if there's any within range - otherwise slowly rotate to signify active
        if (currentTargets.Count > 0)
        {
            if (currentTargets[0] != null)
            {
                ShootAt(currentTargets[0]);
            }
        }
        else
        {
            //turretObj.transform.Rotate(0, 0, 25f * Time.deltaTime);
        }
    }

    public void ShootAt(GameObject target)
    {
        //cast a ray at target
        //Ray2D hit = Physics2D.Raycast();
        Vector2 currentPos = transform.position;
        Vector2 targetPos = target.transform.position;
        Vector2 direction = targetPos - currentPos;
        direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Vector2 rayAngle = GetDirectionVector2D(angle);

        //fire at the target if the ray hits
        LayerMask unitsLayer = LayerMask.GetMask("Enemy", "ShipTileWall");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayAngle, aggroRadius, unitsLayer);
        Debug.DrawRay(transform.position, rayAngle * aggroRadius, Color.red, 0.05f);

        //if the ray hits the currentTarget
        if (hit)
        {
            if (turretType == TurretType.Player)
            {
                if (hit.transform.gameObject.tag == "isEnemy")
                {
                    var pos = target.transform.position;

                    if (Time.time > cooldownTimer)
                    {
                        var q = Quaternion.FromToRotation(Vector3.up, pos - transform.position);
                        var go = Instantiate(projectile, transform.position, q);
                        go.GetComponent<Rigidbody2D>().AddForce(go.transform.up * 500);

                        cooldownTimer = Time.time + gunCooldown;
                    }
                }
            }
            else if (turretType == TurretType.Enemy)
            {
                if (hit.transform.gameObject.tag == "Player")
                {
                    var pos = target.transform.position;

                    if (Time.time > cooldownTimer)
                    {
                        var q = Quaternion.FromToRotation(Vector3.left, pos - transform.position);
                        var go = Instantiate(projectile, transform.position, q);
                        go.GetComponent<Rigidbody2D>().AddForce(go.transform.up * 500);

                        cooldownTimer = Time.time + gunCooldown;
                    }
                }
            }

        }

    }

    public Vector2 GetDirectionVector2D(float angle)
    {
        return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
    }
}
