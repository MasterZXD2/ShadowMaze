using UnityEngine;

public class PuzzleEnemyBrain : EnemyBrain
{
    private void Start()
    {
        motor.ChangeMotor(Motor.CharacterController);
    }


}
