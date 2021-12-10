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
    public GameObject playerShipMap;

    public List<GameObject> shipwrecks;
    public GameObject spaceStation;
    public Transform selectedTarget;
    public int targetDistance;
    public int fuelRate;
    private int fuelCost;

    public int minWrecks;
    public int maxWrecks;

    public bool isTravelling;
    public bool isTravelPaused;
    private bool eventTriggered;
    public float travelSpeed;

    public enum EventType { DebrisStrike, BoardingParty, ShipToShip, TraderShip }
    public EventType eventType;
    public string eventTxt;

    public bool isEvent;
    private float eventDistanceTrigger;

    public GameObject ui_Alert;
    public GameObject ui_EventPrompt;

    public TextMeshProUGUI txt_shipName;
    public TextMeshProUGUI txt_shipSize;
    public TextMeshProUGUI txt_shipQuality;
    public TextMeshProUGUI txt_shipEnemy;
    public TextMeshProUGUI txt_shipDistance;
    public TextMeshProUGUI txt_fuelCost;
    public TextMeshProUGUI txt_playerShipCurHealth;
    public TextMeshProUGUI txt_playerShipFuel;

    public TextMeshProUGUI txt_eventPrompt;

    public static OverworldController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!OverworldController.instance)
        {
            OverworldController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        Scan();
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

            txt_playerShipCurHealth.text = "SHIP: " + Mathf.RoundToInt((playerShipMap.GetComponent<ShipMapProps>().mapCurHealth / playerShipMap.GetComponent<ShipMapProps>().mapMaxHealth) * 100).ToString() + "%";
            txt_playerShipFuel.text = "FUEL: " + Mathf.RoundToInt((playerShipMap.GetComponent<ShipMapProps>().mapCurFuel / 100) * 100).ToString() + "%";
        }

        Travel();
        TravelEvents();
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
            Vector2 newPos = new Vector2(playerShip.position.x, playerShip.position.y) + Random.insideUnitCircle * 8f;
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
                    //If player has enough fuel
                    if (HasFuel())
                    {
                        //ClearOverWorldTarget();
                        isTravelling = true;

                        //Enable Ion Engine effects
                        for (int i = 0; i < PlayerShipController.instance.effects_ion.Count; i++)
                        {
                            PlayerShipController.instance.effects_ion[i].SetActive(true);
                        }

                        //Set the randomised event trigger distance
                        eventDistanceTrigger = Random.Range(0, targetDistance - 0.25f);
                        Debug.Log("Event will trigger at " + eventDistanceTrigger);

                        //If already docked with a generated map, undock and clear the map
                        TargetShipController.instance.playerShipIsDocked = false;
                        if (TargetShipController.instance.generatedMap)
                        {
                            TargetShipController.instance.MapClear();
                        }
                    }                    
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

    //Determines if the player ship has enough fuel
    bool HasFuel()
    {
        if(playerShipMap.GetComponent<ShipMapProps>().mapCurFuel - fuelCost >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }        
    }

    //Move the ship Selector to the current ship, set the line renderer to target the current ship and display the ships stats in the stats UI
    void DisplayOverWorldTarget (Transform target)
    {
        //Set the selectedTarget and distance
        selectedTarget = target;
        targetDistance = Mathf.RoundToInt(Vector3.Distance(playerShip.position, selectedTarget.position));
        fuelCost = fuelRate * targetDistance;

        //Update selector and targeting line
        GameController.instance.selectorOverWorld.SetActive(true);
        GameController.instance.selectorOverWorld.transform.position = target.position;
        playerShip.gameObject.GetComponent<LineRenderer>().enabled = true;
        playerShip.gameObject.GetComponent<LineRenderer>().SetPosition(0, playerShip.transform.position);
        playerShip.gameObject.GetComponent<LineRenderer>().SetPosition(1, target.position);

        //if the player has enough fuel
        if (HasFuel())
        {
            playerShip.gameObject.GetComponent<LineRenderer>().startColor = Color.green;
            playerShip.gameObject.GetComponent<LineRenderer>().endColor = Color.green;
            txt_fuelCost.text = "FUEL COST: " + fuelCost.ToString() + "%";
        }
        else
        {
            playerShip.gameObject.GetComponent<LineRenderer>().startColor = Color.red;
            playerShip.gameObject.GetComponent<LineRenderer>().endColor = Color.red;
            txt_fuelCost.text = "FUEL COST: " + fuelCost + "% REQUIRED";
        }

        //Update txt UI elements displaying ship info

        OverWorldShipProps shipStats = target.GetComponent<OverWorldShipProps>();

        txt_shipName.text = "LICENSE ID: " + shipStats.shipName.ToUpper();
        txt_shipSize.text = "CLASS: " + shipStats.size.ToString().ToUpper();
        txt_shipQuality.text = "QUALITY: " + shipStats.quality.ToString().ToUpper();
        txt_shipEnemy.text = "LIFESIGNS: " + shipStats.enemy.ToString().ToUpper();
        txt_shipDistance.text = "DISTANCE: " + Mathf.RoundToInt(targetDistance).ToString() + "KM";      

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
    }

    //Hide UI targeting info
    void ClearOverWorldTarget ()
    {
        GameController.instance.selectorOverWorld.SetActive(false);
        playerShip.gameObject.GetComponent<LineRenderer>().enabled = false;

        txt_shipName.text = "LICENSE ID: - ";
        txt_shipSize.text = "CLASS: - ";
        txt_shipQuality.text = "QUALITY: - ";
        txt_shipEnemy.text = "LIFESIGNS: - ";
        txt_shipDistance.text = "DISTANCE: - ";
        txt_fuelCost.text = "FUEL COST: - ";
    }

    //Move the player towards the target
    void Travel()
    {
        //Move the player ship towards the target location
        if (isTravelling && !isTravelPaused)
        {
            float distance = Vector3.Distance(playerShip.position, selectedTarget.position);
            playerShip.gameObject.GetComponent<LineRenderer>().SetPosition(0, playerShip.transform.position);

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

    void TravelEvents()
    {
        if (isTravelling && !eventTriggered)
        {
            float curDistance = Vector3.Distance(playerShip.position, selectedTarget.position);
            if (curDistance <= eventDistanceTrigger)
            {
                //randomise chance of event based on player ship scanners value
                float eventChance = Random.Range(1, 100);
                if(eventChance > 50)
                {
                    GenEvent();
                }
                eventTriggered = true;
            }
        }        
    }

    string EventText()
    {
        string txt = null;

        if(eventType == EventType.BoardingParty)
        {
            txt = "RAIDER BOARDING PARTY INBOUND";
        }
        else if (eventType == EventType.DebrisStrike)
        {
            txt = "COLLISION COURSE WITH DEBRIS FIELD DETECTED";
        }
        else if (eventType == EventType.ShipToShip)
        {
            txt = "HOSTILE SHIP ON APPROACH VECTOR";
        }
        else if (eventType == EventType.TraderShip)
        {
            txt = "PASSING SALVAGE SHIP HAILING FOR TRADE";
        }

        return txt;
    }

    //Generates the travel event
    void GenEvent()
    {
        Debug.Log("Event generating");
        //Randomise type of event
        //switch to event type accordingly
        eventType = (EventType)Random.Range(0, 4);

        //If the player scanner has been upgraded
        if (PlayerShipController.instance.GetComponent<ShipMapProps>().scanner > 50)
        {
            isTravelPaused = true;
            //activate alert to the player somehow
            //Set the txt for the event
            ui_EventPrompt.SetActive(true);
            txt_eventPrompt.text = EventText();
        }
        else
        {
            EngageEvent();
        }
    }

    //Fires the travel event (prompted either automatically or if pressed from the Overworld prompt)
    public void EngageEvent()
    {
        ui_EventPrompt.SetActive(false);

        switch (eventType)
        {
            case EventType.DebrisStrike:
                Debug.Log("Debris has struck the ship dealing X damage");
                //Generate a cloud of debris that'll move over and under the ship, damaging X random tiles in the process
                break;
            case EventType.BoardingParty:
                isTravelPaused = true;
                Debug.Log("Hijackers on approach vector. Prepare for boarding party assault.");
                //Pick a random floor tile
                StartCoroutine("BoardingParty");
                break;
            case EventType.ShipToShip:
                isTravelPaused = true;
                Debug.Log("Combat Raider on approach vector. Prepare for Ship to Ship combat.");
                //Initiate Ship Combat
                ShipCombatController.instance.CombatSetup();
                break;
            default:
                break;
        }
    }

    public void EvadeEvent()
    {
        //Display that you evade the event
        ui_EventPrompt.SetActive(false);
        isTravelPaused = false;
    }

    //Triggers om arrival at target location
    private void ReachDestination()
    {
        //Disable Ion Engine effects
        for (int i = 0; i < PlayerShipController.instance.effects_ion.Count; i++)
        {
            PlayerShipController.instance.effects_ion[i].SetActive(false);
        }

        playerShipMap.GetComponent<ShipMapProps>().mapCurFuel -= fuelCost;
        isTravelling = false;
        TargetShipController.instance.MapGen();
        ClearOverWorldTarget();
        selectedTarget = null;
        eventTriggered = false;
    }

    IEnumerator BoardingParty()
    {
        yield return new WaitForSeconds(5);
        Debug.Log("Raiders boarded");
        PlayerShipController playerShip = playerShipMap.GetComponent<PlayerShipController>();
        Transform airlock = playerShip.airlock;
        //Spawn X boarders at this position
        //Only end the event when all boarders are defeated
        isTravelPaused = false;
    }

    IEnumerator DebrisField()
    {
        yield return new WaitForSeconds(5);
        Debug.Log("Debris strike");

        //Destroy x random tiles
        //Spawn X boarders at this position
        //Only end the event when all boarders are defeated
        isTravelPaused = false;
    }
}
