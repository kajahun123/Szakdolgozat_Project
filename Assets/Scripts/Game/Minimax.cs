using Assets.Scripts.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Minimax : MonoBehaviour
{
    Position globalBestMove;
    double globalBestScore;
    int maxDepth;
    List<Step> lastSteps;

    public Position MinMax(TileMap map, bool maxPlayer, int depth, double alpha, double beta, int selectedAINumber, int selectedPlayerNumber)
    {
        VirtualMap virtualMap = new VirtualMap(map, map.mapSizeX, map.mapSizeY, selectedAINumber, selectedPlayerNumber);
        globalBestScore = int.MinValue;
        maxDepth = depth;
        MinMax_R(virtualMap, maxPlayer, depth, alpha, beta);
        return globalBestMove;
    }

    private double MinMax_R(VirtualMap map, bool maxPlayer, int depth, double alpha, double beta)
    {
        if (depth == 0 || map.IsGameOver())
        {
            if (map.IsGameOver())
            {
                
            }
            return evaluateScore(map);
        }
        List<Position> options = map.GetMoveOptions(map.CurrentUnit);
        int actualCurrentUnitId = map.CurrentUnit.id;
        if (maxPlayer)
        {
            double bestScore = double.MinValue;
            Position bestMove = options[0];
            foreach (Position option in options)
            {
                map.doMove(option, map.CurrentUnit);
                    double score = MinMax_R(map, false, depth - 1, alpha, beta);

                map.redoMove();

                //Quickfix
                if (actualCurrentUnitId != map.CurrentUnit.id)
                {
                    map.currentAIId = actualCurrentUnitId;
                }

                if (bestScore < score)
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
                double score = MinMax_R(map, true, depth - 1, alpha, beta);
                map.redoMove();

                //Quickfix
                if (actualCurrentUnitId != map.CurrentUnit.id)
                {
                    map.currentPlayerId = actualCurrentUnitId;
                }

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

    private double evaluateScore(VirtualMap map)
    {
        double totalScore = 0;

        foreach (Step step in map.steps)
        {
            totalScore = totalScore + step.score;
        }

        return totalScore;
    }
}
