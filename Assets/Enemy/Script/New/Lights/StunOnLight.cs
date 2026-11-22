using UnityEngine;

public class StunOnLight : LightReaction
{
    protected override void React(EnemyLightSource sources, float dt)
    {
        stun = sources != EnemyLightSource.None;
    }
}