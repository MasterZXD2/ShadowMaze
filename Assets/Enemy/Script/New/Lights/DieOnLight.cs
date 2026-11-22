using UnityEngine;

public class DieOnLight : LightReaction
{
    [SerializeField] private EnemyController enemy;
    [SerializeField] private float dieByLightDuration = 0f;
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

        if (lightHitTimer >= dieByLightDuration)
            enemy.Die();
    }
}