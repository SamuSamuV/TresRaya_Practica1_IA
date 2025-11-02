using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public TMP_Text turnText;
    public GameObject winnerPanel;
    public TMP_Text winnerText;
    public Button restartButton;
    public GameObject drawPanel;
    [SerializeField] GameObject gm;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    private void Start()
    {
        if (restartButton != null) restartButton.onClick.AddListener(() => GameManager.Instance.RestartGame());
        ResetUI();
    }

    public void UpdateTurnText(Player p)
    {
        if (turnText == null) return;
        turnText.text = (p == Player.Red) ? "Red's turn" : "Yellow's turn";
    }

    public void ShowWinner(Player p)
    {
        if (winnerPanel == null || winnerText == null) return;
        winnerPanel.SetActive(true);
        winnerText.text = (p == Player.Red) ? "Red wins!" : "Yellow wins!";

    }

    public void ShowDraw()
    {
        if (drawPanel == null) return;
        drawPanel.SetActive(true);
    }

    public void ResetUI()
    {
        if (winnerPanel != null) winnerPanel.SetActive(false);
        if (drawPanel != null) drawPanel.SetActive(false);
        GameManager.Instance.gameOver = false;
    }
}