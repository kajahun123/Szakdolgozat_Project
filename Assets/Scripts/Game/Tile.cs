using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    public string name;
    public GameObject tileVisualPrefab;
    public float movementCost = 1;
    public GameObject unitOnTile;

    public bool isWalkable = true;
}
