// Copyright © 2025 Samuel Campos Borrego, Laura Gallego Fernández, Icía Fernández Fornos. Todos los derechos reservados.

using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    public SearchAlgorithmBase searchAlgorithm;
    public Player aiPlayer = Player.Kuromi;
    public int searchDepth = 6;

    public void MakeAIMove(Board board)
    {
        if (searchAlgorithm == null)
        {
            Debug.LogError("Search algorithm not assigned.");
            return;
        }

        int col = searchAlgorithm.GetBestMove(board, aiPlayer, searchDepth);
        if (col < 0) return;

        int row = board.GetLowestEmptyRow(col);
        if (row == -1) return;

        board.SetCell(col, row, aiPlayer);
    }
}
