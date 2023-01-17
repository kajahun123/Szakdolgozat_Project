using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimax : MonoBehaviour
{
    public GameManager GM;
    public double minimax(TileMap map, bool maxPlayer, int depth, double alpha, double beta)
    {
        if (depth == 0 || GM.checkWin() != 0)
        {
            return GM.checkWin() * 10;
        }
        HashSet<Node> moves = map.getActualMovementOptions();
        HashSet<Node> attacks = map.getUnitAttackOptions();

        if (maxPlayer)
        {
            double bestScore = double.MinValue;
            foreach (Node m in moves)
            {
                map.doMove(m);
                double score = minimax(map, false, depth - 1, alpha, beta);
                map.redoMove(m);
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
            foreach (Node m in moves)
            {
                map.doMove(m);
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
