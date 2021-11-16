using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipProps : MonoBehaviour
{

    string[] shipNameA = { "Raging ", "Restless ", "Star of ", "Eye of ", "The ", "SS ", "Atala-", "Orphan ", "Dawn", "Morning ", "Midnight "};
    string[] shipNameB = { "Storm", "Duchess", "Tokyo", "Jupiter", "Starlight", "Beauty", "Warrior", "Ocean", "Advent", "Trojan", "Pride", "Danger" };

    public enum shipSize { small, medium, large }
    public enum shipQuality { poor, average, good }
    public enum shipEnemy { raider, bot, mutant }

    public string shipName;
    public shipSize size;
    public shipQuality quality;
    public shipEnemy enemy;

    private void Start()
    {
        GenShip();
    }

    //Generate the ships stats
    void GenShip()
    {
        //Generate the name
        shipName = shipNameA[Random.Range(0, shipNameA.Length)] + shipNameB[Random.Range(0, shipNameB.Length)];

        //Generate the size
        size = (shipSize)Random.Range(0, 2);

        //Generate the loot
        quality = (shipQuality)Random.Range(0, 2);

        //Generate the enemyType
        enemy = (shipEnemy)Random.Range(0, 2);
    }

}
