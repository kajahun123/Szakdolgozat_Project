using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimax : MonoBehaviour
{
    public GameManager GM;
    public double minimax(TileMap map, bool maxPlayer, int depth, double alpha, double beta)
    {
        
        //state.Clone(map);
        if (depth == 0 || GM.checkWin() != 0)
        {
            return GM.evaluateScore(map) ;
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
                //Cloneozás
                double score = minimax(map, false, depth - 1, alpha, beta);
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
                double score = minimax(map, true, depth - 1, alpha, beta);
                
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
}
