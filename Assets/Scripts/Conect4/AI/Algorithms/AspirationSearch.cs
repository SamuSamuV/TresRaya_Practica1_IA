// Copyright © 2025 Samuel Campos Borrego, Laura Gallego Fernández, Icía Fernández Fornos. Todos los derechos reservados.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class AspirationSearch : SearchAlgorithmBase
{
    [Header("Search settings")]
    public int defaultMaxDepth = 6;
    public int nodeCountLimit = 200000;
    public int initialWindow = 50;
    public bool useIterativeDeepening = true;

    private int nodesSearched;

    public override int GetBestMove(Board board, Player aiPlayer, int maxDepth, int timeLimitMs = 0)
    {
        Stopwatch sw = Stopwatch.StartNew();

        if (maxDepth <= 0) maxDepth = defaultMaxDepth;
        int bestMove = -1;
        int bestScore = 0;

        nodesSearched = 0;

        int startDepth = useIterativeDeepening ? 1 : maxDepth;
        for (int depth = startDepth; depth <= maxDepth; depth++)
        {
            int guess = bestMove == -1 ? 0 : bestScore;
            int window = initialWindow;
            int alpha = guess - window;
            int beta = guess + window;

            int resultScore = int.MinValue;
            int resultMove = -1;

            bool done = false;
            while (!done)
            {
                nodesSearched = 0;
                resultScore = NegamaxRoot(board, aiPlayer, depth, alpha, beta, out resultMove);

                if (resultScore <= alpha)
                {
                    alpha = alpha * 2;
                    window *= 2;
                    if (Math.Abs(alpha) > 1000000) alpha = int.MinValue / 4;
                }

                else if (resultScore >= beta)
                {
                    beta = beta * 2;
                    window *= 2;
                    if (Math.Abs(beta) > 1000000) beta = int.MaxValue / 4;
                }

                else
                {
                    done = true;
                }

                if (nodesSearched > nodeCountLimit) { done = true; }
            }

            if (resultMove != -1)
            {
                bestMove = resultMove;
                bestScore = resultScore;
            }
        }

        sw.Stop();
        UnityEngine.Debug.Log($"AspirationSearch tardó {sw.ElapsedMilliseconds} ms (profundidad: " + maxDepth + ", nodos: " + nodesSearched + ")");

        return bestMove;
    }

    private int NegamaxRoot(Board board, Player aiPlayer, int depth, int alpha, int beta, out int bestMove)
    {
        bestMove = -1;
        int bestScore = int.MinValue;

        List<int> moves = GetLegalMoves(board);
        moves.Sort((a, b) => Math.Abs(b - board.Columns / 2).CompareTo(Math.Abs(a - board.Columns / 2)));

        foreach (int col in moves)
        {
            int row = MakeMove(board, col, aiPlayer);
            if (row == -1) continue;

            int score;
            if (board.CheckWin(col, row, aiPlayer))
            {
                score = 1000000 / (1);
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
            if (alpha >= beta) break;
        }

        return bestScore;
    }

    private int Negamax(Board board, Player player, int depth, int alpha, int beta)
    {
        nodesSearched++;
        if (nodesSearched > nodeCountLimit) return 0;

        List<int> moves = GetLegalMoves(board);
        if (moves.Count == 0) return 0;
        if (depth == 0)
        {
            return Evaluate(board, player);
        }

        int best = int.MinValue;
        moves.Sort((a, b) => Math.Abs(b - board.Columns / 2).CompareTo(Math.Abs(a - board.Columns / 2)));

        foreach (int col in moves)
        {
            int row = MakeMove(board, col, player);
            if (row == -1) continue;

            int score;
            if (board.CheckWin(col, row, player))
            {
                score = 1000000 / ((defaultMaxDepth - depth) + 1);
            }
            else
            {
                score = -Negamax(board, Opponent(player), depth - 1, -beta, -alpha);
            }

            UndoMove(board, col, row);

            best = Math.Max(best, score);
            alpha = Math.Max(alpha, score);
            if (alpha >= beta) break;
        }

        return best;
    }
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
        return (p == Player.MyMelody) ? Player.Kuromi : Player.MyMelody;
    }
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
                {
                    score += EvaluateWindow(board, c, r, 1, 0, perspective);
                }

                if (r + 3 < board.Rows)
                {
                    score += EvaluateWindow(board, c, r, 0, 1, perspective);
                }

                if (c + 3 < board.Columns && r + 3 < board.Rows)
                {
                    score += EvaluateWindow(board, c, r, 1, 1, perspective);
                }

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

        if (oppCount == 3 && emptyCount == 1) return -80;
        if (oppCount == 2 && emptyCount == 2) return -5;

        return 0;
    }
}
