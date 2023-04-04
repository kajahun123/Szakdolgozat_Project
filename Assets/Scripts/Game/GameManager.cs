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

    public List<GameObject> playerTurnQueue = new List<GameObject>();
    public List<GameObject> enemyTurnQueue = new List<GameObject>();

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

       
        if (playerTurnQueue[0].GetComponent<UnitScript>().isTurn == true)
        {
            TM.SelectUnit(playerTurnQueue[0]);
        }
        if (enemyTurnQueue[0].GetComponent<UnitScript>().isTurn == true)
        {
            TM.SelectUnit(enemyTurnQueue[0]);
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
        foreach (Transform u in team1.transform)
        {
            playerTurnQueue.Add(u.gameObject);
            
        }
        foreach (Transform u in team2.transform)
        {
            enemyTurnQueue.Add(u.gameObject);
        }

        playerTurnQueue[0].GetComponent<UnitScript>().isTurn = true;
        TM.SelectUnit(playerTurnQueue[0]);
    }

    /* public void addAllUnitsToQueue()
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

     } */

    public void switchCurrentPlayer()
    {
        currentTeam++;
        if (currentTeam == numberOfTeams)
        {
            currentTeam = 0;
        }

    }

    public void EndTurn()
    {
        if (currentTeam == 0)
        {
            playerTurnQueue[0].GetComponent<UnitScript>().isTurn = false;
        }
        else if (currentTeam == 1)
        {
            enemyTurnQueue[0].GetComponent<UnitScript>().isTurn = false;
        }

        switchCurrentPlayer();
        if (currentTeam == 0)
        {
            GameObject selectedUnit;
            Debug.LogWarning("bent0");
            playerTurnQueue.Add(playerTurnQueue[0]);
            playerTurnQueue.RemoveAt(0);
            for (int i = 0; i < playerTurnQueue.Count; i++)
            {
                if (!playerTurnQueue[i].GetComponent<UnitScript>().isDead)
                {
                    //playerTurnQueue.RemoveAt(i);
                    //azért kell csökkenteni az i-t hogy ne ugorjuk át a következõ elemet
                    // i--;
                    selectedUnit = playerTurnQueue[i];
                    selectedUnit.GetComponent<UnitScript>().isTurn = true;
                    TM.SelectUnit(selectedUnit);
                    break;
                }
                playerTurnQueue.Add(playerTurnQueue[i]);
                playerTurnQueue.RemoveAt(0);
            }

        }
        else if (currentTeam == 1)
        {
            GameObject selectedUnit;
            Debug.LogWarning("bent1");
            enemyTurnQueue.Add(enemyTurnQueue[0]);
            enemyTurnQueue.RemoveAt(0);
            for (int i = 0; i < enemyTurnQueue.Count; i++)
            {
                if (!enemyTurnQueue[i].GetComponent<UnitScript>().isDead)
                {
                    selectedUnit = enemyTurnQueue[i];
                    selectedUnit.GetComponent<UnitScript>().isTurn = true;
                    TM.SelectUnit(selectedUnit);
                    break;
                }
                enemyTurnQueue.Add(enemyTurnQueue[i]);
                enemyTurnQueue.RemoveAt(0);
            }
        }


    }

    public void PreviousTurn()
    {
        if (currentTeam == 0)
        {
            playerTurnQueue[0].GetComponent<UnitScript>().isTurn = false;
        }
        else if (currentTeam == 1)
        {
            enemyTurnQueue[0].GetComponent<UnitScript>().isTurn = false;
        }
        switchCurrentPlayer();
        if (currentTeam == 0)
        {
            Debug.LogWarning("reset0");
            GameObject selectedUnit;
            for (int i = playerTurnQueue.Count - 1; i >= 0; i--)
            {
                if (!playerTurnQueue[i].GetComponent<UnitScript>().isDead)
                {
                    //playerTurnQueue.RemoveAt(i);
                    //azért kell csökkenteni az i-t hogy ne ugorjuk át a következõ elemet
                    // i--;
                    selectedUnit = playerTurnQueue[i];
                    playerTurnQueue.Add(playerTurnQueue[0]);
                    playerTurnQueue.RemoveAt(0);
                    selectedUnit.GetComponent<UnitScript>().isTurn = true;
                    playerTurnQueue.Insert(0, selectedUnit);
                    TM.SelectUnit(selectedUnit);
                    break;
                }
            }
        }
        else if (currentTeam == 1)
        {
            Debug.LogWarning("reset1");
            GameObject selectedUnit;
            for (int i = 0; i < enemyTurnQueue.Count; i++)
            {
                if (!enemyTurnQueue[i].GetComponent<UnitScript>().isDead)
                {
                    selectedUnit = enemyTurnQueue[i];
                    enemyTurnQueue.Add(enemyTurnQueue[0]);
                    enemyTurnQueue.RemoveAt(0);
                    selectedUnit.GetComponent<UnitScript>().isTurn = true;
                    enemyTurnQueue.Insert(0, selectedUnit);
                    TM.SelectUnit(selectedUnit);
                    break;
                }
            }
        }
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

    


}
