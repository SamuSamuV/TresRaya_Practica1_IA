// Copyright © 2025 Samuel Campos Borrego, Laura Gallego Fernández, Icía Fernández Fornos. Todos los derechos reservados.

using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    public SearchAlgorithmBase searchAlgorithm;
    public Player aiPlayer = Player.Yellow;
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

        // Place move on board (this only updates logical grid; GameManager should handle visuals normally)
        int row = board.GetLowestEmptyRow(col);
        if (row == -1) return;

        // Use the same flow as human: set grid and then optionally animate via GameManager
        board.SetCell(col, row, aiPlayer);

        // Let GameManager handle win/draw and visual spawn. If you want to reuse GameManager flow:
        // GameManager.Instance.board.SetCell(col,row,aiPlayer);
        // GameManager.Instance.currentPlayer = (aiPlayer == Player.Red) ? Player.Yellow : Player.Red;
        // etc...
        // But if your GameManager expects animation, call its coroutine or a method that spawns visuals.
        // Example: you can call a GameManager method that does the spawn and drop animation instead of directly SetCell.
    }
}
