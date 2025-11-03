// Copyright © 2025 Samuel Campos Borrego, Laura Gallego Fernández, Icía Fernández Fornos. Todos los derechos reservados.

using System.Collections;
using UnityEngine;

public class Board : MonoBehaviour
{
    private Player[,] grid;
    public int columns, rows;

    [Header("Visuals")]
    public GameObject piecePrefab; // simple sprite prefab with Piece.cs
    public Transform piecesParent;
    public Vector2 cellSize = new Vector2(1f, 1f);
    public Vector2 origin = new Vector2(-3f, -2.5f); // bottom-left of grid in world coords (adjust in inspector)
    public float dropSpeed = 8f; // units per second
    public float spawnYOffset = 4f; // how far above the column to spawn

    // Public read-only accessors for search algorithms
    public int Columns => columns;
    public int Rows => rows;

    public void Init(int cols, int rws)
    {
        columns = cols;
        rows = rws;
        grid = new Player[columns, rows];
        ClearBoard();
    }

    public void ClearBoard()
    {
        if (grid == null) return;
        for (int c = 0; c < columns; c++)
            for (int r = 0; r < rows; r++)
                grid[c, r] = Player.None;

        // destroy spawned pieces
        if (piecesParent != null)
        {
            for (int i = piecesParent.childCount - 1; i >= 0; i--)
                DestroyImmediate(piecesParent.GetChild(i).gameObject);
        }
    }

    // Safe getter for cell content (returns Player.None if out of range)
    public Player GetCell(int column, int row)
    {
        if (grid == null) return Player.None;
        if (column < 0 || column >= columns || row < 0 || row >= rows) return Player.None;
        return grid[column, row];
    }

    // Return a deep copy of the grid (useful for debugging / evaluation)
    public Player[,] GetGridCopy()
    {
        Player[,] copy = new Player[columns, rows];
        for (int c = 0; c < columns; c++)
            for (int r = 0; r < rows; r++)
                copy[c, r] = grid[c, r];
        return copy;
    }

    public int GetLowestEmptyRow(int column)
    {
        if (grid == null) return -1;
        if (column < 0 || column >= columns) return -1;
        for (int r = 0; r < rows; r++)
            if (grid[column, r] == Player.None) return r;
        return -1;
    }

    // Spawn centered on the column X to avoid lateral offsets
    public GameObject SpawnPiece(int column, int spawnRowAboveBoard, Player p)
    {
        Vector2 columnPos = GetWorldPosition(column, spawnRowAboveBoard);
        Vector2 spawnPos = new Vector2(columnPos.x, columnPos.y + spawnYOffset);
        GameObject go = Instantiate(piecePrefab, spawnPos, Quaternion.identity, piecesParent);
        Piece piece = go.GetComponent<Piece>();
        piece.SetPlayer(p);
        return go;
    }

    // Drop until close enough, then snap exactly to avoid float-compare issues.
    public IEnumerator DropPieceToRow(GameObject pieceObj, int column, int targetRow)
    {
        Vector2 targetPos = GetWorldPosition(column, targetRow);
        float epsilon = 0.01f;
        while (Vector2.Distance(pieceObj.transform.position, targetPos) > epsilon)
        {
            pieceObj.transform.position = Vector2.MoveTowards(pieceObj.transform.position, targetPos, dropSpeed * Time.deltaTime);
            yield return null;
        }
        // ensure exact snap
        pieceObj.transform.position = targetPos;

        // set sorting/order so lower rows draw in correct order if you need that behavior
        SpriteRenderer sr = pieceObj.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = targetRow;
    }

    public Vector2 GetWorldPosition(int column, int row)
    {
        float x = origin.x + column * cellSize.x;
        float y = origin.y + row * cellSize.y;
        return new Vector2(x, y);
    }

    public void SetCell(int column, int row, Player p)
    {
        if (grid == null) return;
        if (column < 0 || column >= columns || row < 0 || row >= rows) return;
        grid[column, row] = p;
    }

    public bool IsFull()
    {
        for (int c = 0; c < columns; c++)
            if (GetLowestEmptyRow(c) != -1) return false;
        return true;
    }

    public bool CheckWin(int column, int row, Player p)
    {
        if (p == Player.None) return false;
        Vector2Int[] dirs = new Vector2Int[] {
            new Vector2Int(1,0),
            new Vector2Int(0,1),
            new Vector2Int(1,1),
            new Vector2Int(1,-1)
        };

        foreach (var d in dirs)
        {
            int count = 1;
            count += CountDirection(column, row, d.x, d.y, p);
            count += CountDirection(column, row, -d.x, -d.y, p);
            if (count >= 4) return true;
        }
        return false;
    }

    private int CountDirection(int col, int row, int dx, int dy, Player p)
    {
        int c = col + dx;
        int r = row + dy;
        int count = 0;
        while (c >= 0 && c < columns && r >= 0 && r < rows && grid[c, r] == p)
        {
            count++;
            c += dx; r += dy;
        }
        return count;
    }
}