using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    private int currentScore = 0;

    private void Start()
    {
        UpdateScoreText();
    }

    // ผูกฟังก์ชันนี้กับปุ่ม (Button > OnClick)
    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + currentScore;
    }

    public int GetScore()
    {
        return currentScore;
    }
}