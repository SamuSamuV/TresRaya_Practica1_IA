using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public Button[] cells;
    public TMP_Text[] cellTexts;
    public TMP_Text resultText;
    public GameObject resultPanel;

    private char[] board = new char[9];
    private char human = 'X';
    private char ai = 'O';
    private bool gameOver = false;

    void Start()
    {
        ResetBoard();
    }

    public void OnCellClicked(int index)
    {
        if (gameOver || board[index] != ' ')
            return;

        MakeMove(index, human);

        if (CheckGameOver())
            return;

        AIMove();
        CheckGameOver();
    }

    void MakeMove(int index, char player)
    {
        board[index] = player;
        cellTexts[index].text = player.ToString();
        cells[index].interactable = false;
    }

    void AIMove()
    {
        int bestScore = int.MinValue;
        int move = -1;

        for (int i = 0; i < 9; i++)
        {
            if (board[i] == ' ')
            {
                board[i] = ai;
                int score = Minimax(board, 0, false);
                board[i] = ' ';
                if (score > bestScore)
                {
                    bestScore = score;
                    move = i;
                }
            }
        }

        MakeMove(move, ai);
    }

    int Minimax(char[] b, int depth, bool isMaximizing)
    {
        if (CheckWin(b, ai)) return 10 - depth;
        if (CheckWin(b, human)) return depth - 10;
        if (IsFull(b)) return 0;

        if (isMaximizing)
        {
            int bestScore = int.MinValue;
            for (int i = 0; i < 9; i++)
            {
                if (b[i] == ' ')
                {
                    b[i] = ai;
                    int score = Minimax(b, depth + 1, false);
                    b[i] = ' ';
                    bestScore = Mathf.Max(bestScore, score);
                }
            }
            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;
            for (int i = 0; i < 9; i++)
            {
                if (b[i] == ' ')
                {
                    b[i] = human;
                    int score = Minimax(b, depth + 1, true);
                    b[i] = ' ';
                    bestScore = Mathf.Min(bestScore, score);
                }
            }
            return bestScore;
        }
    }

    bool CheckWin(char[] b, char player)
    {
        int[,] wins = new int[,]
        {
            {0,1,2}, {3,4,5}, {6,7,8},
            {0,3,6}, {1,4,7}, {2,5,8},
            {0,4,8}, {2,4,6}
        };

        for (int i = 0; i < wins.GetLength(0); i++)
        {
            if (b[wins[i, 0]] == player &&
                b[wins[i, 1]] == player &&
                b[wins[i, 2]] == player)
                return true;
        }
        return false;
    }

    bool IsFull(char[] b)
    {
        foreach (char c in b) if (c == ' ') return false;
        return true;
    }

    bool CheckGameOver()
    {
        if (CheckWin(board, human))
        {
            resultPanel.SetActive(true);
            resultText.text = "¡Ganaste!";
            gameOver = true;
            return true;
        }
        if (CheckWin(board, ai))
        {
            resultPanel.SetActive(true);
            resultText.text = "La IA gana!";
            gameOver = true;
            return true;
        }
        if (IsFull(board))
        {
            resultPanel.SetActive(true);
            resultText.text = "Empate!";
            gameOver = true;
            return true;
        }
        return false;
    }

    public void ResetBoard()
    {
        for (int i = 0; i < 9; i++)
        {
            board[i] = ' ';
            cellTexts[i].text = "";
            cells[i].interactable = true;
        }
        resultText.text = "";
        gameOver = false;

        resultPanel.SetActive(false);
    }
}
