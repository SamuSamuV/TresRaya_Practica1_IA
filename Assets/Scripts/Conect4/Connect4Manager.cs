// Copyright © 2025 Samuel Campos Borrego, Laura Gallego Fernández, Icía Fernández Fornos. Todos los derechos reservados.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Connect4Manager : MonoBehaviour
{
    [Header("Board Settings")]
    public int rows = 6;
    public int cols = 7;
    public float cellSize = 1f;
    public Sprite cellSprite;
    public Sprite player1Sprite;
    public Sprite player2Sprite;

    private int[,] board;
    private GameObject[,] cellObjects;
    private int currentPlayer = 1;
    private bool gameOver = false;

    void Start()
    {
        board = new int[rows, cols];
        cellObjects = new GameObject[rows, cols];
        CreateBoard();
    }

    void CreateBoard()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                GameObject cell = new GameObject($"Cell_{r}_{c}");
                cell.transform.position = new Vector3(c * cellSize, -r * cellSize, 0);
                cell.transform.parent = transform;

                SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();
                sr.sprite = cellSprite;
                sr.sortingOrder = 0;

                cellObjects[r, c] = cell;
            }
        }
    }

    void Update()
    {
        if (gameOver) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int col = Mathf.FloorToInt(mousePos.x / cellSize);
            if (col >= 0 && col < cols)
                DropPiece(col);
        }
    }

    void DropPiece(int col)
    {
        for (int row = rows - 1; row >= 0; row--)
        {
            if (board[row, col] == 0)
            {
                board[row, col] = currentPlayer;
                PlacePieceVisual(row, col);

                if (CheckWin(row, col))
                {
                    Debug.Log($"¡Jugador {currentPlayer} ha ganado!");
                    gameOver = true;
                    return;
                }

                currentPlayer = (currentPlayer == 1) ? 2 : 1;
                return;
            }
        }

        Debug.Log("Columna llena");
    }

    void PlacePieceVisual(int row, int col)
    {
        GameObject piece = new GameObject($"Piece_{row}_{col}");
        piece.transform.position = new Vector3(col * cellSize, -row * cellSize, -0.1f);
        piece.transform.parent = transform;

        SpriteRenderer sr = piece.AddComponent<SpriteRenderer>();
        sr.sprite = (currentPlayer == 1) ? player1Sprite : player2Sprite;
        sr.sortingOrder = 1;

        // --- Ajuste automático del tamaño ---
        float spriteWidth = sr.sprite.bounds.size.x;
        float scaleFactor = cellSize / spriteWidth;
        piece.transform.localScale = Vector3.one * scaleFactor * 0.9f;
    }

    bool CheckWin(int row, int col)
    {
        int player = board[row, col];
        if (player == 0) return false;

        int[][] dirs = new int[][]
        {
            new int[]{0, 1},  // horizontal
            new int[]{1, 0},  // vertical
            new int[]{1, 1},  // diagonal
            new int[]{1, -1}  // diagonal
        };

        foreach (var d in dirs)
        {
            int count = 1;
            count += CountDirection(row, col, d[0], d[1], player);
            count += CountDirection(row, col, -d[0], -d[1], player);
            if (count >= 4) return true;
        }

        return false;
    }

    int CountDirection(int row, int col, int dRow, int dCol, int player)
    {
        int count = 0;
        int r = row + dRow;
        int c = col + dCol;

        while (r >= 0 && r < rows && c >= 0 && c < cols && board[r, c] == player)
        {
            count++;
            r += dRow;
            c += dCol;
        }
        return count;
    }
}