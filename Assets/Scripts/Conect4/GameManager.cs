using System.Collections;
using TMPro;
using UnityEngine;

public enum Player { None = 0, Red = 1, Yellow = 2 }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Board settings")]
    public int columns = 7;
    public int rows = 6;

    [Header("References")]
    public Board board;

    [Header("Gameplay")]
    public Player currentPlayer = Player.Red;
    public bool isBusy = false; // used while piece is dropping
    public bool gameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    private void Start()
    {
        board.Init(columns, rows);
        UIManager.Instance.UpdateTurnText(currentPlayer);
    }

    public void TryPlaceInColumn(int column)
    {
        if (isBusy) return;
        if (column < 0 || column >= columns) return;

        int row = board.GetLowestEmptyRow(column);
        if (row == -1) return; // column full

        StartCoroutine(PlacePieceCoroutine(column, row));
    }

    private IEnumerator PlacePieceCoroutine(int column, int row)
    {
        isBusy = true;
        // instantiate visual piece and drop animation handled by Board
        GameObject pieceObj = board.SpawnPiece(column, rows, currentPlayer); // spawn above board
        yield return board.DropPieceToRow(pieceObj, column, row);

        board.SetCell(column, row, currentPlayer);

        // check win
        if (board.CheckWin(column, row, currentPlayer))
        {
            UIManager.Instance.ShowWinner(currentPlayer);
            isBusy = false;
            gameOver = true;
            yield break;
        }

        // check draw
        if (board.IsFull())
        {
            UIManager.Instance.ShowDraw();
            isBusy = false;
            gameOver = true;
            yield break;
        }

        // switch player
        currentPlayer = (currentPlayer == Player.Red) ? Player.Yellow : Player.Red;
        UIManager.Instance.UpdateTurnText(currentPlayer);
        isBusy = false;
    }

    public void RestartGame()
    {
        StopAllCoroutines();
        board.ClearBoard();
        currentPlayer = Player.Red;
        UIManager.Instance.ResetUI();
        UIManager.Instance.UpdateTurnText(currentPlayer);
        isBusy = false;
    }
}