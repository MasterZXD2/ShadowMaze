using UnityEngine;
using ShadowMaze.MyCharacterController;

public class EnemyTest : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float detectionRadius = 5f;
    public float attackDistance = 1.2f;
    public float damage = 5f;
    public float attackCooldown = 1f;
    public GameObject normalModel;
    public GameObject boxModel;

    private Transform player;
    private PlayerStats playerStats;
    private float lastAttackTime;

    private bool isFrozen = false;
    private float freezeTimer = 0f;
    private float freezeDuration = 5f;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerStats = playerObj.GetComponent<PlayerStats>();
        }

        if (!normalModel || !boxModel)
            Debug.LogError("ต้องใส่ normalModel และ boxModel ใน Inspector");

        boxModel.SetActive(false);
        normalModel.SetActive(true);
    }

    private void Update()
    {
        if (player == null || playerStats == null || playerStats.isDie) return;

        bool isCurrentlyLit = IsHitByFlashlight();

        if (isFrozen)
        {
            if (isCurrentlyLit)
            {
                freezeTimer = freezeDuration;
            }
            else
            {
                freezeTimer -= Time.deltaTime;

                if (freezeTimer <= 0f)
                {
                    Unfreeze();
                }
            }

            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRadius)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            transform.LookAt(player);

            if (distance <= attackDistance && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }


        if (isCurrentlyLit)
        {
            Freeze();
        }
    }

    private void Attack()
    {
        if (playerStats != null && !playerStats.isDie)
        {
            playerStats.Health -= damage;

            if (playerStats.Health <= 0)
            {
                playerStats.die();
                Debug.Log("Player Died");
            }
        }
    }

    private void Freeze()
    {
        if (isFrozen) return;

        isFrozen = true;
        freezeTimer = freezeDuration;

        normalModel.SetActive(false);
        boxModel.SetActive(true);
    }

    private void Unfreeze()
    {
        isFrozen = false;

        boxModel.SetActive(false);
        normalModel.SetActive(true);
    }

    private bool IsHitByFlashlight()
    {
        PlayerLight playerLight = FindObjectOfType<PlayerLight>();
        if (playerLight == null) return false;

        var detected = playerLight.GetObjectsInFlashlightCone();
        if (detected == null) return false;

        return detected.Contains(this.gameObject);
    }
}
