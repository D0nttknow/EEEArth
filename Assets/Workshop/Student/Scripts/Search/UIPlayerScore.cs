using Searching;
using TMPro;
using UnityEngine;

public class UIPlayerScore : MonoBehaviour
{
    public TMP_Text Text;

    public void SetUpTextScore(PlayerScore txt) { 
        Text.text = txt.playerName + " " + txt.score;
    }
}
