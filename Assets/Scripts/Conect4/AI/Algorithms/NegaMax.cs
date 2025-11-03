// Copyright © 2025 Samuel Campos Borrego, Laura Gallego Fernández, Icía Fernández Fornos. Todos los derechos reservados.

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NegaMax : SearchAlgorithmBase
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public override int GetBestMove(Board board, Player aiPlayer, int maxDepth, int timeLimitMs = 0)
    {
        // Stub simple: devolver una columna legal aleatoria o -1 si no hay movimientos.
        List<int> legal = new List<int>();
        for (int c = 0; c < board.Columns; c++)
            if (board.GetLowestEmptyRow(c) != -1) legal.Add(c);

        if (legal.Count == 0) return -1;
        return legal[UnityEngine.Random.Range(0, legal.Count)];
    }

    //IMPORTANTE, la función GetBestMove tiene que estar si o si, ya que hereda de la clase SearchAlgorithmBase, lo que he puesto es proxy, es decir, esta mal, es solo para que no salte error
}
