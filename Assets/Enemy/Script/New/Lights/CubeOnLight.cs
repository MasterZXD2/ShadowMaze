using UnityEngine;

public class CudeOnLight : LightReaction
{
    [SerializeField]
    private EnemyLightSource acceptedSources =
        EnemyLightSource.Flashlight | EnemyLightSource.Lamp;

    [SerializeField, Min(0f)] private float transformDelay = 3f;
    [SerializeField] private CubeTransformation transformation;

    private float exposedTime;

    protected override void React(EnemyLightSource sources, float dt)
    {

    }
}