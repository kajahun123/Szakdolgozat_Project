using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private RaycastHit hit;
    private Ray ray;
    public GameObject tileBeingDisplayed;

    public TileMap TM;

    public int currentTeam;
    public int numberOfTeams = 2;

    public int selectedXTile;
    public int selectedYTile;

    public int cursorX;
    public int cursorY;

    public List<GameObject> turnQueue = new List<GameObject>();

    public void Start()
    {
        currentTeam = 0;
        TM = GetComponent<TileMap>();
        addAllUnitsToQueue();
    }

    public void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            CursorUiUpdate();
        }


        if (turnQueue[0].GetComponent<UnitScript>().isTurn == true)
        {
            TM.SelectUnit(turnQueue[0]);
        }
    }
    public void CursorUiUpdate()
    {
        if (hit.transform.CompareTag("Tile"))
        {
            if (tileBeingDisplayed == null)
            {
                selectedXTile = hit.transform.gameObject.GetComponent<ClickableTile>().tileX;
                selectedYTile = hit.transform.gameObject.GetComponent<ClickableTile>().tileY;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                
                TM.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.gameObject;
                Debug.Log("tile1");
            }
            else if (tileBeingDisplayed != hit.transform.gameObject)
            {
                selectedXTile = tileBeingDisplayed.GetComponent<ClickableTile>().tileX;
                selectedYTile = tileBeingDisplayed.GetComponent<ClickableTile>().tileY;
                TM.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;
                
                selectedXTile = hit.transform.gameObject.GetComponent<ClickableTile>().tileX;
                selectedYTile = hit.transform.gameObject.GetComponent<ClickableTile>().tileY;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                TM.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.gameObject;
                Debug.Log("tile2");
            }
        }
        else if (hit.transform.parent.CompareTag("Unit"))
        {
            if (tileBeingDisplayed == null)
            {
                selectedXTile = hit.transform.parent.gameObject.GetComponent<UnitScript>().x;
                selectedYTile = hit.transform.parent.gameObject.GetComponent<UnitScript>().y;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                TM.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.parent.gameObject.GetComponent<UnitScript>().tileBeingOccupied;
                Debug.Log("unit1");
            }
            else if (tileBeingDisplayed != hit.transform.gameObject)
            {
                if (hit.transform.parent.gameObject.GetComponent<UnitScript>().movementQueue.Count == 0)
                {
                    selectedXTile = tileBeingDisplayed.GetComponent<ClickableTile>().tileX;
                    selectedYTile = tileBeingDisplayed.GetComponent<ClickableTile>().tileY;
                    TM.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;

                    selectedXTile = hit.transform.parent.gameObject.GetComponent<UnitScript>().x;
                    selectedYTile = hit.transform.parent.gameObject.GetComponent<UnitScript>().y;
                    cursorX = selectedXTile;
                    cursorY = selectedYTile;
                    TM.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                    tileBeingDisplayed = hit.transform.parent.gameObject.GetComponent<UnitScript>().tileBeingOccupied;
                    Debug.Log("unit2");
                }
            }
        }
        else
        {
            TM.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void addAllUnitsToQueue()
    {
        turnQueue.AddRange(GameObject.FindGameObjectsWithTag("Unit"));
        for (int i = 0; i < turnQueue.Count; i++)
        {
            GameObject temp = turnQueue[i];
            int randomIndex = Random.Range(i, turnQueue.Count);
            turnQueue[i] = turnQueue[randomIndex];
            turnQueue[randomIndex] = temp;
        }
        turnQueue[0].GetComponent<UnitScript>().isTurn = true;
        TM.SelectUnit(turnQueue[0]);
        Debug.Log("Starter unit: " + turnQueue[0].GetComponent<UnitScript>().name);
    }

    public void EndTurn()
    {
        turnQueue[0].GetComponent<UnitScript>().isTurn = false;
        turnQueue.Add(turnQueue[0]);
        turnQueue.RemoveAt(0);

        turnQueue[0].GetComponent<UnitScript>().isTurn = true;
        Debug.Log("Next unit: " + turnQueue[0].GetComponent<UnitScript>().name);
    }
}
