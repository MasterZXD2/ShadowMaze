using System.Collections.Generic;
using UnityEngine;

public class ShadowPuzzle : Puzzle
{
    [field: SerializeField] private List<BoxCollider> _Shapes;
    private List<Collider> _collidersInPuzzle = new List<Collider>();

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("ShadowPuzzle"))
        {
            _collidersInPuzzle.Add(other);
            CheckPuzzleComplete();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("ShadowPuzzle")) {
            _collidersInPuzzle.Remove(other);
            CheckPuzzleComplete();
        }
    }
    private void OnDrawGizmos()
    {
        if (_Shapes == null) return;

        Gizmos.color = Color.green;
        foreach (var shape in _Shapes)
        {
            if (shape == null) continue;
            Gizmos.matrix = shape.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(shape.center, shape.size);
        }

        Gizmos.color = Color.yellow;
        if (_collidersInPuzzle != null)
        {
            foreach (var col in _collidersInPuzzle)
            {
                Gizmos.DrawCube(Vector3Int.RoundToInt(col.transform.position), Vector3.one * 0.9f);
            }
        }
    }

    public bool CheckPuzzleComplete()
    {
        foreach (var shape in _Shapes)
        {
            if (CheckShapeMatch(shape)) return true;
        }

        return false;
    }


    private bool CheckShapeMatch(BoxCollider shape)
    {
        HashSet<Vector3Int> expectedPositions = new HashSet<Vector3Int>();
        HashSet<Vector3Int> actualBoxPositions = new HashSet<Vector3Int>();

        // คำนวณขนาด box collider (world space)
        Vector3 size = shape.size;
        Vector3 center = shape.center;

        Vector3 worldCenter = shape.transform.TransformPoint(center);
        Vector3 worldSize = Vector3.Scale(size, shape.transform.lossyScale);

        // กำหนด bounding box เริ่มที่ world space (min/max)
        Vector3 min = worldCenter - worldSize / 2f;
        Vector3 max = worldCenter + worldSize / 2f;

        // เติม expected grid positions (แบบ voxel) ขนาด 1x1x1
        for (int x = Mathf.FloorToInt(min.x); x < Mathf.CeilToInt(max.x); x++)
        {
            for (int y = Mathf.FloorToInt(min.y); y < Mathf.CeilToInt(max.y); y++)
            {
                for (int z = Mathf.FloorToInt(min.z); z < Mathf.CeilToInt(max.z); z++)
                {
                    Vector3Int gridPos = new Vector3Int(x, y, z);

                    // กล่องต้องอยู่ภายใน AABB จริงๆ
                    Vector3 worldPos = (Vector3)gridPos + Vector3.one * 0.5f; // center
                    if (PointInsideBox(worldPos, worldCenter, worldSize))
                    {
                        expectedPositions.Add(gridPos);
                    }
                }
            }
        }

        // เก็บตำแหน่งจริงของกล่องทั้งหมดใน Grid
        foreach (var box in _collidersInPuzzle)
        {
            Vector3Int gridPos = Vector3Int.RoundToInt(box.transform.position);
            actualBoxPositions.Add(gridPos);
        }

        // ตรวจว่าครบทุกตำแหน่งที่ shape ต้องการ
        return expectedPositions.SetEquals(actualBoxPositions);
    }

    private bool PointInsideBox(Vector3 point, Vector3 center, Vector3 size)
    {
        Vector3 halfSize = size / 2f;
        return Mathf.Abs(point.x - center.x) <= halfSize.x &&
               Mathf.Abs(point.y - center.y) <= halfSize.y &&
               Mathf.Abs(point.z - center.z) <= halfSize.z;
    }
}
