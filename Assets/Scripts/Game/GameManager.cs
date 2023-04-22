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

    public GameObject playerTeam;
    public GameObject aiTeam;

    public List<GameObject> playerTurnQueue = new List<GameObject>();
    public List<GameObject> enemyTurnQueue = new List<GameObject>();

    public bool isLoggingOn = true;
    public static bool _isDebugModeOn;

    public int currentPlayerId;
    public int currentAIId;

    public Dictionary<int, UnitScript> idsToPlayerUnits = new Dictionary<int, UnitScript>();
    public Dictionary<int, UnitScript> idsToAIUnits = new Dictionary<int, UnitScript>();

    public int playerCount;
    public int enemyCount;


    public void Awake()
    {
        UnitScript.nextAvailablePlayerId = 0;
        UnitScript.nextAvailableAIId = 0;
        currentPlayerId = 0;
        currentAIId = -1;
    }
    public void Start()
    {
        TM = GetComponent<TileMap>();
        _isDebugModeOn = isLoggingOn;
        addAllUnitsToDictionary();
        FirstTurn();
    }

    public void FirstTurn()
    {
        currentTeam = Team.Player;
        TM.SelectUnit(idsToPlayerUnits[0].gameObject);
    }

    public void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            CursorUiUpdate();
        }
    }

    public static void Log(string message)
    {
        if (_isDebugModeOn)
        {
            Debug.Log(message);
        }
    }

    public static void LogState(int depth, UnitScript unit, double score)
    {
        string message = depth + ", " + unit.UnitName + " / " ;
        foreach (UnitState state in unit.states)
        {
            message += "(" + state.x + ", " + state.y + ") ";
        }
        Log(message + ", " + score);
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

    public void addAllUnitsToDictionary()
    {
        foreach (Transform u in playerTeam.transform)
        {
            if (u.gameObject.activeInHierarchy)
            {
                UnitScript unit = u.gameObject.GetComponent<UnitScript>();

                idsToPlayerUnits.Add(unit.id, unit);
            }
        }
        foreach (Transform u in aiTeam.transform)
        {
            if (u.gameObject.activeInHierarchy)
            {
                UnitScript unit = u.gameObject.GetComponent<UnitScript>();

                idsToAIUnits.Add(unit.id, unit);
            }
        }
    }

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

    public void NextTurn()
    {
        if (currentTeam == Team.AI)
        {
            currentTeam = Team.Player;
            int i = currentPlayerId + 1;
            while (!idsToPlayerUnits.ContainsKey(i) || idsToPlayerUnits[i].IsDead)
            {
                if (i >= UnitScript.PlayerCount - 1)
                {
                    i = 0;
                }
                else
                {
                    i++;
                }
            }
            currentPlayerId = i;
            TM.SelectUnit(idsToPlayerUnits[currentPlayerId].gameObject);
        }
        else if (currentTeam == Team.Player)
        {
            currentTeam = Team.AI;
            int i = currentAIId + 1;
            while (!idsToAIUnits.ContainsKey(i) || idsToAIUnits[i].IsDead)
            {
                if (i >= UnitScript.AICount - 1)
                {
                    i = 0;
                }
                else
                {
                    i++;
                }
            }
            currentAIId = i;
            TM.SelectUnit(idsToAIUnits[currentAIId].gameObject);
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
            teamToReturn = playerTeam;
        }
        else if (team == 1)
        {
            teamToReturn = aiTeam;
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
        foreach (Transform u in playerTeam.transform)
        {
            if (u.gameObject.activeInHierarchy)
            {
                if (!u.GetComponent<UnitScript>().IsDead)
                {
                    playerTeamAlive = true;
                    break;
                }
            }
        }
        foreach (Transform u in aiTeam.transform)
        {
            if (u.gameObject.activeInHierarchy)
            {
                if (!u.GetComponent<UnitScript>().IsDead)
                {
                    enemyTeamAlive = true;
                    break;
                }
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

    public GameState IsGameOver()
    {
        bool teamHasUnit = false;
        foreach (var idToPlayer in idsToPlayerUnits)
        {
            if (!idToPlayer.Value.IsDead)
            {
                teamHasUnit = true;
                break;
            }
        }

        if (!teamHasUnit)
        {
            return GameState.Lose;
        }

        foreach (var idToAi in idsToAIUnits)
        {
            if (!idToAi.Value.IsDead)
            {
                return GameState.InGame;
            }
        }

        return GameState.Win;
    }




}
