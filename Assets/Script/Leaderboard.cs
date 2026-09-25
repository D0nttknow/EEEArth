using UnityEngine;
using System.Linq;

public class LeaderboardManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerData
    {
        public string name;
        public int score;
    }

    public GameObject rowPrefab;
    public Transform contentParent;
    public PlayerData[] Table;

    void Start()
    {
        GenerateLeaderboard();
    }

    void GenerateLeaderboard()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        PlayerData[] sortedTable = Table.OrderByDescending(p => p.score).ToArray();

        for (int i = 0; i < sortedTable.Length; i++)
        {
            int rank = i + 1;
            string name = sortedTable[i].name;
            int score = sortedTable[i].score;

            GameObject row = Instantiate(rowPrefab, contentParent);

            row.transform.Find("RankText").GetComponent<TMPro.TextMeshProUGUI>().text = rank.ToString();
            row.transform.Find("NameText").GetComponent<TMPro.TextMeshProUGUI>().text = name;
            row.transform.Find("ScoreText").GetComponent<TMPro.TextMeshProUGUI>().text = score + " m";
        }
    }
}