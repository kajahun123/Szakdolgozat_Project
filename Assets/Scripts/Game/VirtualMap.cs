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
        public UnitScript CurrentUnit
        {
            get
            {
                return currentTeam == Team.Player ? idsToPlayerUnits[GetCurrentOrNextAlive(Team.Player)] : idsToAIUnits[GetCurrentOrNextAlive(Team.AI)];
            }
        }

        
        public TileMap originalMap;
        public int currentAIId 
        {
            get
            {
                return _currentAIId;
            }
            set
            {
                //GameManager.Log("\t\t\t\tsetCurrentId: " + _currentAIId + " -> " + value);
                _currentAIId = value;
            }
        }
        private int _currentAIId;
        public int currentPlayerId;
        public Dictionary<int, UnitScript> idsToPlayerUnits = new Dictionary<int, UnitScript>();
        public Dictionary<int, UnitScript> idsToAIUnits = new Dictionary<int, UnitScript>();
        public int maxPlayerId;
        public int maxAIId;
        public Team currentTeam;

        public VirtualMap(TileMap map, int mapSizeX, int mapSizeY, int selectedAINumber, int selectedPlayerNumber)
        {
            this.originalMap = map;
            table = new UnitScript[mapSizeX, mapSizeY];
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
                        if (table[x, y].team == Team.Player)
                        {
                            if (table[x, y].id > maxPlayerId)
                            {
                                maxPlayerId = table[x, y].id;
                            }

                            idsToPlayerUnits.Add(table[x, y].id, table[x, y]);
                        }
                        else if (table[x, y].team == Team.AI)
                        {
                            if (table[x, y].id > maxAIId)
                            {
                                maxAIId = table[x, y].id;
                            }
                            idsToAIUnits.Add(table[x, y].id, table[x, y]);
                        }
                    }

                }
            }

            this.mapSizeX = mapSizeX;
            this.mapSizeY = mapSizeY;
            steps = new Stack<Step>();
            this.currentAIId = selectedAINumber;
            this.currentPlayerId = selectedPlayerNumber;
            this.currentTeam = Team.AI;
        }

        public int GetCurrentOrNextAlive(Team team)
        {
            if (team == Team.AI)
            {
                if (HasTeamLivingUnits(team))
                {
                    int i = currentAIId;
                    while (!idsToAIUnits.ContainsKey(i) || idsToAIUnits[i].VIsDead)
                    {
                        if (i >= maxAIId)
                        {
                            i = 0;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    return i;
                }
            }
            else if (team == Team.Player)
            {
                if (HasTeamLivingUnits(team))
                {
                    int i = currentPlayerId;
                    while (!idsToPlayerUnits.ContainsKey(i) || idsToPlayerUnits[i].VIsDead)
                    {
                        if (i >= maxPlayerId)
                        {
                            i = 0;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    currentPlayerId = i;
                    return i;
                }
            }

            throw new Exception("GetCurrentOrNextAlive: Nincs current unit!");
        }

        public void doMove(Position move, UnitScript unit)
        {
            if (IsFieldEmpty(move.x, move.y) || unit.VX == move.x && unit.VY == move.y)
            {
                UnitState lastState = unit.states.Peek();
                UnitState newState = new UnitState(lastState);
                newState.x = move.x;
                newState.y = move.y;
                unit.states.Push(newState);
                table[lastState.x, lastState.y] = null;
                table[move.x, move.y] = unit;
                //függvény: minél közelebb lép a legközelebbi játékoshoz, annál több pont, minél távolabb annál kevesebb
                double movementScore = GetScoreByDistance(unit, table);
                Step step = new Step(Step.Type.Movement, unit, movementScore);
                steps.Push(step);
            }
            else if (IsEnemyOnField(unit, move.x, move.y))
            {
                UnitScript target = table[move.x, move.y];
                VirtualAttack(unit, target);
                //int attackScore = 0;
                int attackScore = GetScoreByDamage(unit, target);
                Step step = new Step(Step.Type.Attack, target, attackScore, unit);
                steps.Push(step);
            }
            else
            {
                throw new Exception("Cannot make a step!");
            }

            //if (IsGameOver())
            //{
            NextTurn();
            //}
        }

        private void NextTurn()
        {
            if (currentTeam == Team.AI)
            {
                currentTeam = Team.Player;
                if (HasTeamLivingUnits(Team.AI))
                {
                    int i = currentAIId + 1;
                    while (!idsToAIUnits.ContainsKey(i) || idsToAIUnits[i].VIsDead)
                    {
                        if (i >= maxAIId)
                        {
                            i = 0;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    currentAIId = i;
                }
            }
            else if (currentTeam == Team.Player)
            {
                currentTeam = Team.AI;
                if (HasTeamLivingUnits(Team.Player))
                {
                    int i = currentPlayerId + 1;
                    while (!idsToPlayerUnits.ContainsKey(i) || idsToPlayerUnits[i].VIsDead)
                    {
                        if (i >= maxPlayerId)
                        {
                            i = 0;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    currentPlayerId = i;
                }
            }
        }

        private void PreviousTurn()
        {
            if (currentTeam == Team.AI)
            {
                currentTeam = Team.Player;
                if (HasTeamLivingUnits(currentTeam))
                {
                    int i = currentPlayerId - 1;
                    while (!idsToPlayerUnits.ContainsKey(i) || idsToPlayerUnits[i].VIsDead)
                    {
                        if (i <= 0)
                        {
                            i = maxPlayerId;
                        }
                        else
                        {
                            i--;
                        }
                    }
                    currentPlayerId = i;
                }
            }
            else if (currentTeam == Team.Player)
            {
                currentTeam = Team.AI;
                if (HasTeamLivingUnits(currentTeam))
                {
                    int i = currentAIId - 1;
                    while (!idsToAIUnits.ContainsKey(i) || idsToAIUnits[i].VIsDead)
                    {
                        if (i <= 0)
                        {
                            i = maxAIId;
                        }
                        else
                        {
                            i--;
                        }
                    }
                    currentAIId = i;
                }
            }
        }

        public bool HasTeamLivingUnits(Team team)
        {
            if (team == Team.Player)
            {
                foreach (var idToPlayer in idsToPlayerUnits)
                {
                    if (!idToPlayer.Value.VIsDead)
                    {
                        return true;
                    }
                }
            }
            else
            {
                foreach (var idToAi in idsToAIUnits)
                {
                    if (!idToAi.Value.VIsDead)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public int GetScoreByDamage(UnitScript unit, UnitScript target)
        {
            if (unit.team == Team.Player)
            {
                if (target.VIsDead)
                {
                    return -unit.attackDamage - 10;
                }
                return -unit.attackDamage;
            }
            else
            {
                if (target.VIsDead)
                {
                    return unit.attackDamage + 10;
                }
                return unit.attackDamage;
            }
        }

        public double GetScoreByDistance(UnitScript unit, UnitScript[,] table)
        {
            int closestDistance = int.MaxValue;
            for (int x = 0; x < table.GetLength(0); x++)
            {
                for (int y = 0; y < table.GetLength(1); y++)
                {
                    if (table[x, y] != null && table[x, y].team != unit.team)
                    {
                        if (GetDistance(unit.VX, unit.VY, x, y) < closestDistance)
                        {
                            closestDistance = GetDistance(unit.VX, unit.VY, x, y);
                        }
                    }
                }
            }
            return (double)-closestDistance;
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
                        Position move = new Position(x, y);
                        legalMoves.Add(move);
                    }
                }
            }
            return legalMoves;
        }

        public bool IsValidMove(UnitScript unit, int targetX, int targetY)
        {
            int distance = GetDistance(unit.VX, unit.VY, targetX, targetY);
            if (distance <= unit.movementRange && (IsFieldEmpty(targetX, targetY)) || distance == 0)
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
                table[lastState.x, lastState.y] = null;
                table[currentState.x, currentState.y] = lastStep.changingUnit;
            }
            else if (lastStep.type == Step.Type.Attack && lastState.healthPoint <= 0)
            {
                //GameManager.Log("Reborn: " + lastStep.changingUnit + ", position: " + currentState.x + ", " + currentState.y);
                table[currentState.x, currentState.y] = lastStep.changingUnit;
            }
            PreviousTurn();
        }

        public void VirtualAttack(UnitScript attackerUnit, UnitScript targetUnit)
        {
            int attackerDmg = attackerUnit.attackDamage;
            //GameManager.Log("Attacker: " + attackerUnit + ", target: " + targetUnit);
            UnitState lastEnemyState = targetUnit.states.Peek();
            UnitState newEnemyState = new UnitState(lastEnemyState);
            newEnemyState.healthPoint = Mathf.Max(0, lastEnemyState.healthPoint - attackerDmg);
            targetUnit.states.Push(newEnemyState);
            if (newEnemyState.healthPoint <= 0)
            {
                //GameManager.Log("Death: " + targetUnit + ", position: " + lastEnemyState.x + ", " + lastEnemyState.y);
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

        public bool IsGameOver()
        {
            return !HasTeamLivingUnits(Team.Player) || !HasTeamLivingUnits(Team.AI);
        }

    }
}
