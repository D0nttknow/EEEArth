using UnityEngine;
using TMPro;

public class GameOverDisplay : MonoBehaviour
{
    public TextMeshProUGUI finalScoreText;

    private void Start()
    {
        RefreshScore();
    }

    // เรียกตอนเข้าฉากครั้งแรก และเรียกซ้ำอีกทีหลังดู Rewarded Ad จบ (x2 คะแนน)
    public void RefreshScore()
    {
        int score = PlayerPrefs.GetInt("FinalScore", 0);
        finalScoreText.text = "Final Score: " + score;
    }

    // ผูกปุ่ม "x2 Reward" กับฟังก์ชันนี้ตรงๆ ใน On Click()
    public void OnClickRewardButton()
    {
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowRewardedVideoAds();
        }
    }
}