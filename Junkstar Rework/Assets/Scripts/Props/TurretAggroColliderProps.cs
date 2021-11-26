using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAggroColliderProps : MonoBehaviour
{
    private Transform parentTurret;

    // Start is called before the first frame update
    void Start()
    {
        parentTurret = transform.parent;
        GetComponent<CircleCollider2D>().radius = parentTurret.GetComponent<TurretProps>().aggroRadius;
    }

    //If enemy enters the radius, mark it as the current target and tell the turret to start attacking
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //If its a player owned turret
        if (parentTurret.GetComponent<TurretProps>().turretType == TurretProps.TurretType.Player)
        {
            if (collision.gameObject.tag == "isEnemy")
            {
                parentTurret.GetComponent<TurretProps>().currentTargets.Add(collision.gameObject);
            }
        }

        //else if its a mob owned turret
        else if (parentTurret.GetComponent<TurretProps>().turretType == TurretProps.TurretType.Enemy)
        {
            if (collision.gameObject.tag == "Player")
            {
                parentTurret.GetComponent<TurretProps>().currentTargets.Add(collision.gameObject);
            }

        }
    }

    //If enemy exits the radius and it's still the turrets current target, tell the turret to stop attacking and clear the currentTarget
    private void OnTriggerExit2D(Collider2D collision)
    {
        //If its a player owned turret
        if (parentTurret.GetComponent<TurretProps>().turretType == TurretProps.TurretType.Player)
        {
            if (collision.gameObject.tag == "isEnemy")
            {
                parentTurret.GetComponent<TurretProps>().currentTargets.Remove(collision.gameObject);
            }
        }

        //else if its a mob owned turret
        else if (parentTurret.GetComponent<TurretProps>().turretType == TurretProps.TurretType.Enemy)
        {

            if (collision.gameObject.tag == "Player")
            {
                parentTurret.GetComponent<TurretProps>().currentTargets.Remove(collision.gameObject);
            }
        }
    }
}
