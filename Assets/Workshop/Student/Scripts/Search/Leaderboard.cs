using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Searching
{
    public class Leaderboard : MonoBehaviour
    {
        private List<PlayerScore> scores = new List<PlayerScore>();
        public GameObject UIScore;
        public Transform UiParent;
        void Awake()
        {
            // Add 20 initial scores in unsorted order
            RecordScore(new PlayerScore("Alice", 100));
            RecordScore(new PlayerScore("Bob", 50));
            RecordScore(new PlayerScore("Charlie", 75));
            RecordScore(new PlayerScore("David", 25));
            RecordScore(new PlayerScore("Eve", 125));
            RecordScore(new PlayerScore("Frank", 150));
            RecordScore(new PlayerScore("Lily", 300));
            RecordScore(new PlayerScore("Grace", 175));
            RecordScore(new PlayerScore("Oscar", 375));
            RecordScore(new PlayerScore("Ivan", 225));
            RecordScore(new PlayerScore("Heidi", 200));
            RecordScore(new PlayerScore("Judy", 250));
            RecordScore(new PlayerScore("Kevin", 275));
            RecordScore(new PlayerScore("Nina", 350));
            RecordScore(new PlayerScore("Mona", 325));
            
        }

        public void RecordScore(PlayerScore score)
        {
            // [1] sequential search if the player is already in the list

            int index = -1;
            for (int i = 0; i < scores.Count; i++)
            {
                if (scores[i].playerName == score.playerName)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                scores.RemoveAt(index);
            }
            // [2] find index to insert that make the scores list sorted with binary search
            index = -1; // reset index
            int left = 0;
            int right = scores.Count - 1;
            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (scores[mid].score == score.score)
                {
                    index = mid;
                    break;
                }
                else if (scores[mid].score < score.score)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            // [3] If the score is not found, insert it at the appropriate index
            if (index == -1)
            {
                index = left;
            }
            scores.Insert(index, score);

        }

        public void PrintScores()
        {
            // join all score as string and print it
            string allScores = scores.Aggregate("", (acc, score) => acc + score.score.ToString() + ",");
            Debug.Log(allScores);
            
        }
        public void ShowleaderBoard() {
            foreach (var score in scores)
            {
                UIPlayerScore uIScore = Instantiate(UIScore, UiParent).GetComponent<UIPlayerScore>();
                uIScore.SetUpTextScore(score);
            }
        }
    }
}
