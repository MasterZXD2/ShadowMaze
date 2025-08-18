using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ReflectionPathPreview : MonoBehaviour
{
    [Header("Source & Physics")]
    public Transform source;              // จุดกำเนิดลำแสง (ปลายตะเกียง)
    public float maxDistance = 50f;
    public int maxBounces = 5;
    public LayerMask reflectMask;         // ใส่ Mirror | Painting | Obstacle

    [Header("Line Look")]
    public float startWidth = 0.04f;
    public float endWidth = 0.02f;
    public float uvScrollSpeed = 2f;      // ความเร็วไหลของลาย
    public Color lineColor = Color.white;

    [Header("Toggle")]
    public bool previewOnlyWhenActive = true;

    private LineRenderer lr;
    private Material lrMat;
    private Vector2 uvOffset;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.useWorldSpace = true;
        lr.startWidth = startWidth;
        lr.endWidth   = endWidth;
        lr.numCapVertices = 8;
        lr.numCornerVertices = 8;

        // เตรียม Material (ควรเป็น Unlit/Particles Additive ที่มี _MainTex)
        lrMat = lr.material;
        lr.startColor = lr.endColor = lineColor;

        // เริ่มปิดไว้ก็ได้ ให้เปิดตอนผู้เล่นวางตะเกียง
        if (previewOnlyWhenActive) enabled = false;
    }

    void Update()
    {
        if (!source) return;

        // 1) คำนวณเส้นทางสะท้อน
        Vector3 pos = source.position;
        Vector3 dir = source.forward;   // ให้ pivot หันแกน Z ไปทิศยิง

        List<Vector3> points = new List<Vector3> { pos };
        int bounces = 0;

        while (bounces <= maxBounces)
        {
            if (Physics.Raycast(pos, dir, out RaycastHit hit, maxDistance, reflectMask, QueryTriggerInteraction.Ignore))
            {
                points.Add(hit.point);

                int layer = hit.collider.gameObject.layer;

                // ถ้าเป็นกระจก → สะท้อนต่อ
                if (layer == LayerMask.NameToLayer("Mirror"))
                {
                    dir = Vector3.Reflect(dir, hit.normal).normalized;
                    pos = hit.point + dir * 0.001f; // ดันออกจากผิวกันยิงซ้ำ
                    bounces++;
                }
                else
                {
                    // ชนภาพ/ผนัง/เป้าหมาย → จบเส้น
                    break;
                }
            }
            else
            {
                // ไม่มีอะไรให้ชน → วาดไปสุดระยะ
                points.Add(pos + dir * maxDistance);
                break;
            }
        }

        // 2) อัปเดต LineRenderer
        lr.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
            lr.SetPosition(i, points[i]);

        // 3) เลื่อน UV ให้ “ลายวิ่งไปข้างหน้า” = บอกทิศ
        if (lrMat != null && lrMat.HasProperty("_MainTex"))
        {
            uvOffset.x += uvScrollSpeed * Time.deltaTime;
            lrMat.SetTextureOffset("_MainTex", uvOffset);
        }
    }

    // เผื่ออยากเปิด/ปิดตามสถานการณ์ (เช่น วางตะเกียงแล้วค่อยโชว์)
    public void SetPreviewActive(bool active)
    {
        enabled = active;
        lr.enabled = active;
        if (!active)
        {
            lr.positionCount = 0;
            uvOffset = Vector2.zero;
            if (lrMat != null && lrMat.HasProperty("_MainTex"))
                lrMat.SetTextureOffset("_MainTex", uvOffset);
        }
    }

    // ให้เห็นทางใน Scene View ชัดขึ้นเวลา debug
    void OnDrawGizmosSelected()
    {
        if (lr == null || lr.positionCount < 2) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < lr.positionCount - 1; i++)
            Gizmos.DrawLine(lr.GetPosition(i), lr.GetPosition(i + 1));
    }
}
