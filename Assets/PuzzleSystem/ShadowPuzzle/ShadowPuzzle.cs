using ShadowMaze.MyCharacterController;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShadowPuzzle : Puzzle
{
    struct box {
        public bool b { get; }
        public Collider[] colliders { get; }

        public box(bool v, Collider[] c) : this()
        {
            b = v;
            colliders = c;
        }
    }

    [field: SerializeField] private List<BoxCollider> _Shapes;

    [SerializeField] private LayerMask _puzzleLayerMask;

    private List<BoxCollider> colliderInLights = new List<BoxCollider>();
    private List<ShadowBlockObject> shadowBlockObjects = new List<ShadowBlockObject>();

    [SerializeField] private Transform standPosition;

    private void Update()
    {
        CheckPuzzleComplete();
        foreach (BoxCollider c in colliderInLights)
        {
            if (c.gameObject.TryGetComponent<ShadowBlockObject>(out ShadowBlockObject sb))
            {
                if (sb.SP != this) { 
                    sb.SP = this;
                    shadowBlockObjects.Add(sb);
                }
            }
        }
        float dist = (standPosition.position - PlayerController.Instance.transform.position).sqrMagnitude;
        if (dist < .5f)
        {
            bool f = true;
            foreach (ShadowBlockObject s in shadowBlockObjects)
            {
                if (!s.getB())
                {
                    f = false;
                    break;
                }
            }

            if (shadowBlockObjects.Count != 0) isFinish = f;
            if (PlayerController.Instance._playerKeyBoardInput.p4sh) isFinish = true;
        }

    }

    private void OnDrawGizmos()
    {
        if (_Shapes == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(standPosition.position, .5f);

        Gizmos.color = Color.green;
        foreach (var shape in _Shapes)
        {
            if (shape == null) continue;
            Gizmos.matrix = shape.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(shape.center, shape.size);
        }
    }

    public bool CheckPuzzleComplete()
    {
        List<Collider> c = new List<Collider>();

        foreach (var shape in _Shapes)
        {
            Debug.Log(shape.name);
            box b = CheckShapeMatch(shape);
            if (!b.b)
            {
                foreach (BoxCollider l in colliderInLights)
                {
                    if (l.gameObject.TryGetComponent<ShadowBlockObject>(out ShadowBlockObject sb))
                    {
                        if (sb.SP == this) sb.SP = null;
                    }
                }
                colliderInLights.Clear();
                return false;
            }

            c.AddRange(b.colliders);

        }

        colliderInLights = c
            .OfType<BoxCollider>()   // กรองเฉพาะ BoxCollider (ถ้าต้องการ)
            .Distinct()
            .ToList();
        return true;
    }

    private box CheckShapeMatch(BoxCollider shape)
    {
        // คำนวณขนาด box collider (world space)
        Bounds b = shape.bounds;
        Vector3 halfExtents = b.extents - Vector3.one * 0.05f;

        var expected3D = new HashSet<Vector3Int>();
        var actual3D = new HashSet<Vector3Int>();

        // เติม expected grid positions (แบบ voxel) ขนาด 1x1x1
        for (int x = Mathf.FloorToInt(b.min.x); x < Mathf.CeilToInt(b.max.x); x++)
        {
            for (int y = Mathf.FloorToInt(b.min.y); y < Mathf.CeilToInt(b.max.y); y++)
            {
                for (int z = Mathf.FloorToInt(b.min.z); z < Mathf.CeilToInt(b.max.z); z++)
                {
                    Vector3 worldPos = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
                    if (b.Contains(worldPos))
                    {
                        expected3D.Add(new Vector3Int(x, y, z) * 100);
                    }
                }
            }
        }

        Collider[] _collidersInPuzzle = Physics.OverlapBox(
             b.center,
             halfExtents, 
             shape.transform.rotation,
             _puzzleLayerMask
             );
        foreach (var col in _collidersInPuzzle)
        {
            if (col == shape) continue;
            actual3D.Add(Vector3Int.FloorToInt(col.transform.position * 100f));
        }

        return new box(expected3D.SetEquals(actual3D), _collidersInPuzzle);
    }
}
