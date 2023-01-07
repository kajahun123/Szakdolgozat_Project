using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    public string name;
    public GameObject tileVisualPrefab;

    public bool isWalkable = true;
}
