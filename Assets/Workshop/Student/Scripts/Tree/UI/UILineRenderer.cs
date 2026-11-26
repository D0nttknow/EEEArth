using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(CanvasRenderer))]
public class UILineRenderer : MaskableGraphic
{
    public Transform[] points;

    public float thickness = 10f;
    public bool center = true;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points == null || points.Length < 1)
            return;

        // **จุดเริ่มต้นใหม่:** ใช้ตำแหน่งของ GameObject ที่สคริปต์นี้แนบอยู่
        Vector3 startPoint = transform.position;

        // **สร้าง Vertex Template สำหรับ Beveled Edges (ถ้ามี)**
        // เนื่องจากโครงสร้างการวาดเปลี่ยนไป (จากจุดเดียวไปยังหลายจุด)
        // Logic การสร้าง Beveled Edges แบบเดิมอาจต้องปรับปรุง
        // ในโค้ดใหม่นี้ จะวาดเส้นจาก startPoint ไปยังจุดแรกใน points[0]
        // จากนั้นวาดเส้นระหว่าง points[i] กับ points[i+1] (ถ้ามี) 

        // *******************************************************************
        // ********* 1. สร้าง Segment แรก: จาก transform.position ไปยัง points[0] *********
        // *******************************************************************
        CreateLineSegment(startPoint, points[0].position, vh);

        int index = 0;

        // Add the line segment to the triangles array (Segment 0)
        vh.AddTriangle(index, index + 1, index + 3);
        vh.AddTriangle(index + 3, index + 2, index);


        // *******************************************************************
        // ********* 2. สร้าง Segment ถัดไป: จาก points[i] ไปยัง points[i+1] *********
        // *******************************************************************
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 p1 = points[i].position;
            Vector3 p2 = points[i + 1].position;

            // สร้าง segment ระหว่าง points[i] และ points[i+1]
            CreateLineSegment(p1, p2, vh);

            // คำนวณ Index สำหรับ Segment ใหม่ (เริ่มต้นที่ Segment 1)
            // Index สำหรับ Segment 1 จะเริ่มต้นที่ vh.currentVertCount ก่อนเรียก CreateLineSegment
            // เนื่องจาก vh.currentVertCount จะเท่ากับ 5 หลัง Segment แรกถูกสร้าง

            index = (i + 1) * 5; // Index สำหรับ Segment ที่ i+1 (เริ่มต้นที่ 5, 10, 15, ...)

            // Add the line segment to the triangles array
            vh.AddTriangle(index, index + 1, index + 3);
            vh.AddTriangle(index + 3, index + 2, index);

            // These two triangles create the beveled edges
            // โค้ดเดิมสำหรับ Beveled Edges ยังคงใช้ได้เพราะมันใช้ index ของ Segment ที่แล้ว (index - 5)
            // สำหรับ Segment แรก (i=0) จะเชื่อมต่อ Segment 0 กับ Segment 1
            if (i >= 0) // i = 0 คือ Segment ที่ 1 (เชื่อม Segment 0)
            {
                vh.AddTriangle(index, index - 1, index - 3);
                vh.AddTriangle(index + 1, index - 1, index - 2);
            }
        }
    }

    /// <summary>
    /// Creates a rect from two points that acts as a line segment
    /// </summary>
    /// <param name="point1">The starting point of the segment</param>
    /// <param name="point2">The endint point of the segment</param>
    /// <param name="vh">The vertex helper that the segment is added to</param>
    private void CreateLineSegment(Vector3 point1, Vector3 point2, VertexHelper vh)
    {
        // การทำงานภายในยังคงเหมือนเดิม เพราะจุดประสงค์คือการสร้าง segment จาก 2 จุดที่ให้มา
        Vector3 offset = center ? (rectTransform.sizeDelta / 2) : Vector2.zero;

        // Create vertex template
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        // Create the start of the segment
        Quaternion point1Rotation = Quaternion.Euler(0, 0, RotatePointTowards(point1, point2) + 90);
        vertex.position = point1Rotation * new Vector3(-thickness / 2, 0);
        vertex.position += point1 - offset;
        vh.AddVert(vertex);
        vertex.position = point1Rotation * new Vector3(thickness / 2, 0);
        vertex.position += point1 - offset;
        vh.AddVert(vertex);

        // Create the end of the segment
        Quaternion point2Rotation = Quaternion.Euler(0, 0, RotatePointTowards(point2, point1) - 90);
        vertex.position = point2Rotation * new Vector3(-thickness / 2, 0);
        vertex.position += point2 - offset;
        vh.AddVert(vertex);
        vertex.position = point2Rotation * new Vector3(thickness / 2, 0);
        vertex.position += point2 - offset;
        vh.AddVert(vertex);

        // Also add the end point
        vertex.position = point2 - offset;
        vh.AddVert(vertex);
    }

    /// <summary>
    /// Gets the angle that a vertex needs to rotate to face target vertex
    /// </summary>
    /// <param name="vertex">The vertex being rotated</param>
    /// <param name="target">The vertex to rotate towards</param>
    /// <returns>The angle required to rotate vertex towards target</returns>
    private float RotatePointTowards(Vector2 vertex, Vector2 target)
    {
        return (float)(Mathf.Atan2(target.y - vertex.y, target.x - vertex.x) * (180 / Mathf.PI));
    }
}