using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProps : MonoBehaviour
{
    public enum EnemyType { sentry, ranged, melee }
    public EnemyType enemyType;

    public string enemyName;
    private float curHealth;
    public float maxHealth;
    public float speed;
    public GameObject projectile;
    public float attackDamage;
    public float attackRate;

    private void Start()
    {
        curHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        //Deduct object health until destroyed
        if (curHealth - damage > 0)
        {
            curHealth -= damage;
        }
        else
        {
            Destroy(gameObject);
        }

        //Draw healthbar
        //healthBar.fillAmount = curHealth / maxHealth;
    }
}
