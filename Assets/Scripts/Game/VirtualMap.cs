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
        public UnitScript selectedUnit;
        public VirtualMap(TileMap map, int mapSizeX, int mapSizeY)
        {
            for (int x = 0; x < mapSizeX; x++)
            {
                for (int y = 0; y < mapSizeY; y++)
                {
                    GameObject tile = map.tilesOnMap[x, y];
                    ClickableTile clickableTile = tile.GetComponent<ClickableTile>();
                    if (clickableTile.unitOnTile)
                    {
                        units[x, y] = clickableTile.unitOnTile.GetComponent<UnitScript>();
                        units[x, y].states = new Stack<UnitState>();
                    }
                    
                }
            }

            this.mapSizeX = mapSizeX;
            this.mapSizeY = mapSizeY;
            steps = new Stack<Step>();

        }

        public void doMove(Node node)
        {

            //Mozgás
            
            UnitState lastState = new UnitState(); //states.Peek()
            UnitState newState = new UnitState();
            newState.healthPoint = lastState.healthPoint;
            newState.x = node.x;
            newState.y = node.y;
            selectedUnit.states.Push(newState);

            Step step = new Step(selectedUnit, Step.Type.Move);
            steps.Push(step);
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
            Step lastStep = steps.Peek();
            selectedUnit = lastStep.unit;

            UnitState lastState = selectedUnit.states.Peek();
            if (lastStep.type == Step.Type.Move)
            {
               
            }
        }
    }
}
