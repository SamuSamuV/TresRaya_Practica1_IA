// Copyright © 2025 Samuel Campos Borrego, Laura Gallego Fernández, Icía Fernández Fornos. Todos los derechos reservados.

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Piece : MonoBehaviour
{
    private SpriteRenderer sr;
    public Sprite redSprite;
    public Sprite yellowSprite;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetPlayer(Player p)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (p == Player.Red) sr.sprite = redSprite;
        else if (p == Player.Yellow) sr.sprite = yellowSprite;
        else sr.sprite = null;

        Board board = FindObjectOfType<Board>();
        if (board != null && sr.sprite != null)
        {
            Vector2 spriteSize = sr.sprite.bounds.size;
            float desiredHeight = board.cellSize.y * 0.9f;
            float scale = desiredHeight / spriteSize.y;
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}