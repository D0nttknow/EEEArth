using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI npcText;
    public Transform choiceContainer;
    public Button choiceButtonPrefab; // ลาก Prefab ปุ่มตัวเลือกมาใส่
    public GameObject closeButtonDialogue;
    private DialogueSequen InterractNpcSequen;

    // เก็บปุ่มที่ถูกสร้างขึ้น เพื่อนำไปทำลาย/ซ่อนในภายหลัง
    private List<Button> activeButtons = new List<Button>();

    public void Setup(DialogueSequen sequen)
    {
        this.InterractNpcSequen = sequen;
        DialogueNode currentNode = InterractNpcSequen.tree.root;
        ShowDialogue(currentNode);

        closeButtonDialogue.SetActive(false);
        gameObject.SetActive(true);
    }

    public void ShowDialogue(DialogueNode node)
    {
        InterractNpcSequen.currentNode = node;

        // 1. แสดงข้อความของ NPC
        npcText.text = node.text;

        // 2. ล้างปุ่มตัวเลือกเก่า
        ClearChoices();

        // 3. สร้างปุ่มตัวเลือกใหม่ตาม nexts
        var choices = new List<string>(node.nexts.Keys);
        for (int i = 0; i < choices.Count; i++)
        {
            string choiceText = choices[i];
            CreateChoiceButton(choiceText, i);
        }
    }

    private void CreateChoiceButton(string text, int index)
    {
        Button newButton = Instantiate(choiceButtonPrefab, choiceContainer);

        // ตั้งค่าข้อความบนปุ่ม
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = text;

        // เพิ่ม Listener เมื่อกดปุ่ม
        // ใช้ Lambda Expression เพื่อส่ง index กลับไปให้ DialogueManager
        newButton.onClick.AddListener(() => OnChoiceSelected(index));

        activeButtons.Add(newButton);
    }

    private void ClearChoices()
    {
        foreach (Button button in activeButtons)
        {
            Destroy(button.gameObject);
        }
        activeButtons.Clear();
    }

    private void OnChoiceSelected(int index)
    {
        // ส่ง index ของตัวเลือกที่ผู้เล่นเลือกกลับไปให้ DialogueManager จัดการ
        InterractNpcSequen.SelectChoice(index);
    }
    public void ShowCloseButtonDialog() {
        closeButtonDialogue.gameObject.SetActive(true);
    }
    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
        ClearChoices();
    }
}
