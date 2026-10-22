using UnityEngine;
using TMPro;

public class ResultGameOver : MonoBehaviour
{
    public TextMeshProUGUI finalScoreText;

    private void Start()
    {
        int score = PlayerPrefs.GetInt("FinalScore", 0);
        finalScoreText.text = "Final Score: " + score;
    }
}
