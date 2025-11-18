// Copyright © 2025 Samuel Campos Borrego, Laura Gallego Fernández, Icía Fernández Fornos. Todos los derechos reservados.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class NegaScout : SearchAlgorithmBase
{
    [Header("Search settings")]
    public int defaultMaxDepth = 6;
    public int nodeLimit = 200000;
    public bool useIterativeDeepening = true;

    private int nodes;

    public override int GetBestMove(Board board, Player aiPlayer, int maxDepth, int timeLimitMs = 0)
    {
        Stopwatch sw = Stopwatch.StartNew();

        if (maxDepth <= 0) maxDepth = defaultMaxDepth;
        nodes = 0;

        int bestMove = -1;
        int bestScore = int.MinValue;

        int startDepth = useIterativeDeepening ? 1 : maxDepth;
        for (int depth = startDepth; depth <= maxDepth; depth++)
        {
            int move;
            int score = NegaScoutRoot(board, aiPlayer, depth, int.MinValue / 2, int.MaxValue / 2, out move);

            if (move != -1)
            {
                bestMove = move;
                bestScore = score;
            }
        }

        sw.Stop();
        UnityEngine.Debug.Log($"NegaScout tardó {sw.ElapsedMilliseconds} ms (profundidad: " + maxDepth + ", nodos: " + nodes + ")");

        return bestMove;
    }

    private int NegaScoutRoot(Board board, Player player, int depth, int alpha, int beta, out int bestMove)
    {
        bestMove = -1;
        int bestScore = int.MinValue;

        List<int> moves = GetLegalMoves(board);
        OrderMovesCenterFirst(moves, board);

        bool first = true;

        foreach (int col in moves)
        {
            int row = MakeMove(board, col, player);
            if (row == -1) continue;

            int score;
            if (board.CheckWin(col, row, player))
            {
                score = 1000000 / (1);
            }
            else
            {
                if (first)
                {
                    score = -NegaScoutSearch(board, Opponent(player), depth - 1, -beta, -alpha);
                    first = false;
                }
                else
                {
                    score = -NegaScoutSearch(board, Opponent(player), depth - 1, -alpha - 1, -alpha);
                    if (alpha < score && score < beta)
                    {
                        score = -NegaScoutSearch(board, Opponent(player), depth - 1, -beta, -score);
                    }
                }
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

    private int NegaScoutSearch(Board board, Player player, int depth, int alpha, int beta)
    {
        nodes++;
        if (nodes > nodeLimit) return 0;

        List<int> moves = GetLegalMoves(board);
        if (moves.Count == 0) return 0;
        if (depth == 0)
        {
            return Evaluate(board, player);
        }

        OrderMovesCenterFirst(moves, board);

        bool first = true;
        int b = beta;
        int best = int.MinValue;

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
                score = -NegaScoutSearch(board, Opponent(player), depth - 1, -b, -Math.Max(alpha, best));

                if (!first && alpha < score && score < beta)
                {
                    score = -NegaScoutSearch(board, Opponent(player), depth - 1, -beta, -score);
                }
            }

            UndoMove(board, col, row);

            if (score > best)
            {
                best = score;
            }

            alpha = Math.Max(alpha, best);
            if (alpha >= beta) break;

            b = alpha + 1;
            first = false;
        }

        return best;
    }

    private List<int> GetLegalMoves(Board board)
    {
        List<int> moves = new List<int>();
        for (int c = 0; c < board.Columns; c++)
            if (board.GetLowestEmptyRow(c) != -1)
                moves.Add(c);
        return moves;
    }

    private void OrderMovesCenterFirst(List<int> moves, Board board)
    {
        int center = board.Columns / 2;
        moves.Sort((a, b) => Math.Abs(a - center).CompareTo(Math.Abs(b - center)));
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
        int center = board.Columns / 2;
        int centerCount = 0;
        for (int r = 0; r < board.Rows; r++)
            if (board.GetCell(center, r) == perspective) centerCount++;
        score += centerCount * 3;

        for (int c = 0; c < board.Columns; c++)
        {
            for (int r = 0; r < board.Rows; r++)
            {
                if (c + 3 < board.Columns)
                    score += EvalWindow(board, c, r, 1, 0, perspective);
                if (r + 3 < board.Rows)
                    score += EvalWindow(board, c, r, 0, 1, perspective);
                if (c + 3 < board.Columns && r + 3 < board.Rows)
                    score += EvalWindow(board, c, r, 1, 1, perspective);
                if (c + 3 < board.Columns && r - 3 >= 0)
                    score += EvalWindow(board, c, r, 1, -1, perspective);
            }
        }
        return score;
    }

    private int EvalWindow(Board board, int c, int r, int dc, int dr, Player perspective)
    {
        int myCount = 0, oppCount = 0, emptyCount = 0;
        for (int i = 0; i < 4; i++)
        {
            Player p = board.GetCell(c + dc * i, r + dr * i);
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