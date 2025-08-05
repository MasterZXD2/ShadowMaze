using ShadowMaze.InteractSystem;
using ShadowMaze.MyCharacterController;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;
using UnityEngine.UIElements;

namespace ShadowMaze.EnemyCharactor
{
    public class TransitionEnemyAI : SimpleEnemyAi
    {
        #region Class Veriables
        [SerializeField] private Renderer enemyMeshRenderer; // MeshRenderer ของแคปซูล (ศัตรู)
        [SerializeField] private float cubeFormDuration = 5f; // ระยะเวลาที่อยู่ในร่างกล่องก่อนกลับเป็นศัตรู (ถ้าไม่โดนไฟฉายส่อง)
        [SerializeField] private GameObject cubeFormPrefab; // Prefab ของกล่องที่จะใช้แปลงร่าง
        [SerializeField] private Vector3 sceOffset = Vector3.one;

        private GameObject currentCubeFormInstance; // ตัวแปรเก็บ Instance ของกล่องที่ถูกสร้างขึ้นมา
        private float cubeFormTimer = 0f; // นับเวลาที่อยู่ในร่างกล่อง (เพื่อกลับร่างเดิม)
        #endregion

        #region Normal Function

        #endregion

        #region Movement Function
        protected override void Move(float PH)
        {
            if (enemyState.CurrEnemyActionState == EnemyActionState.StunnedAsCube)
            {
                if (isCurrentlyHitByFlashlight)
                {
                    cubeFormTimer = 0f;
                }
                else
                {
                    if (InMemeoryFragmentArea) return;
                    cubeFormTimer += Time.deltaTime;
                    if (cubeFormTimer >= cubeFormDuration)
                    {
                        TransitionToEnemyForm();
                        cubeFormTimer = 0f;
                    }
                }
            }
            else
            {
                NormalMove(PH);
            }
        }

        protected override void OnFlashLight()
        {
            if (isCurrentlyHitByFlashlight)
            {
                flashlightHitTimer += Time.deltaTime;
            }
            else
            {
                flashlightHitTimer = 0f;
            }

            if (flashlightHitTimer >= flashlightStunDuration)
            {
                TransitionToCubeForm();
                flashlightHitTimer = 0f;
            }
        }

        protected override void AAP()
        {
            disappear();
            Invoke(nameof(DelayedRespawn), 3f);
        }

        #endregion

        #region Transition Functions
        private void TransitionToCubeForm()
        {
            if (enemyState.CurrEnemyActionState == EnemyActionState.StunnedAsCube)
                return;
            disappear();
            enemyState.ActionSet(EnemyActionState.StunnedAsCube); // เปลี่ยนสถานะเป็น StunnedAsCube

            if (cubeFormPrefab != null)
            {
                // สร้างกล่องที่ตำแหน่งและ Rotation ปัจจุบันของ EnemyAI (แคปซูล)
                currentCubeFormInstance = Instantiate(cubeFormPrefab, transform.position, transform.rotation, transform);
                Vector3 s = currentCubeFormInstance.transform.localScale;
                for (int i = 0; i < 3; i++) s[i] *= sceOffset[i];
                currentCubeFormInstance.transform.localScale = s;
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
            agent.SetDestination(transform.position);

            _positionSet = false;
            isMovement = false;
            isAttack = false;
            isLPP = false;
            isCanSeePlayer = false;

            astop = true;

            if (enemyMeshRenderer) enemyMeshRenderer.enabled = false;
            if (enemyController) enemyController.enabled = false;
            //if (agent) agent.enabled = false;
        }
        private void DelayedRespawn() => StartCoroutine(respawn(spawnPoint));

        private IEnumerator respawn(Vector3 point)
        {
            yield return new WaitUntil(() => !EnemyManager.Instance._b);
            transform.position = spawnPoint;
            agent.Warp(transform.position);
            agent.SetDestination(transform.position);

            enemyState.ActionSet(EnemyActionState.Idle);
            flashlightHitTimer = 0f;
            cubeFormTimer = 0f;

            isCanSeePlayer = true;
            astop = false;
            isAttack = false;

            if (enemyMeshRenderer) enemyMeshRenderer.enabled = true;
            if (enemyController) enemyController.enabled = true;
            if (agent) agent.enabled = true;
        }

        #endregion
    }

}