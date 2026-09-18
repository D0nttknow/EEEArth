using UnityEngine;
using UnityEngine.InputSystem;

public class BusController : MonoBehaviour
{
    [Header("Lane Settings")]
    [Tooltip("ระยะห่างระหว่างแต่ละเลน (หน่วยเป็นเมตร)")]
    public float laneDistance = 3.0f;
    [Tooltip("ความเร็วในการเลื่อนเปลี่ยนเลน")]
    public float laneChangeSpeed = 10.0f;

    // 0 = เลนซ้ายสุด, 1 = เลนกลาง, 2 = เลนขวาสุด
    private int currentLane = 1;

    [Header("Speed Settings")]
    public float normalSpeed = 15.0f;     // ความเร็วปกติเมื่อวิ่ง Endless
    public float decelerationRate = 10.0f; // ความเร็วในการเบรกชะลอรถ (ยิ่งมากยิ่งหยุดไว)
    public float accelerationRate = 5.0f;  // ความเร็วในการเร่งกลับไปความเร็วปกติ

    private float currentSpeed;
    private bool isBraking = false;

    void Start()
    {
        // เริ่มต้นให้รถวิ่งด้วยความเร็วปกติ
        currentSpeed = normalSpeed;
    }

    void Update()
    {
        // 1. ระบบเบรก / ชะลอความเร็ว (กด S)
        if (isBraking)
        {
            // ค่อยๆ ลดความเร็วลงเรื่อยๆ จนถึง 0
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, decelerationRate * Time.deltaTime);
        }
        else
        {
            // ค่อยๆ เร่งความเร็วกลับมาวิ่งปกติ
            currentSpeed = Mathf.MoveTowards(currentSpeed, normalSpeed, accelerationRate * Time.deltaTime);
        }

        // 2. เคลื่อนที่รถไปข้างหน้าตามความเร็วปัจจุบัน
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        // 3. คำนวณตำแหน่งเลน (เลนกลาง = 0, เลนซ้าย = -laneDistance, เลนขวา = +laneDistance)
        float targetX = (currentLane - 1) * laneDistance;
        Vector3 targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);

        // เลื่อนตำแหน่งรถไปยังเลนเป้าหมายอย่างนุ่มนวล
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * laneChangeSpeed);
    }

    // --- รับ Input จาก New Input System ---

    // Action: Move (1D Axis: A / D)
    public void OnMove(InputAction.CallbackContext context)
    {
        // ทำงานเฉพาะตอนที่กดปุ่มลงไป (performed)
        if (context.performed)
        {
            float value = context.ReadValue<float>();

            // กด A (ค่าเป็นลบ) -> ไปเลนซ้าย
            if (value < 0 && currentLane > 0)
            {
                currentLane--;
            }
            // กด D (ค่าเป็นบวก) -> ไปเลนขวา
            else if (value > 0 && currentLane < 2)
            {
                currentLane++;
            }
        }
    }

    // Action: Stop (Button: S)
    public void OnStop(InputAction.CallbackContext context)
    {
        // เมื่อเริ่มกดปุ่ม S ค้างไว้
        if (context.started)
        {
            isBraking = true;
        }
        // เมื่อปล่อยปุ่ม S
        else if (context.canceled)
        {
            isBraking = false;
        }
    }

    // --- ระบบชนสิ่งกีดขวาง และ จอดรับผู้โดยสาร ---
    private void OnTriggerEnter(Collider other)
    {
        // ระบบชนสิ่งกีดขวาง = Game Over
        if (other.CompareTag("Obstacle"))
        {
            Debug.Log("💥 Game Over! ชนสิ่งกีดขวาง");
            currentSpeed = 0;
            this.enabled = false; // หยุดการทำงานของ Script
        }
        // ระบบจอดป้ายรับผู้โดยสาร
        else if (other.CompareTag("BusStop"))
        {
            // เช็กว่ารถหยุดนิ่งพอหรือยัง (ความเร็วน้อยกว่า 0.5)
            if (currentSpeed <= 0.5f)
            {
                Debug.Log("🚏 จอดรับผู้โดยสารเรียบร้อย!");
            }
            else
            {
                Debug.Log("⚠️ รถยังไม่หยุด! วิ่งผ่านป้ายไปแล้ว");
            }
        }
    }
}