using ShadowMaze.EnemyCharactor;
using System;
using UnityEngine;

[Flags]
public enum EnemyLightSource { None = 0, Flashlight = 1, Lamp = 2 }

public abstract class LightReaction : MonoBehaviour
{
    private EnemyLightSource hits;
    public bool stun { get; protected set; }

    public void ReportHit(EnemyLightSource source) => hits |= source;

    public static LightReaction FindReceiver(GameObject target)
    {
        var cube = target.GetComponentInParent<EnemyCubeOwner>();
        if (cube) return cube.Owner;
        return target.GetComponentInParent<LightReaction>();
    }

    protected void LateUpdate()
    {
        React(hits, Time.deltaTime);
        hits = EnemyLightSource.None;
    }

    protected abstract void React(EnemyLightSource sources, float dt);
}
