using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Controls tooltips etc
public class CursorProps : MonoBehaviour
{
    public enum CursorType { main, building, destroy, aim }
    public CursorType cursorType;
    public GameObject mainCursor;
    public GameObject buildingCursor;
    public GameObject destroyCursor;
    public GameObject aimingCursor;

    // Start is called before the first frame update
    void Start()
    {
        //Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = Input.mousePosition;
        SwitchCursors();
    }

    void SwitchCursors()
    {
        if (cursorType == CursorType.main)
        {
            mainCursor.SetActive(true);
            buildingCursor.SetActive(false);
            destroyCursor.SetActive(false);
            aimingCursor.SetActive(false);
        }
        else if (cursorType == CursorType.building)
        {
            mainCursor.SetActive(false);
            buildingCursor.SetActive(true);
            destroyCursor.SetActive(false);
            aimingCursor.SetActive(false);
        }
        else if (cursorType == CursorType.destroy)
        {
            mainCursor.SetActive(false);
            buildingCursor.SetActive(false);
            destroyCursor.SetActive(true);
            aimingCursor.SetActive(false);
        }
        else if (cursorType == CursorType.aim)
        {
            mainCursor.SetActive(false);
            buildingCursor.SetActive(false);
            destroyCursor.SetActive(false);
            aimingCursor.SetActive(true);
        }
    }

    void SetGraphics()
    {
        
    }
}
