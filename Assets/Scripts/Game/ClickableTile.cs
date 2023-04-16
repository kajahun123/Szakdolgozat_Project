using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTile : MonoBehaviour
{
    public int tileX;
    public int tileY;

    public GameObject unitOnTile;
    public TileMap map;

    public bool isWalkable;
}
