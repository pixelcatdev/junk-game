using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverWorldShipProps : MonoBehaviour
{
    const string stringGen = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";


    public enum shipSize { small, medium, large }
    public enum shipQuality { poor, average, good }
    public enum shipEnemy { raider, bot, mutant }

    public string shipName;
    public shipSize size;
    public shipQuality quality;
    public shipEnemy enemy;

    public bool isStation;
    public bool isBoss;

    private void Start()
    {
        GenShip();
    }

    //Generate the ships stats
    void GenShip()
    {
        if (!isStation)
        {
            //Generate the name
            int charAmount = 12; //Random.Range(minCharAmount, maxCharAmount); //set those to the minimum and maximum length of your string
            for (int i = 0; i < charAmount; i++)
            {
                shipName += stringGen[Random.Range(0, stringGen.Length)];
            }

            //Generate the size
            size = (shipSize)Random.Range(0, 3);

            //Generate the loot
            quality = (shipQuality)Random.Range(0, 3);

            //Generate the enemyType
            enemy = (shipEnemy)Random.Range(0, 3);
        }

    }

}
