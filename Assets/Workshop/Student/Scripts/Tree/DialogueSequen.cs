using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class DialogueSequen : MonoBehaviour
    {
        public DialogueTree tree;
        public DialogueNode currentNode;
        public DialogueUI dialogueUI; // ลาก DialogueUI Component มาใส่

        public void Start()
        {

            LoadConversations();

        }

        private void LoadConversations()
        {
            // NPC: Ah, traveler! What brings you to this old place?
            //     |
            //     +-- [1] Can you give me a quest?
            //     |       |
            //     |       +-- NPC: I have a task for you. There’s a beast in the woods. Can you take care of it?
            //     |               |
            //     |               +-- [1] I’m ready for anything!
            //     |               |       |
            //     |               |       +-- NPC: You're not ready for this yet. Come back when you're stronger.
            //     |               |
            //     |               +-- [2] Maybe later.
            //     |                       |
            //     |                       +-- NPC: Safe travels, adventurer.
            //     |
            //     +-- [2] Where is the village?
            //     |       |
            //     |       +-- NPC: Follow the road south, and you’ll reach the village.
            //     |
            //     +-- [3] How do I get to the forest?
            //     |       |
            //     |       +-- NPC: Head west, into the forest. But beware, it's dangerous.
            //     |
            //     +-- [4] Goodbye.
            //             |
            //             +-- NPC: Safe travels, adventurer.

            // Create the dialogue nodes
            DialogueNode greeting = new DialogueNode("Ah, traveler! What brings you to this old place?");
            DialogueNode askForQuest = new DialogueNode("I have a task for you. There’s a beast in the woods. Can you take care of it?");
            DialogueNode questDenied = new DialogueNode("You're not ready for this yet. Come back when you're stronger.");
            DialogueNode directionsVillage = new DialogueNode("Follow the road south, and you’ll reach the village.");
            DialogueNode directionsForest = new DialogueNode("Head west, into the forest. But beware, it's dangerous.");
            DialogueNode goodbye = new DialogueNode("Safe travels, adventurer.");
            DialogueNode noIdea = new DialogueNode("I'm afraid I can't help you with that.");

            // Build the tree, adding custom responses
            greeting.AddNext(askForQuest, "Can you give me a quest?");
            greeting.AddNext(directionsVillage, "Where is the village?");
            greeting.AddNext(directionsForest, "How do I get to the forest?");
            greeting.AddNext(goodbye, "Goodbye.");

            askForQuest.AddNext(questDenied, "I’m ready for anything!");
            askForQuest.AddNext(goodbye, "Maybe later.");

            // Set up the root of the dialogue tree
            tree = new DialogueTree(greeting);
        }

    // **เมธอดใหม่สำหรับรับการเลือกจากปุ่ม UI**
    public void SelectChoice(int index)
    {
        var choiceTextKeys = new List<string>(currentNode.nexts.Keys);

        if (index >= 0 && index < choiceTextKeys.Count)
        {
            string choiceKey = choiceTextKeys[index];

            // 1. เลื่อนไปยัง Dialogue Node ถัดไป
            currentNode = currentNode.nexts[choiceKey];

            // 2. ตรวจสอบว่ามีตัวเลือกถัดไปหรือไม่ (จบการสนทนา)
            if (currentNode.nexts.Count > 0)
            {
                dialogueUI.ShowDialogue(currentNode); // แสดง Node ถัดไป
            }
            else
            {
                // ถ้าไม่มีตัวเลือกถัดไป ถือว่าจบบทสนทนา
                dialogueUI.ShowDialogue(currentNode);   // แสดงข้อความสุดท้าย
                dialogueUI.ShowCloseButtonDialog();    // อาจเพิ่ม Delay และเรียก dialogueUI.HideDialogue() ที่นี่
                                                      // หรือทำให้ปุ่ม "ปิด" แสดงขึ้นมา
            }
        }
    }
}

