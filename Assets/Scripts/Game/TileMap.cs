
using Assets.Scripts.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour
{

    //Cloneozható GameState
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


    private void Start()
    {

        GenerateMapData();
        generatePathFindingGraph();
        GenerateMapVisuals();
        setIfTileIsOccupied();
    }

    private void Update()
    {
        //Bal kattintás
        if (Input.GetMouseButtonDown(0))
        {
            //Ha nincs kiválasztva egy egység sem
            if (selectedUnit == null)
            {
                Debug.Log("Nincs unit");
                //mouseClickToSelectUnit();
            }
            else if (selectedUnit.GetComponent<UnitScript>().unitMovementState == selectedUnit.GetComponent<UnitScript>().GetMovementState(1)
                && selectedUnit.GetComponent<UnitScript>().movementQueue.Count == 0)
            {

                if (selectTileToDoAction())
                {
                    
                }
            }
            //TODO: kicserélni, hogy mozgással együtt legyen a támadás
           /* else if (selectedUnit.GetComponent<UnitScript>().unitMovementState == selectedUnit.GetComponent<UnitScript>().GetMovementState(2))
            {
                finalizeOption();
            }*/

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
                tiles[x, y] = Random.Range(0, 6) == 3 ? 1 : 0;
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
                GameObject newTile = Instantiate(tileTypes[tiles[x,y]].tileVisualPrefab, new Vector3(x, 0,y), Quaternion.identity);
                newTile.GetComponent<ClickableTile>().tileX = x;
                newTile.GetComponent<ClickableTile>().tileY = y;
                newTile.GetComponent<ClickableTile>().map = this;
                newTile.transform.SetParent(TileContainer.transform);
                tilesOnMap[x, y] = newTile;

               GameObject gridUI = Instantiate(mapUI, new Vector3(x, 0.501f, y), Quaternion.Euler(90f, 0, 0));
                gridUI.transform.SetParent(UIQuadPotentialMovesContainer.transform);
                quadOnMap[x, y] = gridUI;
                /* 
                GameObject gridUIForPathfindingDisplay = Instantiate(mapUnitMovementUI, new Vector3(x, 0.502f, y), Quaternion.Euler(90f, 0, 0));
                gridUIForPathfindingDisplay.transform.SetParent(UIUnitMovementPathContainer.transform);
                quadOnMapForUnitMovement[x, y] = gridUIForPathfindingDisplay; */

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
                var unit = unitOnTeam.GetComponent<UnitScript>();
                int unitX = unit.x;
                int unitY = unit.y;
                unit.tileBeingOccupied = tilesOnMap[unitX, unitY];
                tilesOnMap[unitX, unitY].GetComponent<ClickableTile>().unitOnTile = unitOnTeam.gameObject;
                UnitState unitState = new UnitState();
                unitState.healthPoint = unit.currentHealthPoints;
                unitState.occupiedTile = tilesOnMap[unitX, unitY];
                unitState.x = unitX;
                unitState.y = unitY;
                unit.states.Push(unitState);

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
                tempList.Add(unitOnTeam.gameObject);
            }
        }
        return tempList;
    }

    //Egység mozgatása
    public void moveUnit()
    {
        if (selectedUnit != null)
        {
            
            selectedUnit.GetComponent<UnitScript>().MoveToNextTile();
        }
    }

    public void mouseClickToSelectUnit()
    {
        if (unitSelected == false && GM.tileBeingDisplayed != null )
        {
            
            if (GM.tileBeingDisplayed.GetComponent<ClickableTile>().unitOnTile != null)
            {
               
                GameObject tempSelectedUnit = GM.tileBeingDisplayed.GetComponent<ClickableTile>().unitOnTile;
                if (tempSelectedUnit.GetComponent<UnitScript>().unitMovementState == tempSelectedUnit.GetComponent<UnitScript>().GetMovementState(0)
                    && tempSelectedUnit.GetComponent<UnitScript>().team == GM.currentTeam)
                {
                    
                    selectedUnit = tempSelectedUnit;
                    selectedUnit.GetComponent<UnitScript>().map = this;
                    selectedUnit.GetComponent<UnitScript>().SetMovementState(1);
                    unitSelected = true;
                    highlightUnitRange();
                }
            }
        }
    }

    public void SelectUnit(GameObject unit)
    {
        if (GM.tileBeingDisplayed != null)
        {
            selectedUnit = unit;
            selectedUnit.GetComponent<UnitScript>().map = this;
            selectedUnit.GetComponent<UnitScript>().SetMovementState(1);
            unitSelected = true;
            highlightUnitRange();
        }
    }

    public IEnumerator moveUnitAndFinalize()
    {
        while (selectedUnit.GetComponent<UnitScript>().movementQueue.Count !=0)
        {
            yield return new WaitForEndOfFrame();
        }
        
        finalizeMovementPosition();
    }

    public IEnumerator attackUnitAndFinalize(GameObject unit)
    {
        yield return new WaitForSeconds(.25f);
        while (unit.GetComponent<UnitScript>().combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        finalizeMovementPosition();
    }

    public void finalizeMovementPosition()
    {
        tilesOnMap[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y].GetComponent<ClickableTile>().unitOnTile = selectedUnit;
        //Moved állapotba tesszük
        selectedUnit.GetComponent<UnitScript>().SetMovementState(2);
        deselectUnit();
        GM.EndTurn();
    }

    public void finalizeOption()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.CompareTag("Tile"))
            {
                if (hit.transform.GetComponent<ClickableTile>().unitOnTile != null)
                {
                    GameObject unitOnTile = hit.transform.GetComponent<ClickableTile>().unitOnTile;
                    int unitX = unitOnTile.GetComponent<UnitScript>().x;
                    int unitY = unitOnTile.GetComponent<UnitScript>().y;
                    Debug.Log("Elõtte: " + selectedUnit.name);
                        deselectUnit();
                    Debug.Log("vége");
                }
                
            }
            else if (hit.transform.parent != null && hit.transform.parent.gameObject.CompareTag("Unit"))
            {
                GameObject unitClicked = hit.transform.parent.gameObject;
                int unitX = unitClicked.GetComponent<UnitScript>().x;
                int unitY = unitClicked.GetComponent<UnitScript>().y;

            }
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

    public bool selectTileToDoAction()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        HashSet<Node> attackableTiles = getUnitAttackOptions();

        if (selectedUnit.GetComponent<UnitScript>().team == 0)
        {
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject.CompareTag("Tile"))
                {
                    int clickedTileX = hit.transform.GetComponent<ClickableTile>().tileX;
                    int clickedTileY = hit.transform.GetComponent<ClickableTile>().tileY;
                    Node nodeToCeck = graph[clickedTileX, clickedTileY];

                    if ((hit.transform.gameObject.GetComponent<ClickableTile>().unitOnTile == null || hit.transform.gameObject.GetComponent<ClickableTile>().unitOnTile == selectedUnit) && (selectedUnitMoveRange.Contains(nodeToCeck)))
                    {
                        generatePathTo(clickedTileX, clickedTileY);
                        unitSelectedPreviousX = selectedUnit.GetComponent<UnitScript>().x;
                        unitSelectedPreviousY = selectedUnit.GetComponent<UnitScript>().y;
                        previousOccupiedTile = selectedUnit.GetComponent<UnitScript>().tileBeingOccupied;
                        moveUnit();
                        //finalizeOption();
                        StartCoroutine(moveUnitAndFinalize());
                        return true;
                    }
                    if (hit.transform.gameObject.GetComponent<ClickableTile>().unitOnTile != null && (hit.transform.gameObject.GetComponent<ClickableTile>().unitOnTile.GetComponent<UnitScript>().team != selectedUnit.GetComponent<UnitScript>().team) && (attackableTiles.Contains(nodeToCeck)))
                    {
                        if (hit.transform.gameObject.GetComponent<ClickableTile>().unitOnTile.GetComponent<UnitScript>().currentHealthPoints > 0)
                        {
                            Debug.Log("Támadás Tile");
                            StartCoroutine(BM.Attack(selectedUnit, hit.transform.gameObject.GetComponent<ClickableTile>().unitOnTile));
                            Debug.Log("HP: " + hit.transform.gameObject.GetComponent<ClickableTile>().unitOnTile.GetComponent<UnitScript>().currentHealthPoints);
                            StartCoroutine(attackUnitAndFinalize(selectedUnit));

                            return true;
                        }
                    }

                }
                else if (hit.transform.parent.gameObject.CompareTag("Unit"))
                {
                    GameObject unitClicked = hit.transform.parent.gameObject;
                    int unitX = unitClicked.GetComponent<UnitScript>().x;
                    int unitY = unitClicked.GetComponent<UnitScript>().y;
                    if (unitClicked.GetComponent<UnitScript>().team != selectedUnit.GetComponent<UnitScript>().team && attackableTiles.Contains(graph[unitX, unitY]))
                    {
                        if (unitClicked.GetComponent<UnitScript>().currentHealthPoints > 0)
                        {
                            Debug.Log("Támadás Unit");
                            StartCoroutine(BM.Attack(selectedUnit, unitClicked));
                            StartCoroutine(attackUnitAndFinalize(selectedUnit));
                            Debug.Log("HP: " + unitClicked.GetComponent<UnitScript>().currentHealthPoints);
                            return true;
                        }
                    }
                }

            }
        }
        else if (selectedUnit.GetComponent<UnitScript>().team == 1)
        {
            //doMove(findBestMove());
            Node bestMove = findBestMove();
            generatePathTo(bestMove.x, bestMove.y);
            unitSelectedPreviousX = selectedUnit.GetComponent<UnitScript>().x;
            unitSelectedPreviousY = selectedUnit.GetComponent<UnitScript>().y;
            previousOccupiedTile = selectedUnit.GetComponent<UnitScript>().tileBeingOccupied;
            Debug.Log("CurrentPath : " + currentPath);
            moveUnit();
            StartCoroutine(moveUnitAndFinalize());
            Debug.Log("x: " + bestMove.x + ", y: " + bestMove.y);
            Debug.Log("sikerült az AI lépés");
            return true;
        }
        return false;
    }

    public HashSet<Node> getUnitMovementOptions()
    {
        float[,] cost = new float[mapSizeX, mapSizeY];
        HashSet<Node> UIHighlight = new HashSet<Node>();
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();
        HashSet<Node> tempUIHighlight = new HashSet<Node>();
        int moveSpeed = selectedUnit.GetComponent<UnitScript>().moveSpeed;
        Node unitInitialNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];

        finalMovementHighlight.Add(unitInitialNode);
        foreach (Node n in unitInitialNode.neighbours)
        {
            cost[n.x, n.y] = costToEnterTile(n.x, n.y);
            if(moveSpeed - cost[n.x, n.y] >= 0)
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
                if (i < attRange -1)
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
        if (tilesOnMap[x,y].GetComponent<ClickableTile>().unitOnTile != null)
        {
            if (tilesOnMap[x,y].GetComponent<ClickableTile>().unitOnTile.GetComponent<UnitScript>().team != selectedUnit.GetComponent<UnitScript>().team)
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

        //int attRange = selectedUnit.GetComponent<UnitScript>().attackRange;

        //Node unitInitalNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];
        finalMovementHighlight = getUnitMovementOptions();
        //totalAttackableTiles = getUnitAttackOptions();

        highlightCurrentUnit();
        highlightMovementRange(finalMovementHighlight);
        //highlightEnemiesInRange(totalAttackableTiles);
        highlightUnitAttackOption();

        
        selectedUnitMoveRange = finalMovementHighlight;
        //selectedUnitAttackRange = totalAttackableTiles;
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
        foreach  (GameObject quad in quadOnMap)
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
        
        if (unitCanEnterTile(x,y) == false)
        {
            return Mathf.Infinity;
        }

        Tile t = tileTypes[tiles[x, y]];
        float distance = t.movementCost;
        return distance;
    }

    public void generatePathTo(int x, int y)
    {
        Debug.Log("asd");
        //Ugyan oda kattintott mint ahol a unit áll
        if (selectedUnit.GetComponent<UnitScript>().x == x && selectedUnit.GetComponent<UnitScript>().y == y)
        {
            currentPath = new List<Node>();
            selectedUnit.GetComponent<UnitScript>().path = currentPath;
            
            return;
        }

        //nem tudunk oda lépni, visszatérünk
        if(unitCanEnterTile(x,y) == false)
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
            if(n != source)
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
            if(prev[target] == null)
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

    public void doMove(Node move)
    {

        HashSet<Node> attackableTiles = getUnitAttackOptions();
        Node nodeToCeck = graph[move.x, move.y];
        tempUnit = selectedUnit;

        Debug.Log((tilesOnMap[move.x, move.y]));
        if (tilesOnMap[move.x, move.y].GetComponent<ClickableTile>().unitOnTile != null && tilesOnMap[move.x, move.y].GetComponent<ClickableTile>().unitOnTile.GetComponent<UnitScript>().team != selectedUnit.GetComponent<UnitScript>().team && attackableTiles.Contains(nodeToCeck))
        {
            Debug.LogWarning("Támadás");
            
            if ((tilesOnMap[move.x, move.y].GetComponent<ClickableTile>().unitOnTile.GetComponent<UnitScript>().currentHealthPoints > 0))
            {
                tempEnemy = tilesOnMap[move.x, move.y].GetComponent<ClickableTile>().unitOnTile;
                var enemy = tempEnemy.GetComponent<UnitScript>();
                
                
                //tempNode = tilesOnMap[tempEnemy.GetComponent<UnitScript>().x, tempEnemy.GetComponent<UnitScript>().y].GetComponent<ClickableTile>().unitOnTile;
                //tempTile = tempEnemy.GetComponent<UnitScript>().tileBeingOccupied;
                Debug.Log("Támadás Tile");
                Debug.Log("Támadó: " + selectedUnit);
                Debug.Log("Célpont: " + tilesOnMap[move.x, move.y].GetComponent<ClickableTile>().unitOnTile);
                Debug.Log(BM);
                BM.Attack(selectedUnit, (tilesOnMap[move.x, move.y].GetComponent<ClickableTile>().unitOnTile));
                Debug.Log("HP: " + (tilesOnMap[move.x, move.y].GetComponent<ClickableTile>().unitOnTile.GetComponent<UnitScript>().currentHealthPoints));
                attackUnitAndFinalize(selectedUnit);
                UnitState enemyUnitState = new UnitState();
                enemyUnitState.healthPoint = enemy.currentHealthPoints;
                enemyUnitState.occupiedTile = enemy.tileBeingOccupied;
                enemyUnitState.x = enemy.x;
                enemyUnitState.x = enemy.y;
                enemy.states.Push(enemyUnitState);


            }
        }
        else if (tilesOnMap[move.x, move.y].GetComponent<ClickableTile>().unitOnTile == null)
        {
            
            //tempNode = tilesOnMap[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y].GetComponent<ClickableTile>().unitOnTile;
            Debug.Log("tempnode1: " + tilesOnMap[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y].GetComponent<ClickableTile>().unitOnTile);
            //tempTile = selectedUnit.GetComponent<UnitScript>().tileBeingOccupied;
            Debug.LogWarning("1" + tempTile);
            tilesOnMap[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y].GetComponent<ClickableTile>().unitOnTile = null;
            Debug.LogWarning("selected? " + selectedUnit);
            Debug.LogWarning("move? " + move.x + " " + move.y);
            tilesOnMap[move.x, move.y].GetComponent<ClickableTile>().unitOnTile = selectedUnit;
            Debug.LogWarning("tileonmap: " + tilesOnMap[move.x, move.y].GetComponent<ClickableTile>().unitOnTile);
            selectedUnit.GetComponent<UnitScript>().tileBeingOccupied = tilesOnMap[move.x, move.y];

            var unit = selectedUnit.GetComponent<UnitScript>();
            UnitState selectedUnitState = new UnitState();
            selectedUnitState.healthPoint = unit.currentHealthPoints;
            selectedUnitState.occupiedTile = unit.tileBeingOccupied;
            selectedUnitState.x = unit.x;
            selectedUnitState.y = unit.y;
            unit.states.Push(selectedUnitState);
        }
        //Moved állapotba tesszük
        selectedUnit.GetComponent<UnitScript>().SetMovementState(2);
        deselectUnit();
        GM.EndTurn();
        
    }

    //TODO: redo-t megcsinalni
    public void redoMove(Node move)
    {
        deselectUnit();
        GM.PreviousTurn();
        Debug.LogWarning("2" + tempTile);
        if (tempEnemy != null)
        {
            var enemy = tempEnemy.GetComponent<UnitScript>();
            var lastState = enemy.states.Peek();
            enemy.currentHealthPoints = lastState.healthPoint;
            enemy.tileBeingOccupied = lastState.occupiedTile;
            enemy.x = lastState.x;
            enemy.y = lastState.y;
            //tempNode = tempEnemy;
            tilesOnMap[enemy.x, enemy.y].GetComponent<ClickableTile>().unitOnTile = tempEnemy;
            tempEnemy.GetComponent<UnitScript>().isDead = false;
            tempEnemy.GetComponent<UnitScript>().showUnit();
            enemy.states.Pop();
        }
        else
        {
            var unit = selectedUnit.GetComponent<UnitScript>();
            var lastState = unit.states.Peek();
            unit.currentHealthPoints = lastState.healthPoint;
            unit.tileBeingOccupied = lastState.occupiedTile;
            unit.x = lastState.y;
            unit.y = lastState.y;
            Debug.Log(selectedUnit + " x: " + unit.x + " y: " + unit.y);
            tilesOnMap[move.x, move.y].GetComponent<ClickableTile>().unitOnTile = null;
            tilesOnMap[unit.x, unit.y].GetComponent<ClickableTile>().unitOnTile = selectedUnit;
            unit.states.Pop();
            //selectedUnit.GetComponent<UnitScript>().tileBeingOccupied = tempTile;
        }
    }

    public Node findBestMove()
    {
        HashSet<Node> moves = getActualMovementOptions(selectedUnit);
        moves.UnionWith(getUnitAttackOptions());
        double bestScore = double.MinValue;
        Node bestMove = null;
        foreach (Node m in moves)
        {
            Debug.LogWarning(this.selectedUnit);
            double score = AI.minimax(this, true, 4, double.MinValue, double.MaxValue);
            
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = m;
                
                Debug.Log("Best: " + bestMove.x + ", " + bestMove.y);
            }
            Debug.Log("move: " + m.x + ", " + m.y);
        }
        Debug.Log("vége");
        return bestMove;
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
