using Assets.Scripts.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private RaycastHit hit;
    private Ray ray;
    public GameObject tileBeingDisplayed;

    public TileMap TM;

    public Team currentTeam;
    public int numberOfTeams = 2;

    public int selectedXTile;
    public int selectedYTile;

    public int cursorX;
    public int cursorY;

    public GameObject team1;
    public GameObject team2;

    public List<GameObject> playerTurnQueue = new List<GameObject>();
    public List<GameObject> enemyTurnQueue = new List<GameObject>();

    public bool isLoggingOn = true;
    private static bool _isLoggingOn;

    public int currentPlayerId;
    public int currentAIId;

    public Dictionary<int, UnitScript> idsToPlayerUnits;
    public Dictionary<int, UnitScript> idsToAIUnits;

    public int maxPlayerId;
    public int maxAIId;

    public void Start()
    {
        currentTeam = Team.Player;
        TM = GetComponent<TileMap>();
        //addAllUnitsToQueue();
        _isLoggingOn = isLoggingOn;
        addAllUnitToQueue();
    }

    public void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            CursorUiUpdate();
        }

       /*
        if (playerTurnQueue[0].GetComponent<UnitScript>().isTurn == true)
        {
            TM.SelectUnit(playerTurnQueue[0]);
        }
        if (enemyTurnQueue[0].GetComponent<UnitScript>().isTurn == true)
        {
            TM.SelectUnit(enemyTurnQueue[0]);
        }*/
    }

    public static void Log(string message)
    {
        if (_isLoggingOn)
        {
            Debug.Log(message);
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

    public void addAllUnitToQueue()
    {
        foreach (Transform u in team1.transform)
        {
            UnitScript unit = u.gameObject.GetComponent<UnitScript>();
            if (unit.id > maxPlayerId)
            {
                maxPlayerId = unit.id;
            }

            idsToPlayerUnits.Add(unit.id, unit);

        }
        foreach (Transform u in team2.transform)
        {
            UnitScript unit = u.gameObject.GetComponent<UnitScript>();
            if (unit.id > maxPlayerId)
            {
                maxAIId = unit.id;
            }

            idsToAIUnits.Add(unit.id, unit);
        }

        idsToPlayerUnits[0].isTurn = true;
        //gameobjectbe rakni
        TM.SelectUnit(idsToPlayerUnits[0]);
    }

    /*public void addAllUnitsToQueue()
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
    }*/

    public void switchCurrentPlayer()
    {
        if(currentTeam == Team.Player)
        {
            currentTeam = Team.AI;
        }
        else if(currentTeam == Team.AI)
        {
            currentTeam = Team.Player;
        }

    }

    public void nextUnit()
    {

    }

    public void EndTurn()
    {
        if (currentTeam == Team.Player)
        {
            playerTurnQueue[0].GetComponent<UnitScript>().isTurn = false;
        }
        else if (currentTeam == Team.AI)
        {
            enemyTurnQueue[0].GetComponent<UnitScript>().isTurn = false;
        }

        switchCurrentPlayer();
        if (currentTeam == Team.Player)
        {
            GameObject selectedUnit;
            Debug.LogWarning("bent0");
            playerTurnQueue.Add(playerTurnQueue[0]);
            playerTurnQueue.RemoveAt(0);
            for (int i = 0; i < playerTurnQueue.Count; i++)
            {
                if (!playerTurnQueue[i].GetComponent<UnitScript>().IsDead)
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
        else if (currentTeam == Team.AI)
        {
            GameObject selectedUnit;
            Debug.LogWarning("bent1");
            enemyTurnQueue.Add(enemyTurnQueue[0]);
            enemyTurnQueue.RemoveAt(0);
            for (int i = 0; i < enemyTurnQueue.Count; i++)
            {
                if (!enemyTurnQueue[i].GetComponent<UnitScript>().IsDead)
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

    private void NextTurn()
    {
        if (currentTeam == Team.AI)
        {
            currentTeam = Team.Player;
            int i = currentPlayerId + 1;
            while (!idsToPlayerUnits.ContainsKey(i) || idsToPlayerUnits[i].VIsDead)
            {
                if (currentPlayerId == maxPlayerId)
                {
                    i = 0;
                }
                else
                {
                    i++;
                }
            }
            currentPlayerId = i;
            TM.SelectUnit(idsToPlayerUnits[currentPlayerId]);
            //átadni a tileMapnak az id-t
        }
        else if (currentTeam == Team.Player)
        {
            currentTeam = Team.AI;
            int i = currentAIId + 1;
            while (!idsToAIUnits.ContainsKey(i) || idsToAIUnits[i].IsDead)
            {
                if (currentAIId == maxAIId)
                {
                    i = 0;
                }
                else
                {
                    i++;
                }
            }
            
            currentAIId = i;
            TM.SelectUnit(idsToAIUnits[currentAIId]);
            //átadni a tileMapnak az id-t
        }
    }

    public Team GetCurrentTeam(GameObject selectedUnit)
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
            if (!u.GetComponent<UnitScript>().IsDead)
            {
                playerTeamAlive = true;
                break;
            }
        }
        foreach (Transform u in team2.transform)
        {
            //Debug.Log("Win: " + u.GetComponent<UnitScript>().UnitName);
            if (!u.GetComponent<UnitScript>().IsDead)
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
            if (!u.GetComponent<UnitScript>().IsDead)
            {
                playerTeamAlive = true;
                break;
            }
        }
        foreach (Transform u in team2.transform)
        {
            //Debug.Log("Win: " + u.GetComponent<UnitScript>().UnitName);
            if (!u.GetComponent<UnitScript>().IsDead)
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
