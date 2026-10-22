using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.LevelPlay;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("ชื่อ Scene ที่อยาก 'โชว์' Banner (ที่เหลือจะซ่อนให้อัตโนมัติ)")]
    public string bannerVisibleSceneName = "Start";

    private LevelPlayBannerAd bannerAd;
    private LevelPlayInterstitialAd interstitialAd;
    private LevelPlayRewardedAd rewardedVideoAd;

    private bool isAdsEnabled = false;
    private string pendingSceneAfterInterstitial = null;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // คอยฟังทุกครั้งที่เปลี่ยน Scene เพื่อโชว์/ซ่อน Banner เอง
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isAdsEnabled || bannerAd == null) return;

        if (scene.name == bannerVisibleSceneName)
        {
            ShowBannerOnStart();
        }
        else
        {
            HideBanner();
        }
    }

    private void Start()
    {
        LevelPlay.ValidateIntegration();
        LevelPlay.OnInitSuccess += SdkInitCompletedEvent;
        LevelPlay.OnInitFailed += SdkInitFailedEvent;
        LevelPlay.Init(AdConfig.AppKey);
    }

    private void SdkInitFailedEvent(LevelPlayInitError error)
    {
        Debug.Log($"Ads not enabled: {error}");
    }

    private void SdkInitCompletedEvent(LevelPlayConfiguration configuration)
    {
        EnableAds();
        isAdsEnabled = true;
    }

    private void EnableAds()
    {
        // ----- Banner -----
        var configBuilder = new LevelPlayBannerAd.Config.Builder();
        configBuilder.SetSize(LevelPlayAdSize.BANNER)
            .SetPosition(LevelPlayBannerPosition.BottomCenter);
        var bannerConfig = configBuilder.Build();

        bannerAd = new LevelPlayBannerAd(AdConfig.BannerAdUnitId, bannerConfig);
        bannerAd.OnAdLoaded += (info) => bannerAd.ShowAd();
        bannerAd.OnAdLoadFailed += (error) => Debug.LogError("Banner โหลดล้มเหลว: " + error);

        // ----- Interstitial -----
        interstitialAd = new LevelPlayInterstitialAd(AdConfig.InterstitialAdUnitId);
        interstitialAd.OnAdLoadFailed += (error) => Debug.LogError("Interstitial โหลดล้มเหลว: " + error);
        interstitialAd.OnAdDisplayed += (info) => PauseGame();   // เปิดโฆษณา -> หยุดเกม
        interstitialAd.OnAdClosed += (info) =>
        {
            ResumeGame();          // ปิดโฆษณา -> คืนเวลาเกม
            OnInterstitialClosed();
        };
        interstitialAd.LoadAd();

        // ----- Rewarded -----
        rewardedVideoAd = new LevelPlayRewardedAd(AdConfig.RewardedVideoAdUnitId);
        rewardedVideoAd.OnAdLoadFailed += (error) => Debug.LogError("Rewarded โหลดล้มเหลว: " + error);
        rewardedVideoAd.OnAdDisplayed += (info) => PauseGame();  // เปิดโฆษณา -> หยุดเกม
        rewardedVideoAd.OnAdRewarded += (info, reward) => OnRewardSuccess();
        rewardedVideoAd.OnAdClosed += (info) => ResumeGame();    // ปิดโฆษณา (ไม่ว่าดูจบหรือข้าม) -> คืนเวลาเกม
        rewardedVideoAd.LoadAd();
    }

    // =====================================================
    // หยุด/คืนเวลาเกมตอนโฆษณาเปิด-ปิด
    // ไม่ใช้ Time.timeScale เพราะมันจะไปหยุด Canvas/Animation
    // ที่ใช้แสดง Mock Ad ใน Editor ด้วย ใช้ Flag ของตัวเองแทน
    // =====================================================

    public static bool IsGamePaused { get; private set; } = false;

    private void PauseGame()
    {
        IsGamePaused = true;
    }

    private void ResumeGame()
    {
        IsGamePaused = false;
    }

    // =====================================================
    // BANNER — เรียกตอนเข้าหน้า Start
    // =====================================================

    public void ShowBannerOnStart()
    {
        if (bannerAd == null) return;
        bannerAd.LoadAd(); // พอโหลดเสร็จ OnAdLoaded จะสั่ง ShowAd() ให้เอง
    }

    public void HideBanner()
    {
        bannerAd?.HideAd();
    }

    // =====================================================
    // INTERSTITIAL — เรียกก่อนเข้า Scene GameOver
    // =====================================================

    // เรียกฟังก์ชันนี้แทนการ SceneManager.LoadScene ตรงๆ
    // จะโชว์โฆษณาก่อน แล้วค่อยเปลี่ยน Scene เมื่อผู้เล่นปิดโฆษณาเอง
    public void ShowInterstitialThenLoadScene(string sceneName)
    {
        pendingSceneAfterInterstitial = sceneName;

        if (interstitialAd != null && interstitialAd.IsAdReady())
        {
            interstitialAd.ShowAd();
        }
        else
        {
            // โฆษณายังไม่พร้อม ข้ามไปหน้าถัดไปเลยไม่ต้องรอ
            Debug.Log("Interstitial ยังไม่พร้อม ข้ามไปหน้าถัดไปเลย");
            SceneManager.LoadScene(sceneName);
            pendingSceneAfterInterstitial = null;
        }
    }

    // ผู้เล่นกดปิดโฆษณาเอง (ปุ่ม X มุมโฆษณา) จะเข้ามาที่นี่
    private void OnInterstitialClosed()
    {
        if (!string.IsNullOrEmpty(pendingSceneAfterInterstitial))
        {
            SceneManager.LoadScene(pendingSceneAfterInterstitial);
            pendingSceneAfterInterstitial = null;
        }

        interstitialAd.LoadAd(); // โหลดเตรียมไว้ใช้รอบหน้า
    }

    // =====================================================
    // REWARDED — เรียกตอนกดปุ่ม "x2 Reward" ที่ GameOver
    // =====================================================

    public void ShowRewardedVideoAds()
    {
        if (rewardedVideoAd != null && rewardedVideoAd.IsAdReady())
        {
            rewardedVideoAd.ShowAd();
        }
        else
        {
            Debug.Log("Rewarded Ad ยังไม่พร้อม");
        }
    }

    // ผู้เล่นดูจบจริง ถึงจะเข้ามาที่นี่ (ดูไม่จบ/ปิดกลางคัน จะไม่เข้า)
    private void OnRewardSuccess()
    {
        int currentScore = PlayerPrefs.GetInt("FinalScore", 0);
        int newScore = currentScore * 2;
        PlayerPrefs.SetInt("FinalScore", newScore);
        PlayerPrefs.Save();

        // สั่งให้หน้า GameOver รีเฟรชตัวเลขคะแนนที่โชว์อยู่
        GameOverDisplay display = FindObjectOfType<GameOverDisplay>();
        if (display != null) display.RefreshScore();

        rewardedVideoAd.LoadAd(); // โหลดเตรียมไว้เผื่อใช้อีก
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        bannerAd?.DestroyAd();
        interstitialAd?.DestroyAd();
        rewardedVideoAd?.DestroyAd();
    }
}