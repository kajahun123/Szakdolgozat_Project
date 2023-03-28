using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State : TileMap
{
    //TileMap map;
    List<GameObject> units;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    public State(TileMap map)
    {
        this.unitsOnBoard = map.unitsOnBoard;
        this.units = map.getAllUnits();
        this.selectedUnit = map.selectedUnit;
        this.graph = map.graph;
        this.tilesOnMap = map.tilesOnMap;
        this.quadOnMapCurrentUnit = map.quadOnMapCurrentUnit;
        this.quadOnMap = map.quadOnMap;
        this.GM = map.GM;
        this.BM = map.BM;
        this.currentPath = map.currentPath;
    }
    
    

}
