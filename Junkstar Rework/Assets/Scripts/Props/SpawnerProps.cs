using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerProps : MonoBehaviour
{
    public GameObject mobToSpawn;
    public int minSpawn;
    public int maxSpawn;
    public int maxMultiplier;
    public int spawnCount;
    public bool CanDestroy;
    public GameObject triggeredEffect;

    private bool isTriggered;
    public float spawnRadius;
    public float spawnTimer;

    private bool readyToSpawn;
    private float timer;
    //public TextMeshProUGUI uiTimer;

    private void Start()
    {
        readyToSpawn = false;
        timer = spawnTimer;
    }

    private void Update()
    {
        //If the player has woken up the spawner
        if (isTriggered)
        {
            if (readyToSpawn)
            {
                if (spawnCount <= minSpawn)
                {
                    timer = spawnTimer;
                    //uiTimer.gameObject.SetActive(true);
                    readyToSpawn = false;
                }
            }
            else
            {
                if (timer > 0)
                {
                    timer -= Time.deltaTime;
                    //uiTimer.gameObject.SetActive(true);
                    //uiTimer.text = Mathf.Round(timer) + " seconds";
                }
                else
                {
                    //uiTimer.gameObject.SetActive(false);
                    SpawnWave();
                    readyToSpawn = true;
                }
            }
        }
    }

    void SpawnWave()
    {
        for (int i = 0; i < maxSpawn; i++)
        {
            Vector2 newPos = transform.position;
            Vector2 spawnPos = newPos + Random.insideUnitCircle * spawnRadius;
            GameObject newMob = Instantiate(mobToSpawn, spawnPos, transform.rotation, transform);
            Instantiate(triggeredEffect, newMob.transform.position, transform.rotation);
            newMob.GetComponent<EnemyProps>().parentSpawner = gameObject;
            spawnCount += 1;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && isTriggered == false)
        {
            isTriggered = true;
        }
    }
}