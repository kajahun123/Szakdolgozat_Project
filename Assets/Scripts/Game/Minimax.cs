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
            return evaluateScore(map.steps);
        }
       // GameManager.Log("getMoveOptions: " + map.CurrentUnit + ", depth: " + depth);
        List<Position> options = map.GetMoveOptions(map.CurrentUnit);
        if (maxPlayer)
        {
            double bestScore = double.MinValue;
            Position bestMove = options[0];
            foreach (Position option in options)
            {
                //GameManager.Log("\t\tMove: " + map.CurrentUnit + ", moveTo: " + option.x + ", " + option.y );
                map.doMove(option, map.CurrentUnit);
                double score = MinMax_R(map, false, depth - 1, alpha, beta);
                //if (depth == maxDepth)
                //{
                //    GameManager.LogState(depth, map.idsToAIUnits[0], score);
                //}
                map.redoMove();
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

        //GameManager.Log(totalScore.ToString());

        //if (GameManager._isDebugModeOn)
        //{
        //    lastSteps = new List<Step>(steps);
        //}

        return totalScore;
    }
}
