using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class DialogueNode
    {
        public string text;
        public Dictionary<string, DialogueNode> nexts = new Dictionary<string, DialogueNode>();

        public DialogueNode(string text)
        {
            this.text = text;
            nexts = new Dictionary<string, DialogueNode>();
        }

        public void AddNext(DialogueNode next, string choiceText)
        {
            nexts.Add(choiceText, next);
        }

        public void Print()
        {
            Debug.Log("NPC: " + text);
            var choiceText = new List<string>(nexts.Keys);
            for (int i = 0; i < choiceText.Count; i++)
            {
                Debug.Log("    +-- [" + (i + 1) + "] " + choiceText[i]);
            }
            Debug.Log("--------------------");
        }
    }

    public class DialogueTree
    {
        public DialogueNode root;

        public DialogueTree(DialogueNode root)
        {
            this.root = root;
        }
    }
