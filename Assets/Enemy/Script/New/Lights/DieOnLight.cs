using UnityEngine;

public class DieOnLight : LightReaction
{
    [SerializeField] private EnemyController enemy;
    [SerializeField] private float lightDuration = 0f;
    private float lightHitTimer = 0f;
    protected override void React(EnemyLightSource sources, float dt)
    {
        if (sources != EnemyLightSource.None)
        {
            lightHitTimer += Time.deltaTime;
        }
        else
        {
            lightHitTimer = 0f;
        }

        if (lightHitTimer >= lightDuration)
            enemy.Die();
    }
}