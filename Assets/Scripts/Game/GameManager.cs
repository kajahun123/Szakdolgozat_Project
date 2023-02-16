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

    public GameObject team1;
    public GameObject team2;

    public List<GameObject> turnQueue = new List<GameObject>();

    public void Start()
    {
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
        currentTeam = GetCurrentTeam(turnQueue[0]);
        turnQueue[0].GetComponent<UnitScript>().isTurn = true;
        TM.SelectUnit(turnQueue[0]);   
    }

    public void EndTurn()
    {
        turnQueue[0].GetComponent<UnitScript>().isTurn = false;

        for (int i = 0; i < turnQueue.Count; i++)
        {
            if (turnQueue[i].GetComponent<UnitScript>().isDead)
            {
                turnQueue.RemoveAt(i);
                //azért kell csökkenteni az i-t hogy ne ugorjuk át a következõ elemet
                i--;
            }
        }

        turnQueue.Add(turnQueue[0]);
        turnQueue.RemoveAt(0);
        currentTeam = GetCurrentTeam(turnQueue[0]);
        turnQueue[0].GetComponent<UnitScript>().isTurn = true;
        //Debug.Log("CurrentUnit: " + turnQueue[0].GetComponent<UnitScript>().UnitName);
        TM.SelectUnit(turnQueue[0]);
        
    }

    public void PreviousTurn()
    {
        turnQueue[0].GetComponent<UnitScript>().isTurn = false;
        turnQueue.Insert(0,turnQueue[turnQueue.Count-1]);
        turnQueue.RemoveAt(turnQueue.Count-1);
        currentTeam = GetCurrentTeam(turnQueue[0]);
        turnQueue[0].GetComponent<UnitScript>().isTurn = true;
        //Debug.Log("CurrentUnit(previous): " +  turnQueue[0].GetComponent<UnitScript>().UnitName);
        TM.SelectUnit(turnQueue[0]);
    }

    public int GetCurrentTeam(GameObject selectedUnit)
    {
        return selectedUnit.GetComponent<UnitScript>().team;
    }

    public GameObject ReturnCurrenTeamGameObject(int team)
    {
        GameObject teamToReturn = null;
        if (team == 0)
        {
            teamToReturn = team1;
        }
        else if (team == 1)
        {
            teamToReturn = team2;
        }

        return teamToReturn;
    }

    public void cechkIfUnitsRemain(GameObject unit, GameObject enemy)
    {
        StartCoroutine(checkIfUnitsRemainedCoroutine(unit, enemy));

    }

    public IEnumerator checkIfUnitsRemainedCoroutine(GameObject unit, GameObject enemy)
    {
        while (unit.GetComponent<UnitScript>().combatQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }

        while (enemy.GetComponent<UnitScript>().combatQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        //Debug.Log("team: " + team1.transform.);
        bool playerTeamAlive = false;
        bool enemyTeamAlive = false;
        foreach (Transform u in team1.transform)
        {
            //Debug.Log("Win: " + u.GetComponent<UnitScript>().UnitName);
            if (!u.GetComponent<UnitScript>().isDead)
            {
                playerTeamAlive = true;
                break;
            }
        }
        foreach (Transform u in team2.transform)
        {
            //Debug.Log("Win: " + u.GetComponent<UnitScript>().UnitName);
            if (!u.GetComponent<UnitScript>().isDead)
            {
                enemyTeamAlive = true;
                break;
            }
        }

        if (playerTeamAlive == false)
        {
            yield return -1;
        }
        else if (enemyTeamAlive == false)
        { 
            yield return 1;
        }
    }

    public int checkWin()
    {
        bool playerTeamAlive = false;
        bool enemyTeamAlive = false;
        foreach (Transform u in team1.transform)
        {
            //Debug.Log("Win: " + u.GetComponent<UnitScript>().UnitName);
            if (!u.GetComponent<UnitScript>().isDead)
            {
                playerTeamAlive = true;
                break;
            }
        }
        foreach (Transform u in team2.transform)
        {
            //Debug.Log("Win: " + u.GetComponent<UnitScript>().UnitName);
            if (!u.GetComponent<UnitScript>().isDead)
            {
                enemyTeamAlive = true;
                break;
            }
        }

        if (playerTeamAlive == false)
        {
            return 1;
        }
        else if (enemyTeamAlive == false)
        {
            return -1;
        }
        return 0;
    }

    public double evaluateScore(TileMap map)
    {
        double totalScore = 0;
        double score = 0;
        foreach (Transform u in team1.transform)
        {
            if (!u.GetComponent<UnitScript>().isDead)
            {
                score -= 10;
            }
        }
        
        totalScore += score;
        //AI
        foreach (Transform u in team2.transform)
        {
            if (!u.GetComponent<UnitScript>().isDead)
            {
                score += 10;
            }
        }
        Debug.Log("Név: " + map.selectedUnit.GetComponent<UnitScript>().UnitName);
        totalScore = totalScore + score;
        return totalScore;
    }


}
