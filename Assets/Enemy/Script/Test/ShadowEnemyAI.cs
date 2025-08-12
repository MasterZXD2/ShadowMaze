using ShadowMaze.MyCharacterController;
using UnityEngine;
using UnityEngine.AI;

namespace ShadowMaze.EnemyCharactor
{
    public class ShadowEnemyAI : SimpleEnemyAi
    {
        [SerializeField] private LayerMask enemyMask;
        [SerializeField] private float dSASpawn;
        [SerializeField] private float otherRange = 2f;
        [SerializeField] private GameObject shadowArea;
        private float SASpawn = 0;
        private bool SAN = false;
        private int _count = 0;
        private bool _group = false;
        private bool lamp = false;

        protected override void Update()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, otherRange);
            foreach (Collider item in colliders)
            {
                if (item.gameObject.TryGetComponent<ShadowEnemyAI>(out ShadowEnemyAI enemy))
                {
                    if (enemy == this) continue;
                    _count++;
                }
            }
            _group = _count > 0;

            base.Update();
            if (!SAN)
            {
                SASpawn += Time.deltaTime;
                if (SASpawn > dSASpawn)
                {
                    SAN = true;
                    SpawnArea(transform.forward);
                    SpawnArea(transform.right);
                    SpawnArea(-transform.forward);
                    SpawnArea(-transform.right);
                }
            }

            _count = 0;
            lamp = false;
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(transform.position, otherRange);
        }

        private void SpawnArea(Vector3 offset)
        {
            Vector3 pos = transform.position + offset;
            Vector3 sp;
            if (Physics.Raycast(pos, -Vector3.up, out RaycastHit hit, 10f, _groundLayers))
            {
                sp = hit.point + hit.normal * .1f;
            } else
            {
                sp = pos;
            }
            Collider[] c0 = Physics.OverlapSphere(sp, 1f, enemyMask);
            bool a = true;
            foreach (Collider item in c0)
            {
                if (item.gameObject.TryGetComponent<ShadowArea>(out ShadowArea sa))
                {
                    a = false;
                }
            }
            if (a)
            {
                GameObject obj0 = Instantiate(shadowArea);
                obj0.transform.position = sp;
            }
        }

        protected override void OnFlashLight()
        {
            if (isCurrentlyHitByFlashlight)
            {
                if (_group)
                {
                    if (lamp) flashlightHitTimer += Time.deltaTime;
                }
                else { 
                    if (!lamp) flashlightHitTimer += Time.deltaTime; 
                }
            }
            else
            {
                flashlightHitTimer = 0f;
            }

            if (flashlightHitTimer >= flashlightStunDuration) die();
        }

        public override void HitByFlashlight(bool b = false)
        {
            base.HitByFlashlight(b);
            lamp = b;
        }
    }
}