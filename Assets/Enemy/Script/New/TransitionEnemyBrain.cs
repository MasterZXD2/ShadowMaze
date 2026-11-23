using ShadowMaze.EnemyCharactor;
using ShadowMaze.InteractSystem;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class TransitionEnemyBrain : PatrolChaseBrain
{
    #region Class Veriables
    [SerializeField] private List<Renderer> enemyMeshRenderers; // MeshRenderer ของแคปซูล (ศัตรู)
    [SerializeField] private List<ParticleSystem> particleSystemRenderers;
    [SerializeField] private float cubeFormDuration = 5f; // ระยะเวลาที่อยู่ในร่างกล่องก่อนกลับเป็นศัตรู (ถ้าไม่โดนไฟฉายส่อง)
    [SerializeField] private GameObject cubeFormPrefab; // Prefab ของกล่องที่จะใช้แปลงร่าง

    private GameObject currentCubeFormInstance; // ตัวแปรเก็บ Instance ของกล่องที่ถูกสร้างขึ้นมา
    public bool isInCudefrom { get; private set; } = false;
    public EnemyState enemyState => state;

    private Vector3 spawnPoint;
    #endregion

    protected void Start()
    {
        spawnPoint = transform.position;
        combat.attackSucceeded.AddListener(() =>
        {
            Disappear();
            Invoke(nameof(DelayedRespawn), 3f);
        });
    }

    protected override void Update()
    {
        if (isInCudefrom)
            return;
        base.Update();
    }

    protected override void NormalMove()
    {
        if (state.CurrEnemyActionState == EnemyActionState.StunnedAsCube)
            return;
        base.NormalMove();
    }

    #region Transition Functions
    public void TransitionToCubeForm()
    {
        if (state.CurrEnemyActionState == EnemyActionState.StunnedAsCube)
            return;
        Disappear();
        state.ActionSet(EnemyActionState.StunnedAsCube); // เปลี่ยนสถานะเป็น StunnedAsCube

        if (cubeFormPrefab != null)
        {
            // สร้างกล่องที่ตำแหน่งและ Rotation ปัจจุบันของ EnemyAI (แคปซูล)
            currentCubeFormInstance = Instantiate(cubeFormPrefab, transform.position, new Quaternion(0, 0, 0, 0));
            if (!currentCubeFormInstance.TryGetComponent<EnemyCubeOwner>(out var cubeOwner))
                cubeOwner = currentCubeFormInstance.AddComponent<EnemyCubeOwner>();
            cubeOwner.Owner = reaction as CubeOnLight;
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

    public void TransitionToEnemyForm()
    {
        Vector3 spawnPosition;
        float colliderHeight = motor.GetColliderHeight();
        RaycastHit hit;
        Vector3 raycastOrigin = currentCubeFormInstance ? currentCubeFormInstance.transform.position + Vector3.up * colliderHeight : transform.position + Vector3.up * colliderHeight;
        if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, colliderHeight, perception.GroundLayers))
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

        StartCoroutine(Respawn(spawnPosition));
    }
    #endregion
    private void Disappear()
    {
        motor.SetDestination(transform.position);
        motor.Pause();

        hasPatrolDestination = false;
        isMovement = false;
        hasLastPlayerPosition = false;
        isBlind = true;

        enemy.Pause(); 
        presentation.GetModelCollider().isTrigger = true;
         
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
    }
    private void DelayedRespawn() => StartCoroutine(Respawn(spawnPoint));

    private IEnumerator Respawn(Vector3 point)
    {
        yield return new WaitUntil(() => !EnemyManager.Instance._b);
        isInCudefrom = false;

        motor.Resume();
        motor.Warp(point);
        motor.SetDestination(point);

        enemyState.ActionSet(EnemyActionState.Idle);

        isBlind = false;

        enemy.Resume();
        presentation.GetModelCollider().isTrigger = false;

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
    }
}
