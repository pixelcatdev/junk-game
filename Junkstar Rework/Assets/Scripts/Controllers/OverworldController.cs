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

    public List<GameObject> shipwrecks;
    public GameObject spaceStation;
    public Transform selectedTarget;

    public int minWrecks;
    public int maxWrecks;

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
            }

            //Debug for travelling to selected target
            if (Input.GetKeyDown(KeyCode.T))
            {
                //Turn off any UI elements
                playerShip.gameObject.GetComponent<LineRenderer>().enabled = false;

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

            //SelectTarget();
            if (!isTravelling)
            {
                CursorOver();
            }            
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
        int randomWreckTotal = Random.Range(minWrecks, maxWrecks);

        for (int i = 0; i < randomWreckTotal; i++)
        {
            Vector2 newPos = new Vector2(playerShip.position.x, playerShip.position.y) + Random.insideUnitCircle * 4f;
            GameObject newWreck = Instantiate(wreckShip, newPos, mapScreen.transform.rotation, transform.GetChild(0));

            shipwrecks.Add(newWreck);
        }
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

    //Get ship info/set destination on mouse over
    void CursorOver()
    {
        LayerMask hitLayer = LayerMask.GetMask("OverWorld");
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0f, hitLayer);

        if (hit)
        {
            if (hit.transform.gameObject.GetComponent<OverWorldShipProps>())
            {
                //Display the Target UI
                DisplayOverWorldTarget(hit.transform);                

                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    isTravelling = true;
                    ClearOverWorldTarget();
                }
            }
            else
            {
                ClearOverWorldTarget();
            }
        }
        else
        {
            ClearOverWorldTarget();
        }
    }

    //Move the ship Selector to the current ship, set the line renderer to target the current ship and display the ships stats in the stats UI
    void DisplayOverWorldTarget (Transform target)
    {
        //Set the selectedTarget
        selectedTarget = target;

        //Update selector and targeting line
        GameController.instance.selectorOverWorld.SetActive(true);
        GameController.instance.selectorOverWorld.transform.position = target.position;
        playerShip.gameObject.GetComponent<LineRenderer>().enabled = true;
        playerShip.gameObject.GetComponent<LineRenderer>().SetPosition(0, playerShip.transform.position);
        playerShip.gameObject.GetComponent<LineRenderer>().SetPosition(1, target.position);

        //Update txt UI elements displaying ship info

        OverWorldShipProps shipStats = target.GetComponent<OverWorldShipProps>();

        txt_shipName.text = shipStats.shipName.ToUpper();
        txt_shipSize.text = shipStats.size.ToString().ToUpper();
        txt_shipQuality.text = shipStats.quality.ToString().ToUpper();
        txt_shipEnemy.text = shipStats.enemy.ToString().ToUpper();

        //If the target isnt the trading station, get the ships stats and pass them into TargetShipMap so it can create the appropriate map (quality, size, enemy etc)
        if (!shipStats.isStation)
        {
            TargetShipController.instance.shipName = shipStats.shipName.ToUpper();
            TargetShipController.instance.size = shipStats.size.ToString();
            TargetShipController.instance.quality = shipStats.quality.ToString();
            TargetShipController.instance.enemy = shipStats.enemy.ToString();
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
        float distance = Vector2.Distance(playerShip.position, target.position);

        //Calculate fuel
        //Fuel consumption per unit * distance vs Current Fuel
    }

    //Hide UI targeting info
    void ClearOverWorldTarget ()
    {
        GameController.instance.selectorOverWorld.SetActive(false);
        playerShip.gameObject.GetComponent<LineRenderer>().enabled = false;

        txt_shipName.text = null;
        txt_shipSize.text = null;
        txt_shipQuality.text = null;
        txt_shipEnemy.text = null;
    }

    //Move the player towards the target
    void Travel()
    {
        //Move the player ship towards the target location
        if (isTravelling)
        {
            float distance = Vector3.Distance(playerShip.position, selectedTarget.position);

            if (distance > 0f)
            {
                playerShip.position = Vector2.MoveTowards(playerShip.transform.position, selectedTarget.position, travelSpeed * Time.deltaTime);
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
        TargetShipController.instance.MapGen();
        ClearOverWorldTarget();
        selectedTarget = null;
    }
}
