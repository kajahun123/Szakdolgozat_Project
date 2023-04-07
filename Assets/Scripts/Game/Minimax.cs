using Assets.Scripts.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimax : MonoBehaviour
{
    public double minimax(TileMap map, bool maxPlayer, int depth, double alpha, double beta, UnitScript startUnit) {
        VirtualMap virtualMap = new VirtualMap(map, map.mapSizeX, map.mapSizeY);
        return minimax_r(virtualMap, maxPlayer, depth, alpha, beta, startUnit);
    }
    private double minimax_r(VirtualMap map, bool maxPlayer, int depth, double alpha, double beta, UnitScript unit)
    {
        if (depth == 0 || checkWin() != 0)
        {
            return evaluateScore(map.steps);
        }
        List<Position> options = map.GetMoveOptions(unit);

        if (maxPlayer)
        {
            double bestScore = double.MinValue;
            foreach (Position option in options)
            {
                GameManager.Log("AI");
                map.doMove(option, unit);
                double score = minimax_r(map, false, depth - 1, alpha, beta,);
                map.redoMove();
                bestScore = Math.Max(score, bestScore);
                alpha = Math.Max(alpha, bestScore);
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
            foreach (Position option in options)
            {
                Debug.Log("Player");
                map.doMove(option, unit);
                double score = minimax_r(map, true, depth - 1, alpha, beta,);
                map.redoMove();
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

    public double evaluateScore(Stack<Step> steps)
    {
        double totalScore = 0;

        //damage +
        //kapott damage -
        foreach (Step step in steps)
        {
            totalScore = totalScore + step.score;
        }
        
        return totalScore;
    }
}
