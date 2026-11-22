using NUnit.Framework;
using ShadowMaze.EnemyCharactor;
using UnityEngine;
using UnityEngine.Rendering;

public class CubeTransformation : MonoBehaviour
{
    [SerializeField] private EnemyCombat combat;
    public bool isInCudefrom { get; private set; } = false;

    private void Start()
    {
        combat.attackSucceeded.AddListener(() =>
        {
            disappear();
            Invoke(nameof(DelayedRespawn), 3f);
        });
    }

    #region Transition Functions
    protected void TransitionToCubeForm()
    {
        if (enemyState.CurrEnemyActionState == EnemyActionState.StunnedAsCube)
            return;
        disappear();
        enemyState.ActionSet(EnemyActionState.StunnedAsCube); // เปลี่ยนสถานะเป็น StunnedAsCube

        if (cubeFormPrefab != null)
        {
            // สร้างกล่องที่ตำแหน่งและ Rotation ปัจจุบันของ EnemyAI (แคปซูล)
            currentCubeFormInstance = Instantiate(cubeFormPrefab, transform.position, new Quaternion(0, 0, 0, 0));
            //Vector3 s = currentCubeFormInstance.transform.localScale;
            //for (int i = 0; i < 3; i++) s[i] *= sceOffset[i];
            //currentCubeFormInstance.transform.localScale = s;
            currentCubeFormInstance.tag = gameObject.tag;
            currentCubeFormInstance.layer = gameObject.layer;
            // *** ลบบรรทัดนี้: ไม่ให้ EnemyAI เป็น Parent ของกล่องอีกต่อไป ***
            // currentCubeFormInstance.transform.SetParent(this.transform); 
            // แต่จะให้ MoveableObject เป็นตัวจัดการ Parent ตัวเองกับ Player แทน
        }
    }

    private void TransitionToEnemyForm()
    {
        Vector3 spawnPosition;
        float colliderHeight = enemyController ? enemyController.height : 0.5f;
        RaycastHit hit;
        Vector3 raycastOrigin = currentCubeFormInstance ? currentCubeFormInstance.transform.position + Vector3.up * colliderHeight : transform.position + Vector3.up * colliderHeight;
        if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, colliderHeight, _groundLayers))
        {
            spawnPosition = hit.point + Vector3.up * colliderHeight;
        }
        else
        {
            spawnPosition = currentCubeFormInstance.transform.position + Vector3.up * colliderHeight;
        }

        if (currentCubeFormInstance)
        {
            MoveableObject moveableObj = currentCubeFormInstance.GetComponent<MoveableObject>();
            if (moveableObj)
            {
                if (moveableObj.IsPicked()) moveableObj.Interact();
            }
        }

        if (currentCubeFormInstance)
        {
            Destroy(currentCubeFormInstance);
            currentCubeFormInstance = null;
        }

        respawn(spawnPosition);
    }
    #endregion

    #region Control Functions

    private void disappear()
    {
        isInCudefrom = true;
        agent.SetDestination(transform.position);

        _positionSet = false;
        isMovement = false;
        isAttack = false;
        isLPP = false;
        isCanSeePlayer = false;

        astop = true;

        if (enemyModel) GetComponent<Collider>().isTrigger = true;
        if (enemyMeshRenderers.Count != 0)
        {
            foreach (Renderer r in enemyMeshRenderers)
            {
                r.enabled = false;
            }
        }
        if (particleSystemRenderers.Count != 0)
        {
            foreach (ParticleSystem p in particleSystemRenderers)
            {
                p.Stop();
            }
        }
        if (enemyController) enemyController.enabled = false;
        if (agent) agent.enabled = false;
    }
    private void DelayedRespawn() => StartCoroutine(respawn(spawnPoint));

    private IEnumerator respawn(Vector3 point)
    {
        yield return new WaitUntil(() => !EnemyManager.Instance._b);
        isInCudefrom = false;
        transform.position = spawnPoint;
        agent.Warp(transform.position);
        agent.SetDestination(transform.position);

        enemyState.ActionSet(EnemyActionState.Idle);
        flashlightHitTimer = 0f;
        cubeFormTimer = 0f;

        isCanSeePlayer = true;
        astop = false;
        isAttack = false;

        if (enemyModel) GetComponent<Collider>().isTrigger = false;
        if (enemyMeshRenderers.Count != 0)
        {
            foreach (Renderer r in enemyMeshRenderers)
            {
                r.enabled = true;
            }
        }
        if (particleSystemRenderers.Count != 0)
        {
            foreach (ParticleSystem p in particleSystemRenderers)
            {
                p.Play();
            }
        }
        if (enemyController) enemyController.enabled = true;
        if (agent) agent.enabled = true;
    }

    #endregion
}