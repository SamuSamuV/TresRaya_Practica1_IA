// Copyright © 2025 Samuel Campos Borrego, Laura Gallego Fernández, Icía Fernández Fornos. Todos los derechos reservados.

using System;
using System.Collections.Generic;
using UnityEngine;

public class AspirationSearch : SearchAlgorithmBase
{
    [Header("Search settings")]
    public int defaultMaxDepth = 6;
    public int nodeCountLimit = 200000; // safety
    public int initialWindow = 50; // aspiration window (score units)
    public bool useIterativeDeepening = true;

    private int nodesSearched;

    public override int GetBestMove(Board board, Player aiPlayer, int maxDepth, int timeLimitMs = 0)
    {
        if (maxDepth <= 0) maxDepth = defaultMaxDepth;
        int bestMove = -1;
        int bestScore = 0;

        nodesSearched = 0;

        // Iterative deepening
        int startDepth = useIterativeDeepening ? 1 : maxDepth;
        for (int depth = startDepth; depth <= maxDepth; depth++)
        {
            int guess = bestMove == -1 ? 0 : bestScore;
            int window = initialWindow;
            int alpha = guess - window;
            int beta = guess + window;

            int resultScore = int.MinValue;
            int resultMove = -1;

            // Try with aspiration window, if fails enlarge window and re-search
            bool done = false;
            while (!done)
            {
                nodesSearched = 0;
                resultScore = NegamaxRoot(board, aiPlayer, depth, alpha, beta, out resultMove);

                // fail-low
                if (resultScore <= alpha)
                {
                    // expand window downwards
                    alpha = alpha * 2;
                    window *= 2;
                    if (Math.Abs(alpha) > 1000000) alpha = int.MinValue / 4;
                    // continue loop -> re-search
                }
                // fail-high
                else if (resultScore >= beta)
                {
                    // expand window upwards
                    beta = beta * 2;
                    window *= 2;
                    if (Math.Abs(beta) > 1000000) beta = int.MaxValue / 4;
                    // continue loop -> re-search
                }
                else
                {
                    done = true;
                }

                // safety guard
                if (nodesSearched > nodeCountLimit) { done = true; }
            }

            // if we got a valid move, store it (deeper results override previous)
            if (resultMove != -1)
            {
                bestMove = resultMove;
                bestScore = resultScore;
            }
        }

        return bestMove;
    }

    private int NegamaxRoot(Board board, Player aiPlayer, int depth, int alpha, int beta, out int bestMove)
    {
        bestMove = -1;
        int bestScore = int.MinValue;

        List<int> moves = GetLegalMoves(board);
        // simple ordering: center first (helps alpha-beta)
        moves.Sort((a, b) => Math.Abs(b - board.Columns / 2).CompareTo(Math.Abs(a - board.Columns / 2)));

        foreach (int col in moves)
        {
            int row = MakeMove(board, col, aiPlayer);
            if (row == -1) continue;

            int score;
            if (board.CheckWin(col, row, aiPlayer))
            {
                score = 1000000 / (1); // immediate win
            }
            else
            {
                score = -Negamax(board, Opponent(aiPlayer), depth - 1, -beta, -alpha);
            }

            UndoMove(board, col, row);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = col;
            }
            alpha = Math.Max(alpha, score);
            if (alpha >= beta) break; // beta cut-off
        }

        return bestScore;
    }

    private int Negamax(Board board, Player player, int depth, int alpha, int beta)
    {
        nodesSearched++;
        if (nodesSearched > nodeCountLimit) return 0; // safety

        List<int> moves = GetLegalMoves(board);
        if (moves.Count == 0) return 0; // draw
        if (depth == 0)
        {
            return Evaluate(board, player);
        }

        int best = int.MinValue;
        // move ordering: center-first
        moves.Sort((a, b) => Math.Abs(b - board.Columns / 2).CompareTo(Math.Abs(a - board.Columns / 2)));

        foreach (int col in moves)
        {
            int row = MakeMove(board, col, player);
            if (row == -1) continue;

            int score;
            if (board.CheckWin(col, row, player))
            {
                score = 1000000 / ((defaultMaxDepth - depth) + 1); // prefer quicker wins
            }
            else
            {
                score = -Negamax(board, Opponent(player), depth - 1, -beta, -alpha);
            }

            UndoMove(board, col, row);

            best = Math.Max(best, score);
            alpha = Math.Max(alpha, score);
            if (alpha >= beta) break; // cutoff
        }

        return best;
    }

    // Helpers
    private List<int> GetLegalMoves(Board board)
    {
        List<int> moves = new List<int>();
        for (int c = 0; c < board.Columns; c++)
        {
            if (board.GetLowestEmptyRow(c) != -1) moves.Add(c);
        }
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

    // Evaluation function: heuristic windows + center control
    private int Evaluate(Board board, Player perspective)
    {
        // simple but effective heuristic:
        // + score for 2/3 aligned with open ends, + center weighting
        int score = 0;

        // center column control
        int centerCol = board.Columns / 2;
        int centerCount = 0;
        for (int r = 0; r < board.Rows; r++)
            if (board.GetCell(centerCol, r) == perspective) centerCount++;
        score += centerCount * 3;

        // scan all windows of 4
        for (int c = 0; c < board.Columns; c++)
        {
            for (int r = 0; r < board.Rows; r++)
            {
                // horizontal
                if (c + 3 < board.Columns)
                {
                    score += EvaluateWindow(board, c, r, 1, 0, perspective);
                }
                // vertical
                if (r + 3 < board.Rows)
                {
                    score += EvaluateWindow(board, c, r, 0, 1, perspective);
                }
                // diag up-right
                if (c + 3 < board.Columns && r + 3 < board.Rows)
                {
                    score += EvaluateWindow(board, c, r, 1, 1, perspective);
                }
                // diag down-right
                if (c + 3 < board.Columns && r - 3 >= 0)
                {
                    score += EvaluateWindow(board, c, r, 1, -1, perspective);
                }
            }
        }

        return score;
    }

    private int EvaluateWindow(Board board, int startC, int startR, int dc, int dr, Player perspective)
    {
        int myCount = 0;
        int oppCount = 0;
        int emptyCount = 0;
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

        if (oppCount == 3 && emptyCount == 1) return -80; // block urgent
        if (oppCount == 2 && emptyCount == 2) return -5;

        return 0;
    }
}
