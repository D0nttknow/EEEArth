using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float timeLeft = 30f;

    public ScoreManager scoreManager; // ลาก ScoreManager มาใส่ใน Inspector

    public string gameOverSceneName = "GameOver";

    private bool isGameOver = false;

    private void Update()
    {
        if (isGameOver) return;

        // ถ้าโฆษณาเปิดอยู่ ให้พักการนับเวลาไว้ก่อน
        if (AdsManager.IsGamePaused) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            UpdateTimerText();
            TriggerGameOver();
        }
        else
        {
            UpdateTimerText();
        }
    }

    private void UpdateTimerText()
    {
        // แสดงผลแบบปัดเป็นจำนวนเต็ม เช่น "Time: 30"
        timerText.text = "Time: " + Mathf.CeilToInt(timeLeft);
    }

    private void TriggerGameOver()
    {
        isGameOver = true;

        // เก็บคะแนนสุดท้ายไว้ส่งต่อไปหน้า GameOver
        int finalScore = scoreManager != null ? scoreManager.GetScore() : 0;
        PlayerPrefs.SetInt("FinalScore", finalScore);
        PlayerPrefs.Save();

        // โชว์ Interstitial ก่อน แล้วค่อยเข้า GameOver
        // (ฟังก์ชันนี้จัดการเองว่าจะรอผู้เล่นปิดโฆษณาก่อน แล้วค่อยเปลี่ยน Scene
        //  หรือถ้าโฆษณายังไม่พร้อม ก็จะข้ามไปหน้า GameOver ให้เลยทันที)
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowInterstitialThenLoadScene(gameOverSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
    }
}