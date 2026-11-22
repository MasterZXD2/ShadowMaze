using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private EnemyMotor motor;

    [Space(10)]
    [Header("Stats")]
    public float Health = 100f;
    public float MaxHealth = 100f;

    [Space(10)]
    [Header("Movement")]
    public float Gravity = 10f;
    public float TerminalVelo = 10f;
    public float JumpSpeed = 8f;
    public float InAirAcceleration = 40f;
    public float InAirDrag = 1f;
    public float WalkSpeed = 4f;

    private void Start()
    {
        MaxHealth = Health;
        WalkSpeed = motor.Agent.speed * 1.1f;
    }
}