using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    public Tile[] tileTypes;
    public int[,] tiles;

    int mapSizeX = 10;
    int mapSizeY = 10;

    private void Start()
    {
        tiles = new int[mapSizeX,mapSizeY];

        //legener�ljuk az �sszes mez�t �s Grass �rt�ket adunk neki (0=grass)
        for(int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tiles[x, y] = 0;
            }
        }

        GenerateMapVisuals();
    }

    public void GenerateMapVisuals()
    {
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                GameObject newTile = Instantiate(tileTypes[tiles[x,y]].tileVisualPrefab, new Vector3(x, y, 0), Quaternion.identity);
                Debug.Log(tileTypes[tiles[x, y]].tileVisualPrefab.name);
            }
        }
    }
}
