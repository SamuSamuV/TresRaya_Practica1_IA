// Copyright © 2025 Samuel Campos Borrego, Laura Gallego Fernández, Icía Fernández Fornos. Todos los derechos reservados.

using System;
using System.Collections.Generic;
using UnityEngine;

public class NegaMaxAB : SearchAlgorithmBase
{
    [Header("Search settings")]
    public int defaultMaxDepth = 6;
    public int nodeCountLimit = 200000;
    private int nodesSearched;

    public override int GetBestMove(Board board, Player aiPlayer, int maxDepth, int timeLimitMs = 0)
    {
        if (maxDepth <= 0) maxDepth = defaultMaxDepth;
        nodesSearched = 0;

        int bestMove = -1;
        int bestScore = int.MinValue;

        List<int> moves = GetLegalMoves(board);
        // orden simple: centro primero (mejora poda)
        moves.Sort((a, b) => Math.Abs(b - board.Columns / 2).CompareTo(Math.Abs(a - board.Columns / 2)));

        foreach (int col in moves)
        {
            int row = MakeMove(board, col, aiPlayer);
            if (row == -1) continue;

            int score;
            if (board.CheckWin(col, row, aiPlayer))
                score = 1000000;
            else
                score = -Negamax(board, Opponent(aiPlayer), maxDepth - 1, int.MinValue / 4, int.MaxValue / 4);

            UndoMove(board, col, row);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = col;
            }
        }

        Debug.Log($"[NegaMaxAB] Move={bestMove}, Score={bestScore}, Nodes={nodesSearched}");
        return bestMove;
    }

    private int Negamax(Board board, Player player, int depth, int alpha, int beta)
    {
        nodesSearched++;
        if (nodesSearched > nodeCountLimit || depth == 0)
            return Evaluate(board, player);

        List<int> moves = GetLegalMoves(board);
        if (moves.Count == 0) return 0; // empate

        // centro primero
        moves.Sort((a, b) => Math.Abs(b - board.Columns / 2).CompareTo(Math.Abs(a - board.Columns / 2)));

        int best = int.MinValue;

        foreach (int col in moves)
        {
            int row = MakeMove(board, col, player);
            if (row == -1) continue;

            int score;
            if (board.CheckWin(col, row, player))
                score = 1000000 / ((defaultMaxDepth - depth) + 1);
            else
                score = -Negamax(board, Opponent(player), depth - 1, -beta, -alpha);

            UndoMove(board, col, row);

            best = Math.Max(best, score);
            alpha = Math.Max(alpha, score);

            //  Poda alfa-beta real:
            if (alpha >= beta)
                break; // cortar ramas inútiles
        }

        return best;
    }

    // --- utilidades comunes ---
    private List<int> GetLegalMoves(Board board)
    {
        List<int> moves = new List<int>();
        for (int c = 0; c < board.Columns; c++)
            if (board.GetLowestEmptyRow(c) != -1)
                moves.Add(c);
        return moves;
    }

    private int MakeMove(Board board, int column, Player p)
    {
        int row = board.GetLowestEmptyRow(column);
        if (row == -1) return -1;
        board.SetCell(column, row, p);
        return row;
    }

    private void UndoMove(Board board, int column, int row)
    {
        board.SetCell(column, row, Player.None);
    }

    private Player Opponent(Player p)
    {
        return (p == Player.Red) ? Player.Yellow : Player.Red;
    }

    // --- Evaluación igual que en los demás ---
    private int Evaluate(Board board, Player perspective)
    {
        int score = 0;
        int centerCol = board.Columns / 2;
        int centerCount = 0;
        for (int r = 0; r < board.Rows; r++)
            if (board.GetCell(centerCol, r) == perspective) centerCount++;
        score += centerCount * 3;

        for (int c = 0; c < board.Columns; c++)
        {
            for (int r = 0; r < board.Rows; r++)
            {
                if (c + 3 < board.Columns)
                    score += EvaluateWindow(board, c, r, 1, 0, perspective);
                if (r + 3 < board.Rows)
                    score += EvaluateWindow(board, c, r, 0, 1, perspective);
                if (c + 3 < board.Columns && r + 3 < board.Rows)
                    score += EvaluateWindow(board, c, r, 1, 1, perspective);
                if (c + 3 < board.Columns && r - 3 >= 0)
                    score += EvaluateWindow(board, c, r, 1, -1, perspective);
            }
        }
        return score;
    }

    private int EvaluateWindow(Board board, int startC, int startR, int dc, int dr, Player perspective)
    {
        int myCount = 0, oppCount = 0, emptyCount = 0;
        for (int i = 0; i < 4; i++)
        {
            Player p = board.GetCell(startC + dc * i, startR + dr * i);
            if (p == perspective) myCount++;
            else if (p == Player.None) emptyCount++;
            else oppCount++;
        }

        if (myCount == 4) return 10000;
        if (myCount == 3 && emptyCount == 1) return 100;
        if (myCount == 2 && emptyCount == 2) return 10;
        if (oppCount == 3 && emptyCount == 1) return -80;
        if (oppCount == 2 && emptyCount == 2) return -5;
        return 0;
    }
}
