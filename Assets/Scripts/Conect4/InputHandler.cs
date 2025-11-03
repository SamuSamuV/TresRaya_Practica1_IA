// Copyright © 2025 Samuel Campos Borrego, Laura Gallego Fernández, Icía Fernández Fornos. Todos los derechos reservados.

using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public Camera mainCamera;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Update()
    {
        if (GameManager.Instance.gameOver) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 world = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            int column = GetColumnFromWorld(world);
            GameManager.Instance.TryPlaceInColumn(column);
        }
    }

    private int GetColumnFromWorld(Vector2 worldPos)
    {
        Board b = GameManager.Instance.board;
        float relX = worldPos.x - b.origin.x;
        if (relX < 0f) return -1;
        float colFloat = (relX + b.cellSize.x / 2f) / b.cellSize.x;
        int column = Mathf.FloorToInt(colFloat);
        if (column < 0 || column >= b.columns) return -1;

        return column;
    }
}