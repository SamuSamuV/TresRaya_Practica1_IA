// Copyright © 2025 Samuel Campos Borrego, Laura Gallego Fernández, Icía Fernández Fornos. Todos los derechos reservados.

using UnityEngine;

public abstract class SearchAlgorithmBase : MonoBehaviour
{
    public abstract int GetBestMove(Board board, Player aiPlayer, int maxDepth, int timeLimitMs = 0);
}
