using UnityEngine;

public class EnemyPresentation : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private GameObject enemyModel;
    [SerializeField] private float rotationAngleLimit = 120f;
    [SerializeField] private float activeRotationAngleLimit = 90f;
    [SerializeField] private float rotationSpeedDuringSearch = 100f;
    [SerializeField] private float activeRotationSpeedDuringSearch = 200f;
    [SerializeField] private float EnemyModelRotationSpeed = 10f;
    private Vector3 modelOffset;

    private float currRotationAngleLimit = 60f;
    private float currRotationSpeedDuringSearch = 60f;
    private float currentRotationOffset = 0f; // หมุนสะสม
    private int rotationDirection = 1; // 1 = ขวา, -1 = ซ้าย

    private void Start()
    {
        if (enemyModel) modelOffset = enemyModel.transform.position - transform.position;
    }

    private void Update()
    {
        if (enemyModel)
        {
            enemyModel.transform.position = transform.position + modelOffset;
            enemyModel.transform.rotation = transform.rotation;
        }
    }

    public Collider GetModelCollider() { return enemyModel.GetComponent<Collider>(); }

    public void FollowRotate(Vector3 dir)
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            EnemyModelRotationSpeed * Time.deltaTime
        );
    }

    public void SearchRotate()
    {
        float rotateAmount = currRotationSpeedDuringSearch * Time.deltaTime * rotationDirection;
        currentRotationOffset += rotateAmount;

        // เช็คว่าหมุนเกิน limit แล้วให้กลับทิศ
        if (Mathf.Abs(currentRotationOffset) >= currRotationAngleLimit)
        {
            rotationDirection *= -1; // กลับทิศหมุน
            currentRotationOffset = Mathf.Clamp(currentRotationOffset, -currRotationAngleLimit, currRotationAngleLimit);
        }

        // หมุนตามแนวตั้ง (yaw)
        transform.Rotate(Vector3.up, rotateAmount);
    }

    public void StartIdleLookAround()
    {
        currRotationSpeedDuringSearch = rotationSpeedDuringSearch;
        currRotationAngleLimit = rotationAngleLimit;

        currentRotationOffset = 0f;
        rotationDirection = 1;
    }

    public void StartActiveSearch()
    {
        currRotationSpeedDuringSearch = activeRotationSpeedDuringSearch;
        currRotationAngleLimit = activeRotationAngleLimit;
        currentRotationOffset = 0f;
        rotationDirection = 1;
    }
}
