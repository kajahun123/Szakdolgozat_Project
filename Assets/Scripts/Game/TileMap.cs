
using Assets.Scripts.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileMap : MonoBehaviour
{

    public GameManager GM;
    public BattleManager BM;

    public Tile[] tileTypes;
    public int[,] tiles;

    public GameObject unitsOnBoard;
    public int mapSizeX;
    public int mapSizeY;

    public GameObject selectedUnit;

    public Node[,] graph;

    public GameObject[,] tilesOnMap;

    public GameObject[,] quadOnMap;
    public GameObject[,] quadOnMapForUnitMovement;
    public GameObject[,] quadOnMapCursor;
    public GameObject[,] quadOnMapCurrentUnit;

    public GameObject mapUI;
    public GameObject mapCursorUI;
    public GameObject mapUnitMovementUI;
    public GameObject currentUnitUI;

    public GameObject TileContainer;
    public GameObject UIQuadPotentialMovesContainer;
    public GameObject UIQuadCursorContainer;
    public GameObject UIUnitMovementPathContainer;
    public GameObject UICurrentUnitContainer;

    public bool unitSelected = false;

    public HashSet<Node> selectedUnitMoveRange;
    public HashSet<Node> selectedUnitAttackRange;

    public List<Node> currentPath = null;

    public int unitSelectedPreviousX;
    public int unitSelectedPreviousY;

    public GameObject previousOccupiedTile;

    public Material greenUIMat;
    public Material redUIMat;
    public Material blueUIMat;

    [Header("AI")]
    public Minimax AI;
    public GameObject tempNode;
    public GameObject tempTile;
    public GameObject tempUnit;
    public GameObject tempEnemy;
    public int tempHP;
    public int depth = 3;

    private bool isGameOver;

    public bool isAIWorking;

    public bool inMoving;

    private void Awake()
    {
        isGameOver = false;
        isAIWorking = false;
        inMoving = false;
        GenerateMapData();
        generatePathFindingGraph();
        GenerateMapVisuals();
        setIfTileIsOccupied();
    }

    private void Update()
    {
        if (GM.IsGameOver() != GameState.InGame)
        {
            isGameOver = true;
            if(GM.IsGameOver() == GameState.Win)
            {
                SceneManager.LoadScene(3, LoadSceneMode.Single);
            }
            else
            {
                SceneManager.LoadScene(2, LoadSceneMode.Single);
            }

            return;
        }

        if (isGameOver || inMoving || BM.battleStatus)
        {
            return;
        }

        if (GM.currentTeam == Team.Player && Input.GetMouseButtonDown(0) || GM.currentTeam == Team.AI && !isAIWorking)
        {
            //Ha nincs kiválasztva egy egység sem
            if (selectedUnit == null)
            {
                Debug.Log("Nincs unit");
            }
            else if (selectedUnit.GetComponent<UnitScript>().unitMovementState == selectedUnit.GetComponent<UnitScript>().GetMovementState(1)
                && selectedUnit.GetComponent<UnitScript>().movementQueue.Count == 0)
            {
                
                selectTileToDoAction();
            }
        }
    }
    public void GenerateMapData()
    {
        tiles = new int[mapSizeX, mapSizeY];

        //legeneráljuk az összes mezõt és Grass értéket adunk neki
        //GRASS = 0
        //WALL = 1
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                if (x < mapSizeX-2 && x > 1 && y < mapSizeY-1 && y > 0)
                {
                    if (tiles[x + 1, y] == 0 & tiles[x - 1, y] == 0 && tiles[x, y + 1] == 0 && tiles[x, y - 1] == 0)
                    {
                        tiles[x, y] = UnityEngine.Random.Range(0, 5) == 3 ? 1 : 0;
                    }
                }
            }
        }
    }

    public void GenerateMapVisuals()
    {

        tilesOnMap = new GameObject[mapSizeX, mapSizeY];
        quadOnMap = new GameObject[mapSizeX, mapSizeY];
        quadOnMapCursor = new GameObject[mapSizeX, mapSizeY];
        quadOnMapForUnitMovement = new GameObject[mapSizeX, mapSizeY];
        quadOnMapCurrentUnit = new GameObject[mapSizeX, mapSizeY];
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                GameObject newTile = Instantiate(tileTypes[tiles[x, y]].tileVisualPrefab, new Vector3(x, 0, y), Quaternion.identity);
                newTile.GetComponent<ClickableTile>().tileX = x;
                newTile.GetComponent<ClickableTile>().tileY = y;
                newTile.GetComponent<ClickableTile>().map = this;
                newTile.GetComponent<ClickableTile>().isWalkable = tileTypes[tiles[x, y]].isWalkable;
                newTile.transform.SetParent(TileContainer.transform);
                tilesOnMap[x, y] = newTile;

                GameObject gridUI = Instantiate(mapUI, new Vector3(x, 0.501f, y), Quaternion.Euler(90f, 0, 0));
                gridUI.transform.SetParent(UIQuadPotentialMovesContainer.transform);
                quadOnMap[x, y] = gridUI;

                GameObject gridUICursor = Instantiate(mapCursorUI, new Vector3(x, 0.503f, y), Quaternion.Euler(90f, 0, 0));
                gridUICursor.transform.SetParent(UIQuadCursorContainer.transform);
                quadOnMapCursor[x, y] = gridUICursor;

                GameObject gridUICurrentUnit = Instantiate(currentUnitUI, new Vector3(x, 0.503f, y), Quaternion.Euler(90f, 0, 0));
                gridUICurrentUnit.transform.SetParent(UICurrentUnitContainer.transform);
                quadOnMapCurrentUnit[x, y] = gridUICurrentUnit;
            }
        }
    }

    public void generatePathFindingGraph()
    {
        graph = new Node[mapSizeX, mapSizeY];

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                graph[x, y] = new Node();
                graph[x, y].x = x;
                graph[x, y].y = y;
            }
        }

        //szomszédok kiszámolása
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                // ha az X nagyobb mint 0 akkor adhatunk hozzzá bal szomszédot
                if (x > 0)
                {
                    graph[x, y].neighbours.Add(graph[x - 1, y]);
                }
                //ha az X kisebb mint a mapSizeX - 1 akkor adhatunk hozzá jobb szomszédot
                if (x < mapSizeX - 1)
                {
                    graph[x, y].neighbours.Add(graph[x + 1, y]);
                }
                //ha az Y nagyobb mint 0 akkor adhatunk hozzá alsó szomszédot
                if (y > 0)
                {
                    graph[x, y].neighbours.Add(graph[x, y - 1]);
                }
                // ha az Y kisebb mint mapSizeY -1 akkor adhatunk hozzá felsõ szomszédot
                if (y < mapSizeY - 1)
                {
                    graph[x, y].neighbours.Add(graph[x, y + 1]);
                }

            }
        }
    }


    public Vector3 tileCoordinateToWorldCoordinate(int x, int y)
    {
        return new Vector3(x, 0.75f, y);
    }

    public void setIfTileIsOccupied()
    {
        foreach (Transform team in unitsOnBoard.transform)
        {

            foreach (Transform unitOnTeam in team)
            {
                if (unitOnTeam.gameObject.activeInHierarchy)
                {
                    var unit = unitOnTeam.GetComponent<UnitScript>();
                    int unitX = unit.x;
                    int unitY = unit.y;
                    unit.tileBeingOccupied = tilesOnMap[unitX, unitY];
                    tilesOnMap[unitX, unitY].GetComponent<ClickableTile>().unitOnTile = unitOnTeam.gameObject;
                }
            }
        }
    }

    public List<GameObject> getAllUnits()
    {
        List<GameObject> tempList = new List<GameObject>();
        foreach (Transform team in unitsOnBoard.transform)
        {

            foreach (Transform unitOnTeam in team.transform)
            {
                if (unitOnTeam.gameObject.activeInHierarchy)
                {
                    tempList.Add(unitOnTeam.gameObject);
                }
            }
        }
        return tempList;
    }

    //Egység mozgatása
    public void moveUnit(Action callBack)
    {
        if (selectedUnit != null)
        {
            inMoving = true;
            selectedUnit.GetComponent<UnitScript>().MoveToNextTile(callBack);
        }
    }



    public void SelectUnit(GameObject unit)
    {
        selectedUnit = unit;
        selectedUnit.GetComponent<UnitScript>().map = this;
        selectedUnit.GetComponent<UnitScript>().SetMovementState(1);
        unitSelected = true;
        highlightUnitRange();
    }
    public void finalizeMovementPosition()
    {
        inMoving = false;
        if (GM.currentTeam == Team.AI)
        {
            isAIWorking = false;
        }
        tilesOnMap[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y].GetComponent<ClickableTile>().unitOnTile = selectedUnit;
        //Moved állapotba tesszük
        selectedUnit.GetComponent<UnitScript>().SetMovementState(2);
        deselectUnit();
        if (GM.IsGameOver() == GameState.InGame)
        {
            GM.NextTurn();
        }
    }


    public void deselectUnit()
    {
        if (selectedUnit != null)
        {
            if (selectedUnit.GetComponent<UnitScript>().unitMovementState == selectedUnit.GetComponent<UnitScript>().GetMovementState(1))
            {
                disableHighlightCurrentUnit();
                disableHighlightMovementRange();
                selectedUnit.GetComponent<UnitScript>().SetMovementState(0);
                selectedUnit = null;
                unitSelected = false;
            }
            else if (selectedUnit.GetComponent<UnitScript>().unitMovementState == selectedUnit.GetComponent<UnitScript>().GetMovementState(2))
            {
                disableHighlightCurrentUnit();
                disableHighlightMovementRange();
                selectedUnit = null;
                unitSelected = false;
            }
        }
    }

    public void selectTileToDoAction()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        HashSet<Node> attackableTiles = getUnitAttackOptions();

        if (selectedUnit.GetComponent<UnitScript>().team == Team.Player)
        {
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject.CompareTag("Tile"))
                {
                    int clickedTileX = hit.transform.GetComponent<ClickableTile>().tileX;
                    int clickedTileY = hit.transform.GetComponent<ClickableTile>().tileY;
                    Node nodeToCeck = graph[clickedTileX, clickedTileY];
                    UnitScript unit = selectedUnit.GetComponent<UnitScript>();
                    if ((hit.transform.gameObject.GetComponent<ClickableTile>().unitOnTile == null || hit.transform.gameObject.GetComponent<ClickableTile>().unitOnTile == selectedUnit) && (selectedUnitMoveRange.Contains(nodeToCeck)))
                    {
                        disableHighlightCurrentUnit();
                        disableHighlightMovementRange();
                        generatePathTo(clickedTileX, clickedTileY);
                        moveUnit(finalizeMovementPosition);
                    }
                    else if (hit.transform.gameObject.GetComponent<ClickableTile>().unitOnTile != null && (hit.transform.gameObject.GetComponent<ClickableTile>().unitOnTile.GetComponent<UnitScript>().team != selectedUnit.GetComponent<UnitScript>().team) && (attackableTiles.Contains(nodeToCeck)))
                    {
                        if (hit.transform.gameObject.GetComponent<ClickableTile>().unitOnTile.GetComponent<UnitScript>().currentHealthPoints > 0)
                        {
                            disableHighlightCurrentUnit();
                            disableHighlightMovementRange();
                            StartCoroutine(BM.Attack(selectedUnit, hit.transform.gameObject.GetComponent<ClickableTile>().unitOnTile, finalizeMovementPosition));
                        }
                    }
                    else if (unit.x == clickedTileX && unit.y == clickedTileY)
                    {
                        disableHighlightCurrentUnit();
                        disableHighlightMovementRange();
                        generatePathTo(clickedTileX, clickedTileY);
                        moveUnit(finalizeMovementPosition);
                    }

                }
                else if (hit.transform.parent.gameObject.CompareTag("Unit"))
                {
                    UnitScript unit = selectedUnit.GetComponent<UnitScript>();
                    GameObject unitClicked = hit.transform.parent.gameObject;
                    int unitX = unitClicked.GetComponent<UnitScript>().x;
                    int unitY = unitClicked.GetComponent<UnitScript>().y;
                    if (unitClicked.GetComponent<UnitScript>().team != selectedUnit.GetComponent<UnitScript>().team && attackableTiles.Contains(graph[unitX, unitY]))
                    {
                        if (unitClicked.GetComponent<UnitScript>().currentHealthPoints > 0)
                        {
                            disableHighlightCurrentUnit();
                            disableHighlightMovementRange();
                            StartCoroutine(BM.Attack(selectedUnit, unitClicked, finalizeMovementPosition));
                        }
                    }
                    else if (unit.x == unitX && unit.y == unitY)
                    {
                        disableHighlightCurrentUnit();
                        disableHighlightMovementRange();
                        generatePathTo(unitX, unitY);
                        moveUnit(finalizeMovementPosition);
                    }
                }
            }
        }
        else if (selectedUnit.GetComponent<UnitScript>().team == Team.AI)
        {
            isAIWorking = true;
            UnitScript unit = selectedUnit.GetComponent<UnitScript>();
            Position bestMove = AI.MinMax(this, true, depth, double.MinValue, double.MaxValue, GM.currentAIId, GM.currentPlayerId);
            if (tilesOnMap[bestMove.x, bestMove.y].GetComponent<ClickableTile>().unitOnTile == null)
            {
                generatePathTo(bestMove.x, bestMove.y);
                moveUnit(finalizeMovementPosition);
            }
            else if (tilesOnMap[bestMove.x, bestMove.y].GetComponent<ClickableTile>().unitOnTile != null && tilesOnMap[bestMove.x, bestMove.y].GetComponent<ClickableTile>().unitOnTile.GetComponent<UnitScript>().team != unit.team)
            {
                StartCoroutine(BM.Attack(selectedUnit, tilesOnMap[bestMove.x, bestMove.y].GetComponent<ClickableTile>().unitOnTile, finalizeMovementPosition));
            }
            else if (bestMove.x == unit.x && bestMove.y == unit.y)
            {
                generatePathTo(bestMove.x, bestMove.y);
                moveUnit(finalizeMovementPosition);
            }
        }
    }

    public HashSet<Node> getUnitMovementOptions()
    {
        float[,] cost = new float[mapSizeX, mapSizeY];
        HashSet<Node> UIHighlight = new HashSet<Node>();
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();
        HashSet<Node> tempUIHighlight = new HashSet<Node>();
        int moveSpeed = selectedUnit.GetComponent<UnitScript>().movementRange;
        Node unitInitialNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];

        finalMovementHighlight.Add(unitInitialNode);
        foreach (Node n in unitInitialNode.neighbours)
        {
            cost[n.x, n.y] = costToEnterTile(n.x, n.y);
            if (moveSpeed - cost[n.x, n.y] >= 0)
            {
                UIHighlight.Add(n);
            }
        }

        finalMovementHighlight.UnionWith(UIHighlight);

        while (UIHighlight.Count != 0)
        {
            foreach (Node n in UIHighlight)
            {
                foreach (Node neighbour in n.neighbours)
                {
                    if (!finalMovementHighlight.Contains(neighbour))
                    {
                        cost[neighbour.x, neighbour.y] = costToEnterTile(neighbour.x, neighbour.y) + cost[n.x, n.y];
                        if (moveSpeed - cost[neighbour.x, neighbour.y] >= 0)
                        {
                            tempUIHighlight.Add(neighbour);
                        }
                    }
                }
            }

            UIHighlight = tempUIHighlight;
            finalMovementHighlight.UnionWith(UIHighlight);
            tempUIHighlight = new HashSet<Node>();
        }
        return finalMovementHighlight;
    }



    public HashSet<Node> getUnitAttackOptions()
    {
        HashSet<Node> tempNeighbourHas = new HashSet<Node>();
        HashSet<Node> neighbourHash = new HashSet<Node>();
        HashSet<Node> seenNodes = new HashSet<Node>();
        Node initalNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];
        int attRange = selectedUnit.GetComponent<UnitScript>().attackRange;

        neighbourHash = new HashSet<Node>();
        neighbourHash.Add(initalNode);
        for (int i = 0; i < attRange; i++)
        {
            foreach (Node t in neighbourHash)
            {
                foreach (Node tn in t.neighbours)
                {
                    tempNeighbourHas.Add(tn);
                }
            }
            neighbourHash = tempNeighbourHas;
            tempNeighbourHas = new HashSet<Node>();
            if (i < attRange - 1)
            {
                seenNodes.UnionWith(neighbourHash);
            }
        }
        neighbourHash.ExceptWith(seenNodes);
        neighbourHash.Remove(initalNode);
        return neighbourHash;
    }

    public HashSet<Node> getUnitTotalAttackableTiles(HashSet<Node> finalMovementHighlight, int attRange, Node unitInitalNode)
    {
        HashSet<Node> tempNeighbourHash = new HashSet<Node>();
        HashSet<Node> neighbourHash = new HashSet<Node>();
        HashSet<Node> seenNodes = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();
        foreach (Node n in finalMovementHighlight)
        {
            neighbourHash = new HashSet<Node>();
            neighbourHash.Add(n);
            for (int i = 0; i < attRange; i++)
            {
                foreach (Node t in neighbourHash)
                {
                    foreach (Node tn in t.neighbours)
                    {
                        tempNeighbourHash.Add(tn);
                    }
                }

                neighbourHash = tempNeighbourHash;
                tempNeighbourHash = new HashSet<Node>();
                if (i < attRange - 1)
                {
                    seenNodes.UnionWith(neighbourHash);
                }
            }
            neighbourHash.ExceptWith(seenNodes);
            seenNodes = new HashSet<Node>();
            totalAttackableTiles.UnionWith(neighbourHash);
        }
        totalAttackableTiles.Remove(unitInitalNode);
        return totalAttackableTiles;
    }

    public bool unitCanEnterTile(int x, int y)
    {
        //ha ellenfél van ott, ahova lépni akarunk akkor false-t dob vissza
        if (tilesOnMap[x, y].GetComponent<ClickableTile>().unitOnTile != null)
        {
            if (tilesOnMap[x, y].GetComponent<ClickableTile>().unitOnTile.GetComponent<UnitScript>().team != selectedUnit.GetComponent<UnitScript>().team)
            {
                return false;
            }
        }
        //ha rálehet lépni a mezõre akkor true
        return tileTypes[tiles[x, y]].isWalkable;
    }

    public void highlightUnitRange()
    {
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();
        finalMovementHighlight = getUnitMovementOptions();

        highlightCurrentUnit();
        highlightMovementRange(finalMovementHighlight);
        highlightUnitAttackOption();


        selectedUnitMoveRange = finalMovementHighlight;
    }

    public void highlightCurrentUnit()
    {
        if (selectedUnit != null)
        {
            quadOnMapCurrentUnit[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y].GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public void disableHighlightCurrentUnit()
    {
        foreach (GameObject quad in quadOnMapCurrentUnit)
        {
            if (quad.GetComponent<Renderer>().enabled == true)
            {
                quad.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    public void highlightMovementRange(HashSet<Node> movement)
    {
        foreach (Node n in movement)
        {
            quadOnMap[n.x, n.y].GetComponent<Renderer>().material = blueUIMat;
            quadOnMap[n.x, n.y].GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public void highlightEnemiesInRange(HashSet<Node> enemies)
    {
        foreach (Node n in enemies)
        {
            quadOnMap[n.x, n.y].GetComponent<Renderer>().material = redUIMat;
            quadOnMap[n.x, n.y].GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public void highlightUnitAttackOption()
    {
        if (selectedUnit != null)
        {
            highlightEnemiesInRange(getUnitAttackOptions());
        }
    }

    public void disableHighlightMovementRange()
    {
        foreach (GameObject quad in quadOnMap)
        {
            if (quad.GetComponent<Renderer>().enabled == true)
            {
                quad.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    public float costToEnterTile(int x, int y)
    {
        //Ha nem tud belépni akkor végtelen

        if (unitCanEnterTile(x, y) == false)
        {
            return Mathf.Infinity;
        }

        Tile t = tileTypes[tiles[x, y]];
        float distance = t.movementCost;
        return distance;
    }

    public void generatePathTo(int x, int y)
    {
        //Ugyan oda kattintott mint ahol a unit áll
        if (selectedUnit.GetComponent<UnitScript>().x == x && selectedUnit.GetComponent<UnitScript>().y == y)
        {
            currentPath = new List<Node>();
            selectedUnit.GetComponent<UnitScript>().path = currentPath;

            return;
        }

        //nem tudunk oda lépni, visszatérünk
        if (unitCanEnterTile(x, y) == false)
        {
            return;
        }

        selectedUnit.GetComponent<UnitScript>().path = null;
        currentPath = null;
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];
        Node target = graph[x, y];
        dist[source] = 0;
        prev[source] = null;

        List<Node> unvisited = new List<Node>();

        foreach (Node n in graph)
        {
            //mindegyiket beallitjuk végtelenre
            if (n != source)
            {
                dist[n] = Mathf.Infinity;
                prev[n] = null;
            }
            //hozzáadjuk a nem bejártakhoz õket
            unvisited.Add(n);
        }
        while (unvisited.Count > 0)
        {
            //a legrövidebb nembejart node
            Node u = null;

            foreach (Node posibbleU in unvisited)
            {
                if (u == null || dist[posibbleU] < dist[u])
                {
                    u = posibbleU;
                }
            }

            if (u == target)
            {
                break;
            }

            unvisited.Remove(u);

            foreach (Node n in u.neighbours)
            {
                float alt = dist[u] + costToEnterTile(n.x, n.y);
                if (alt < dist[n])
                {

                    dist[n] = alt;
                    prev[n] = u;
                }
            }
        }
        if (prev[target] == null)
        {
            return;
        }
        currentPath = new List<Node>();
        Node curr = target;

        //hozzáadjuk a céltól a startig
        while (curr != null)
        {
            currentPath.Add(curr);
            curr = prev[curr];
        }
        //megforditjuk
        currentPath.Reverse();
        selectedUnit.GetComponent<UnitScript>().path = currentPath;
    }

    public HashSet<Node> getActualMovementOptions(GameObject unit)
    {
        HashSet<Node> legalMoves = new HashSet<Node>();
        Debug.Log("Selected unit: " + selectedUnit + ", x: " + selectedUnit.GetComponent<UnitScript>().y);
        Debug.Log("unit: " + unit + ", x: " + unit.GetComponent<UnitScript>().y);
        Node unitInitialNode = graph[unit.GetComponent<UnitScript>().x, unit.GetComponent<UnitScript>().y];
        Node node;
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                node = graph[x, y];
                if (selectedUnitMoveRange.Contains(node) && unitInitialNode != node)
                {
                    legalMoves.Add(node);
                }

            }
        }
        return legalMoves;
    }

}
