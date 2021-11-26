using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OverworldController : MonoBehaviour
{
    public Transform mapScreen;
    public Transform playerShip;
    public GameObject wreckShip;
    public GameObject ui_Selector;

    public List<GameObject> shipwrecks;
    public GameObject spaceStation;
    private int curTarget;
    private bool canSwitchTarget;

    public bool isTravelling;
    public float travelSpeed;
    public ParticleSystem starfieldFx;

    public TextMeshProUGUI txt_shipName;
    public TextMeshProUGUI txt_shipSize;
    public TextMeshProUGUI txt_shipQuality;
    public TextMeshProUGUI txt_shipEnemy;
    public TextMeshProUGUI txt_shipDistance;

    private void Start()
    {
        //Scan();
        canSwitchTarget = true;
    }

    private void Update()
    {
        //Debug for setting new Scans
        if (GameController.instance.gameState == GameController.GameState.overworld)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                if (shipwrecks.Count > 0)
                {
                    ClearScan();
                }
                Scan();
                SwitchTarget();
            }

            //Debug for travelling to selected target
            if (Input.GetKeyDown(KeyCode.T))
            {
                //Turn off any UI elements
                playerShip.gameObject.GetComponent<LineRenderer>().enabled = false;
                ui_Selector.SetActive(false);

                isTravelling = true;

                //If already docked with a generated map, undock and clear the map
                TargetShipController.instance.playerShipIsDocked = false;
                if (TargetShipController.instance.generatedMap)
                {
                    TargetShipController.instance.MapClear();
                }
            }

            //Debug for quitting overworld screen
            if (Input.GetKeyDown(KeyCode.Q))
            {
                GameController.instance.SwitchToGame();
            }

            SelectTarget();
        }

        Travel();
    }

    //Scan for new shipwrecks
    public void Scan()
    {
        //Reset playerShip Position
        playerShip.position = transform.position;

        //Add space station to list
        shipwrecks.Add(spaceStation);

        //Randomise shipwrecks
        int randomWreckTotal = Random.Range(30, 50);

        for (int i = 0; i < randomWreckTotal; i++)
        {
            Vector2 newPos = new Vector2(playerShip.position.x, playerShip.position.y) + Random.insideUnitCircle * 4f;
            GameObject newWreck = Instantiate(wreckShip, newPos, mapScreen.transform.rotation, transform.GetChild(0));

            shipwrecks.Add(newWreck);
        }

        //Enable UI elements
        EnableScanUI();


    }

    private void EnableScanUI()
    {
        //Enable UI elements
        playerShip.gameObject.GetComponent<LineRenderer>().enabled = true;
        ui_Selector.SetActive(true);

        //Place cursor at first shipwreck
        curTarget = 0;
        ui_Selector.transform.position = shipwrecks[curTarget].transform.position;
        UpdateTargetUI();
    }

    //Clears the map and removes all shipwrecks
    public void ClearScan()
    {
        for (int i = 0; i < shipwrecks.Count; i++)
        {
            //If not a station
            if (!shipwrecks[i].GetComponent<OverWorldShipProps>().isStation)
            {
                Destroy(shipwrecks[i]);
            }
        }
        shipwrecks.Clear();
    }   

    //Navigate through the map
    public void SelectTarget()
    {
        if (shipwrecks.Count > 0 && canSwitchTarget && !isTravelling)
        {
            float dirX = Input.GetAxisRaw("Horizontal");

            //Navigate right
            if (dirX > 0)
            {
                if (curTarget + 1 > shipwrecks.Count - 1)
                {
                    curTarget = 0;
                }
                else
                {
                    curTarget += 1;
                }

                SwitchTarget();

            }

            //Navigate right
            else if (dirX < 0)
            {
                if (curTarget - 1 < 0)
                {
                    curTarget = shipwrecks.Count - 1;
                }
                else
                {
                    curTarget -= 1;
                }

                SwitchTarget();
            }
        }        
    }

    void SwitchTarget()
    {
        ui_Selector.transform.position = shipwrecks[curTarget].transform.position;
        playerShip.gameObject.GetComponent<LineRenderer>().SetPosition(0, playerShip.transform.position);
        playerShip.gameObject.GetComponent<LineRenderer>().SetPosition(1, shipwrecks[curTarget].transform.position);
        canSwitchTarget = false;
        StartCoroutine("SwitchTargetReset");
        UpdateTargetUI();
    }

    void UpdateTargetUI()
    {
        txt_shipName.text = shipwrecks[curTarget].GetComponent<OverWorldShipProps>().shipName.ToUpper();
        txt_shipSize.text = shipwrecks[curTarget].GetComponent<OverWorldShipProps>().size.ToString().ToUpper();
        txt_shipQuality.text = shipwrecks[curTarget].GetComponent<OverWorldShipProps>().quality.ToString().ToUpper();
        txt_shipEnemy.text = shipwrecks[curTarget].GetComponent<OverWorldShipProps>().enemy.ToString().ToUpper();

        //If the target isnt the trading station, get the ships stats and pass them into TargetShipMap so it can create the appropriate map (quality, size, enemy etc)
        if (!shipwrecks[curTarget].gameObject.GetComponent<OverWorldShipProps>().isStation)
        {
            TargetShipController.instance.shipName = shipwrecks[curTarget].GetComponent<OverWorldShipProps>().shipName.ToUpper();
            TargetShipController.instance.size = shipwrecks[curTarget].GetComponent<OverWorldShipProps>().size.ToString();
            TargetShipController.instance.quality = shipwrecks[curTarget].GetComponent<OverWorldShipProps>().quality.ToString();
            TargetShipController.instance.enemy = shipwrecks[curTarget].GetComponent<OverWorldShipProps>().enemy.ToString();
            TargetShipController.instance.isStation = false;
        }
        else
        {
            TargetShipController.instance.isStation = true;
            TargetShipController.instance.size = "Trading Station";
            TargetShipController.instance.quality = null;
            TargetShipController.instance.enemy = null;

        }        

        //Calculate Distance
        float distance = Vector2.Distance(playerShip.position, shipwrecks[curTarget].transform.position);

        //Calculate fuel
        //Fuel consumption per unit * distance vs Current Fuel
    }

    //Move the player towards the target
    void Travel()
    {
        //Move the player ship towards the target location
        if (isTravelling)
        {
            float distance = Vector3.Distance(playerShip.position, shipwrecks[curTarget].transform.position);

            if (distance > 0f)
            {
                playerShip.position = Vector2.MoveTowards(playerShip.transform.position, shipwrecks[curTarget].transform.position, travelSpeed * Time.deltaTime);
            }
            else
            {
                ReachDestination();
            }

        }
    }

    //Triggers om arrival at target location
    private void ReachDestination()
    {        
        isTravelling = false;
        canSwitchTarget = true;
        TargetShipController.instance.MapGen();
    }

    //Stops the shipwreck selector spamming through all available wrecks
    IEnumerator SwitchTargetReset()
    {
        yield return new WaitForSeconds(0.25f);
        canSwitchTarget = true;
    }

}
