using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingProps : MonoBehaviour
{
    //Building details
    public string buildingName;
    public string buildingInfo;
    public Sprite buildingBlueprint;
    public bool needsFloor;
    public List<BuildingRecipe> buildingRecipe = new List<BuildingRecipe>();

}

[System.Serializable]
public class BuildingRecipe
{
    public GameObject lootObject;
    public int itemCost;
    public bool canAfford;

    public BuildingRecipe(GameObject _lootObject, int _itemCost, bool _canAfford)
    {
        _lootObject = lootObject;
        _itemCost = itemCost;
        _canAfford = canAfford;
    }
}