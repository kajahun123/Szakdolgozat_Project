using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Game
{
    class VirtualMap 
    {
        public UnitScript[,] units;
        public int mapSizeX;
        public int mapSizeY;
        public Stack<Step> steps;
        public VirtualMap(TileMap map, int mapSizeX, int mapSizeY)
        {
            for (int x = 0; x < mapSizeX; x++)
            {
                for (int y = 0; y < mapSizeY; y++)
                {
                    GameObject tile = map.tilesOnMap[x, y];
                    ClickableTile clickableTile = tile.GetComponent<ClickableTile>();
                    units[x, y] = clickableTile.unitOnTile.GetComponent<UnitScript>();
                    units[x, y].states = new Stack<UnitState>();
                }
            }

            this.mapSizeX = mapSizeX;
            this.mapSizeY = mapSizeY;
        }

        public void doMove(Node node)
        {
            throw new NotImplementedException();
        }

        public HashSet<Node> getActualMovementOptions(GameObject unit)
        {
            throw new NotImplementedException();
        }

        public HashSet<Node> getUnitAttackOptions()
        {
            throw new NotImplementedException();
        }

        public void redoMove(Node node)
        {
            throw new NotImplementedException();
        }
    }
}
