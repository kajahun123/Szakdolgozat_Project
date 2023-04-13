using Assets.Scripts.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimax : MonoBehaviour
{
    Position globalBestMove;
    double globalBestScore;
    int maxDepth;

    public Position minimax(TileMap map, bool maxPlayer, int depth, double alpha, double beta, int selectedAINumber, int selectedPlayerNumber) {
        VirtualMap virtualMap = new VirtualMap(map, map.mapSizeX, map.mapSizeY, selectedAINumber, selectedPlayerNumber);
        globalBestScore = int.MinValue;
        maxDepth = depth;
        minimax_r(virtualMap, maxPlayer, depth, alpha, beta);
        return globalBestMove;
    }

    private double minimax_r(VirtualMap map, bool maxPlayer, int depth, double alpha, double beta)
    {
        if (depth == 0 || map.IsGameOver())
        {
            return evaluateScore(map.steps);
        }
        List<Position> options = map.GetMoveOptions(map.CurrentUnit);

        if (maxPlayer)
        {
            double bestScore = double.MinValue;
            Position bestMove = options[0];
            foreach (Position option in options)
            {
                map.doMove(option, map.CurrentUnit);
                GameManager.LogState(depth, map.idsToAIUnits[0]);
                double score = minimax_r(map, false, depth - 1, alpha, beta);
                map.redoMove();
                bestScore = Math.Max(score, bestScore);
                if(bestScore < score)
                {
                    bestScore = score;
                    bestMove = option;
                }
                alpha = Math.Max(alpha, bestScore);
                if (beta <= alpha)
                {
                    break;
                }
            }
            if (depth == maxDepth && globalBestScore < bestScore)
            {
                globalBestScore = bestScore;
                globalBestMove = bestMove;
            }
            return bestScore;
        }
        else
        {
            double bestScore = double.MaxValue;
            foreach (Position option in options)
            {
                map.doMove(option, map.CurrentUnit);
                double score = minimax_r(map, true, depth - 1, alpha, beta);
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

    private double evaluateScore(Stack<Step> steps)
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
