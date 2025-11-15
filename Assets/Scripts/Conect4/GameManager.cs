// Copyright © 2025 Samuel Campos Borrego, Laura Gallego Fernández, Icía Fernández Fornos. Todos los derechos reservados.

using System.Collections;
using UnityEngine;

public enum Player { None = 0, MyMelody = 1, Kuromi = 2 }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Board settings")]
    public int columns = 7;
    public int rows = 6;

    [Header("References")]
    public Board board;
    public AIPlayer aiPlayer;

    [Header("Gameplay")]
    public Player currentPlayer = Player.MyMelody;
    public bool isBusy = false;
    public bool gameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    private void Start()
    {
        RandomInitPlayer();
        board.Init(columns, rows);
        UIManager.Instance.UpdateTurnText(currentPlayer);
    }

    public void TryPlaceInColumn(int column)
    {
        if (isBusy || gameOver) return;
        if (column < 0 || column >= columns) return;

        int row = board.GetLowestEmptyRow(column);
        if (row == -1) return;

        StartCoroutine(PlacePieceCoroutine(column, row));
    }

    private IEnumerator PlacePieceCoroutine(int column, int row)
    {
        isBusy = true;
        GameObject pieceObj = board.SpawnPiece(column, rows, currentPlayer);
        yield return board.DropPieceToRow(pieceObj, column, row);

        board.SetCell(column, row, currentPlayer);

        if (board.CheckWin(column, row, currentPlayer))
        {
            UIManager.Instance.ShowWinner(currentPlayer);
            isBusy = false;
            gameOver = true;
            yield break;
        }

        if (board.IsFull())
        {
            UIManager.Instance.ShowDraw();
            isBusy = false;
            gameOver = true;
            yield break;
        }

        currentPlayer = (currentPlayer == Player.MyMelody) ? Player.Kuromi : Player.MyMelody;
        UIManager.Instance.UpdateTurnText(currentPlayer);
        isBusy = false;

        if (aiPlayer != null && currentPlayer == aiPlayer.aiPlayer && !gameOver)
        {
            StartCoroutine(AIPlayTurn());
        }
    }

    private IEnumerator AIPlayTurn()
    {
        yield return new WaitForSeconds(0.3f);

        int col = aiPlayer.searchAlgorithm.GetBestMove(board, aiPlayer.aiPlayer, aiPlayer.searchDepth);
        if (col < 0) yield break;

        TryPlaceInColumn(col);
    }

    public void RestartGame()
    {
        StopAllCoroutines();
        board.ClearBoard();
        currentPlayer = Player.MyMelody;
        UIManager.Instance.ResetUI();
        UIManager.Instance.UpdateTurnText(currentPlayer);
        isBusy = false;
        gameOver = false;
        RandomInitPlayer();
    }

    void RandomInitPlayer()
    {
        if (Random.Range(1, 3) == 1)
        {
            StartCoroutine(AIPlayTurn());
            currentPlayer = Player.Kuromi;
        }

        else
        {
            currentPlayer = Player.MyMelody;
        }

        UIManager.Instance.UpdateTurnText(currentPlayer);
    }
}
