﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractorProps : MonoBehaviour
{
    public enum InteractorType { bridge, system, playerAirlock, shipAirlock, crate, door, npc }
    public InteractorType interactorType;

    public void Activate()
    {
        switch (interactorType)
        {
            //Launch Overworld Map
            case InteractorType.bridge:
                GameController.instance.ToggleWorlds();
                break;

            //Launch System Repair List
            case InteractorType.system:
                break;

            //Transport Player to Dungeon
            case InteractorType.playerAirlock:
                if (TargetShipController.instance.playerShipIsDocked)
                {
                    TargetShipController.instance.EnterShip();
                }
                else
                {
                    Debug.Log("Cannot use airlock when ship is not docked");
                }
                
                break;

            //Transport Player to Junkstar
            case InteractorType.shipAirlock:
                TargetShipController.instance.ExitShip();
                break;

            //Open Inventory & Crate UI
            case InteractorType.crate:
                break;

            //Toggle Door open/close
            case InteractorType.door:
                break;

            //Trigger npc dialogue
            case InteractorType.npc:
                break;

            default:
                break;
        }
    }
}