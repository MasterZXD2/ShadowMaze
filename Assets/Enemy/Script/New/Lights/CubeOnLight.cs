using UnityEngine;

public class CubeOnLight : LightReaction
{
    [SerializeField]
    private EnemyLightSource acceptedSources =
        EnemyLightSource.Flashlight | EnemyLightSource.Lamp;
    [SerializeField] private TransitionEnemyBrain brain;
    [SerializeField] private float lightDuration = 3f;
    [SerializeField] private float cubeFormDuration = 5f;  // ระยะเวลาที่อยู่ในร่างกล่องก่อนกลับเป็นศัตรู (ถ้าไม่โดนไฟฉายส่อง)
    private bool inMemeoryFragmentArea = false;
    private float lightHitTimer = 0f;
    private float cubeFromTimer = 0f;

    protected override void React(EnemyLightSource sources, float dt)
    {
        if (brain.enemyState.CurrEnemyActionState == ShadowMaze.EnemyCharactor.EnemyActionState.StunnedAsCube)
        {
            if ((sources & acceptedSources) != 0)
            {
                cubeFromTimer = 0f;
            } else if (!inMemeoryFragmentArea) {
                cubeFromTimer += dt;
                if (cubeFromTimer >= cubeFormDuration)
                {
                    brain.TransitionToEnemyForm();
                    cubeFromTimer = 0f;
                }
            }
            return;
        }

        if ((sources & acceptedSources) != 0)
        {
            lightHitTimer += Time.deltaTime;
        }
        else
        {
            lightHitTimer = 0f;
        }

        if (lightHitTimer >= lightDuration)
        {
            brain.TransitionToCubeForm();
            lightHitTimer = 0f;
        }
    }

    public void InArea(string name)
    {
        if (name == "MFA") 
            inMemeoryFragmentArea = true;
    }

    public void OutArea()
    {
        inMemeoryFragmentArea = false;
    }
}