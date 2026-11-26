using System.Collections.Generic;
using UnityEngine;
using System.Collections; // อ้างอิงถึง Namespace ของ SkillBook

public class SkillTreeUI : MonoBehaviour
{
    public static SkillTreeUI Instance;

    [Header("Required References")]
    public SkillBook skillBook;
    public SkillNodeUI skillNodePrefab;
    public Transform skillNodeContainer; // ตำแหน่งที่จะวาง Node UI

    // **ส่วนที่เพิ่มเข้ามาสำหรับการจัดการ Scroll View Content**
    public RectTransform contentSkill; // ลาก Content RectTransform ของ Scroll View มาใส่

    // ตัวแปรสำหรับติดตามขอบเขตของ Skill Node ที่ถูกสร้าง
    private float minX = 0f;
    private float maxX = 0f;
    private float minY = 0f;
    private float maxY = 0f; // เนื่องจาก Y จะเป็นค่าลบ

    // กำหนดขนาด Node และระยะห่างเพื่อให้คำนวณง่ายขึ้น
    private readonly float NODE_WIDTH = 150f;
    private readonly float NODE_HEIGHT = 150f;
    private readonly float X_SPACING = 300f; // ระยะห่างรวมระหว่าง Node
    private readonly float Y_SPACING = 200f; // ระยะห่างรวมระหว่างชั้น

    private Dictionary<Skill, SkillNodeUI> skillUIMap = new Dictionary<Skill, SkillNodeUI>();
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        if (skillBook == null || contentSkill == null)
        {
            Debug.LogError("SkillBook หรือ ContentSkill reference is missing!");
            return;
        }

        StartCoroutine(DelayShowTree());

    
    }
    IEnumerator DelayShowTree() {
        yield return new WaitForSeconds(0.1f);
        // รีเซ็ตขอบเขตเริ่มต้น
        minX = 0f;
        maxX = 0f;
        minY = 0f;
        maxY = 0f;
        // เริ่มสร้าง UI Nodes ทั้งหมดจาก Skill Tree (ที่ตำแหน่งเริ่มต้น 0, 0)
        CreateAllSkillNodes(skillBook.attackSkillTree.rootSkill, Vector2.zero);

        // **คำนวณและกำหนดขนาด Content ของ Scroll View**
        CalculateAndSetContentSize();

        // อัปเดต UI ครั้งแรก
        RefreshAllUI();
    }

    private void CalculateAndSetContentSize()
    {
        if (skillUIMap.Count == 0) return;

        // คำนวณความกว้าง: จากซ้ายสุดถึงขวาสุด (บวกขอบเล็กน้อย)
        float contentWidth = (maxX - minX) + NODE_WIDTH + 50f; // +50f คือ Margin

        // คำนวณความสูง: จากจุดสูงสุด (0) ถึงจุดต่ำสุด (minY) (บวกขอบเล็กน้อย)
        // เนื่องจากค่า minY จะเป็นค่าลบ เราใช้ค่าสัมบูรณ์
        float contentHeight = Mathf.Abs(minY) + NODE_HEIGHT + 50f; // +50f คือ Margin

        // กำหนดขนาดให้กับ RectTransform ของ Content
        contentSkill.sizeDelta = new Vector2(contentWidth, contentHeight);

        // หมายเหตุ: สำหรับ Scroll View แนวตั้งที่ Node ถูกวางจากบนลงล่าง (Y เป็นลบ) 
        // ควรตั้งค่า Anchor/Pivot ของ contentSkill เป็น Top-Left (0, 1) 
        // เพื่อให้การคำนวณความสูงทำงานได้อย่างถูกต้อง
    }

    /// <summary>
    /// วนซ้ำเพื่อสร้าง Skill Node UI ตามลำดับชั้น
    /// </summary>
    private void CreateAllSkillNodes(Skill currentSkill, Vector2 position)
    {
        if (skillUIMap.ContainsKey(currentSkill)) return;

        // 1. สร้าง Node UI
        SkillNodeUI newNode = Instantiate(skillNodePrefab, skillNodeContainer);
        newNode.Initialize(currentSkill);
        skillUIMap.Add(currentSkill, newNode);

        // กำหนดตำแหน่ง
        RectTransform rt = newNode.GetComponent<RectTransform>();
        rt.localPosition = position;

        // 2. ติดตามขอบเขตของ Node ที่ถูกสร้างขึ้น
        float nodeHalfWidth = NODE_WIDTH / 2f;
        float nodeHalfHeight = NODE_HEIGHT / 2f;

        minX = Mathf.Min(minX, position.x - nodeHalfWidth);
        maxX = Mathf.Max(maxX, position.x + nodeHalfWidth);
        // เนื่องจาก Y เริ่มจาก 0 และลดลง (เป็นลบ)
        minY = Mathf.Min(minY, position.y - nodeHalfHeight);
        maxY = Mathf.Max(maxY, position.y + nodeHalfHeight);


        // 3. สร้าง Node สำหรับ Skill ถัดไปในลำดับชั้น (ลูก)

        int numChildren = currentSkill.nextSkills.Count;

        // คำนวณตำแหน่งเริ่มต้นของลูกคนแรก เพื่อให้ Node ทั้งหมดอยู่กึ่งกลาง
        float totalWidth = (numChildren - 1) * X_SPACING;
        float startX = position.x - (totalWidth / 2f);

        for (int i = 0; i < numChildren; i++)
        {
            Skill nextSkill = currentSkill.nextSkills[i];

            // ตำแหน่งลูกถัดไปจะเพิ่มจาก startX ไปเรื่อยๆ
            Vector2 nextPos = new Vector2(
                startX + (i * X_SPACING),
                position.y - Y_SPACING // ลงไปหนึ่งชั้น
            );

            CreateAllSkillNodes(nextSkill, nextPos);
        }
    }

    public void RefreshAllUI()
    {
        foreach (var uiNode in skillUIMap.Values)
        {
            uiNode.UpdateUI();
        }
    }

    public void CloseUI() { 
        gameObject.SetActive(false);
    }

    // อาจเพิ่ม Logic สำหรับการวาดเส้นเชื่อมต่อ (Lines) ระหว่าง Node ได้ที่นี่
    // ซึ่งต้องใช้ Component เช่น UILineRenderer หรือ UI.Graphic ที่กำหนดเอง
}