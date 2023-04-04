using Assets.Scripts.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimax : MonoBehaviour
{
    public GameManager GM;
   

    public double minimax(TileMap map, bool maxPlayer, int depth, double alpha, double beta) {
        VirtualMap virtualMap = new VirtualMap(map, map.mapSizeX, map.mapSizeY);
        return minimax_r(virtualMap, maxPlayer, depth, alpha, beta);
    }
    private double minimax_r(VirtualMap map, bool maxPlayer, int depth, double alpha, double beta)
    {
        
        //state.Clone(map);
        if (depth == 0 || GM.checkWin() != 0)
        {
            return evaluateScore();
        }
        Debug.Log("Min: " + map.selectedUnit.GetComponent<UnitScript>().UnitName);
        HashSet<Node> moves = map.getActualMovementOptions(map.selectedUnit);
        HashSet<Node> attacks = map.getUnitAttackOptions();
        moves.UnionWith(map.getUnitAttackOptions());

        if (maxPlayer)
        {
            double bestScore = double.MinValue;
            foreach (Node m in moves)
            {
                Debug.Log("AI");
                //State state = new State(map);
                Debug.Log("State1 selected: " + map.selectedUnit.GetComponent<UnitScript>().UnitName);
                map.doMove(m);
                Debug.Log("State2 selected: " + map.selectedUnit.GetComponent<UnitScript>().UnitName);
                
                double score = minimax_r(map, false, depth - 1, alpha, beta);
                Debug.Log("score :" + score);
                Debug.LogWarning("bejut ide???????");
                map.redoMove(m);
                bestScore = Math.Max(score, bestScore);
                alpha = Math.Max(alpha, bestScore);
                Debug.Log("bestscore: " + bestScore);
                if (beta <= alpha)
                {
                    break;
                }
            }
            return bestScore;
        }
        else
        {
            double bestScore = double.MaxValue;
            foreach (Node m in moves)
            {
                Debug.Log("Player");
                //State state = new State(map);
                Debug.Log("State1 selected: " + map.selectedUnit.GetComponent<UnitScript>().UnitName);
                map.doMove(m);
                Debug.Log("State2 selected: " + map.selectedUnit.GetComponent<UnitScript>().UnitName);
                double score = minimax_r(map, true, depth - 1, alpha, beta);
                
                map.redoMove(m);
                bestScore = Math.Min(score, bestScore);
                alpha = Math.Min(alpha, bestScore);
                if (beta <= alpha)
                {
                    break;
                }
            }

            return bestScore;
        }
    }

    public double evaluateScore()
    {
        double totalScore = 0;
        double score = 0;
        foreach (Transform u in team1.transform)
        {
            if (!u.GetComponent<UnitScript>().isDead)
            {
                score -= 10;
            }
        }

        totalScore += score;
        //AI
        foreach (Transform u in team2.transform)
        {
            if (!u.GetComponent<UnitScript>().isDead)
            {
                score += 10;
            }
        }
        totalScore = totalScore + score;
        return totalScore;
    }
}
