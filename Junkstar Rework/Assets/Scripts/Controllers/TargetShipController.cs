using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Holds all generated map info; structure, power, enemy type etc
public class TargetShipController : MonoBehaviour
{
    public float mapHealth;
    public float mapPower;
    public enum EnemyType { raider, bot, mutant }
    public EnemyType enemy;



    public GameObject shipMap;
    public Transform airlock;

    //Spawn Map
    //Small, Medium or Large
    //Apply damage based on quality
    //Apply crates based on quality
    //Apply explodables based on quality
    //Place spawners and set the enemy type accordingly


}
