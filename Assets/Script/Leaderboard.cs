using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;

public class LeaderboardManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerData
    {
        public string name;
        public int score;
        public string playerId;
    }

    public GameObject rowPrefab;
    public Transform contentParent;
    public ProfileImageManager profileImages;
    public PlayerData[] Table;

    [Header("UGS")]
    public string leaderboardId = "main_leaderboard";
    public bool useUGS = true;

    private bool subscribed;

    private async void Start()
    {
        if (!useUGS)
        {
            ShowFallback();
            return;
        }

        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync();

            // เผื่อ script อื่นกำลัง Initialize อยู่
            while (UnityServices.State == ServicesInitializationState.Initializing)
                await Task.Yield();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Initialize UGS ไม่สำเร็จ: " + e);
            ShowFallback();
            return;
        }

        if (this == null) return; // object ถูกทำลายระหว่างรอ

        AuthenticationService.Instance.SignedIn += OnSignedIn;
        AuthenticationService.Instance.SignedOut += OnSignedOut;
        subscribed = true;

        if (AuthenticationService.Instance.IsSignedIn)
            _ = LoadFromUGS();
        else
            ShowFallback(); // รอ login แล้ว OnSignedIn จะโหลดให้เอง
    }

    private void OnDestroy()
    {
        if (!subscribed) return;
        AuthenticationService.Instance.SignedIn -= OnSignedIn;
        AuthenticationService.Instance.SignedOut -= OnSignedOut;
    }

    private void OnSignedIn() => _ = LoadFromUGS();
    private void OnSignedOut() => ShowFallback();

    private void ShowFallback()
    {
        GenerateLeaderboard(Table.OrderByDescending(p => p.score).ToArray());
    }

    private async Task LoadFromUGS()
    {
        try
        {
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId);

            PlayerData[] result = scoresResponse.Results
                .Select(entry => new PlayerData
                {
                    name = entry.PlayerName.Split('#')[0],
                    score = (int)entry.Score,
                    playerId = entry.PlayerId
                })
                .ToArray();

            if (this != null) GenerateLeaderboard(result);
        }
        catch (System.Exception e)
        {
            Debug.LogError("โหลด Leaderboard จาก UGS ไม่สำเร็จ: " + e);
        }
    }
    private async Task LoadAvatarInto(UnityEngine.UI.RawImage target, string playerId)
    {
        Texture2D tex = await profileImages.LoadAvatar(playerId);
        if (tex != null && target != null) target.texture = tex;
    }
    private void GenerateLeaderboard(PlayerData[] sortedTable)
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        for (int i = 0; i < sortedTable.Length; i++)
        {
            GameObject row = Instantiate(rowPrefab, contentParent);

            row.transform.Find("RankText").GetComponent<TMPro.TextMeshProUGUI>().text = (i + 1).ToString();
            row.transform.Find("NameText").GetComponent<TMPro.TextMeshProUGUI>().text = sortedTable[i].name;
            row.transform.Find("ScoreText").GetComponent<TMPro.TextMeshProUGUI>().text = sortedTable[i].score + " m";
            var avatar = row.transform.Find("AvatarImage")?.GetComponent<UnityEngine.UI.RawImage>();
            if (avatar != null && profileImages != null && !string.IsNullOrEmpty(sortedTable[i].playerId))
                _ = LoadAvatarInto(avatar, sortedTable[i].playerId);
        }
    }

    public async void SubmitScore(int score)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("ยังไม่ได้ Sign in ส่งคะแนนไม่ได้");
            return;
        }

        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
            Debug.Log("ส่งคะแนนสำเร็จ: " + score);

            if (useUGS) _ = LoadFromUGS();
        }
        catch (System.Exception e)
        {
            Debug.LogError("ส่งคะแนนล้มเหลว: " + e);
        }
    }
    [ContextMenu("Random Score")]
    private void TestSubmitScore()
    {
        SubmitScore(Random.Range(10, 500));
    }

    [ContextMenu("Reload Leaderboard")]
    private void TestReload()
    {
        _ = LoadFromUGS();
    }
   
}
