using ShadowMaze.InteractSystem;
using UnityEngine;

public class ShadowBlockObject : BookPuzzleObject
{
    public ShadowPuzzle SP;
    [SerializeField] private BoxCollider collider;

    protected override void Start()
    {
        base.Start();
        collider = GetComponent<BoxCollider>();
    }

    protected override void LateUpdate()
    {
        
    }

    public override void HitByFlashlight()
    {
        if (SP) b = true;
        else b = false;
    }
}
