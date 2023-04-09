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
        public UnitScript[,] table;
        public int mapSizeX;
        public int mapSizeY;
        public Stack<Step> steps;
        public UnitScript selectedUnit;
        public TileMap originalMap;
        public VirtualMap(TileMap map, int mapSizeX, int mapSizeY)
        {
            this.originalMap = map;
            for (int x = 0; x < mapSizeX; x++)
            {
                for (int y = 0; y < mapSizeY; y++)
                {
                    GameObject tile = map.tilesOnMap[x, y];
                    ClickableTile clickableTile = tile.GetComponent<ClickableTile>();
                    if (clickableTile.unitOnTile)
                    {
                        table[x, y] = clickableTile.unitOnTile.GetComponent<UnitScript>();
                        table[x, y].states = new Stack<UnitState>();
                        UnitState initialState = new UnitState(table[x, y]);
                        table[x, y].states.Push(initialState);
                    }

                }
            }

            this.mapSizeX = mapSizeX;
            this.mapSizeY = mapSizeY;
            steps = new Stack<Step>();

        }

        public void doMove(Position move, UnitScript unit)
        {
            if (table[move.x, move.y] == null)
            {
                UnitState lastState = unit.states.Peek();
                UnitState newState = new UnitState(lastState);
                newState.x = move.x;
                newState.y = move.y;
                unit.states.Push(newState);
                table[lastState.x, lastState.y] = null;
                table[move.x, move.y] = unit;
                //függvény: minél közelebb lép a legközelebbi játékoshoz, annál több pont, minél távolabb annál kevesebb
                int movementScore = GetScoreByDistance(unit, table);
                Step step = new Step(Step.Type.Movement, unit,movementScore);
                steps.Push(step);
            }
            else if (table[move.x, move.y] != null && table[move.x, move.y].team != unit.team)
            {
                UnitScript target = table[move.x, move.y];
                VirtualAttack(unit, target);
                int attackScore = GetScoreByDamage(unit);
                Step step = new Step(Step.Type.Attack, target,attackScore, unit);
                steps.Push(step);
            }
        }
        public int GetScoreByDamage(UnitScript unit)
        {
            int attackScore = unit.attackDamage;
            if (unit.team == 0)
            {
                return attackScore *= -1;
            }

            return attackScore;
        }
        public int GetScoreByDistance(UnitScript unit, UnitScript[,] table)
        {
            UnitScript closestUnit;
            int closestDistance = int.MaxValue;
            for (int x = 0; x < table.Length; x++)
            {
                for (int y = 0; y < table.Length; y++)
                {
                    //state vagy tényleges x,y?
                    if(table[x,y] != null)
                    {
                        if (GetDistance(unit.x, unit.y, x, y) < closestDistance)
                        {
                            closestUnit = table[x, y];
                            closestDistance = GetDistance(unit.x, unit.y, x, y);
                        }
                    }
                    
                }
            }
            
            //az AI-nak minél messzebb van annál rosszabb
            if(unit.team == 1)
            {
                return closestDistance * -1;
            }

            //a játékosnak minél messzebb van annál jobb
            return closestDistance;   
        }


        public List<Position> GetMoveOptions(UnitScript unit)
        {
            List<Position> legalMoves = new();
            for (int x = 0; x < mapSizeX; x++)
            {
                for (int y = 0; y < mapSizeY; y++)
                {
                    if (IsValidMove(unit, x, y))
                    {
                        Position move = new Position(x,y);
                        legalMoves.Add(move);
                    }
                }
            }
            return legalMoves;
        }

        public bool IsValidMove(UnitScript unit, int targetX, int targetY) 
        {
            int distance = GetDistance(unit.x, unit.y, targetX, targetY);
            if (distance <= unit.movementRange && IsFieldEmpty(targetX,targetY))
            {
                return true;
            }
            else if (distance <= unit.attackRange && IsEnemyOnField(unit, targetX, targetY))
            {
                return true;
            }
            return false;
        }

        public int GetDistance(int startX, int startY, int endX, int endY)
        {
            return Math.Abs(endX - startX) + Math.Abs(endY - startY); 
        }

        public void redoMove()
        {
            Step lastStep = steps.Pop();
            UnitState lastState = lastStep.changingUnit.states.Pop();
            UnitState currentState = lastStep.changingUnit.states.Peek();
            if (lastStep.type == Step.Type.Movement)
            {
                table[currentState.x, currentState.y] = lastStep.changingUnit;
                table[lastState.x, lastState.y] = null;
            }
            else if (lastStep.type == Step.Type.Attack && lastState.healthPoint <= 0)
            {
                table[currentState.x, currentState.y] = lastStep.changingUnit;
            }
        }

        public void VirtualAttack(UnitScript attackerUnit, UnitScript targetUnit)
        {
            int attackerDmg = attackerUnit.attackDamage;

            UnitState lastEnemyState = targetUnit.states.Peek();
            UnitState newEnemyState = new UnitState(lastEnemyState);
            newEnemyState.healthPoint = Mathf.Max(0, lastEnemyState.healthPoint - attackerDmg);
            targetUnit.states.Push(newEnemyState);
            if (newEnemyState.healthPoint <= 0)
            {
                table[newEnemyState.x, newEnemyState.y] = null;
            }
        }

        public bool IsFieldEmpty(int x, int y)
        {
            if (table[x, y] != null)
            {
                return false;
            }
            return originalMap.tilesOnMap[x, y].GetComponent<ClickableTile>().isWalkable;
        }

        public bool IsEnemyOnField(UnitScript unit, int x, int y)
        {
            return table[x, y] != null && table[x, y].team != unit.team;
        }

    }
}
