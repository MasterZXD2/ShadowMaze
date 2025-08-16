using ShadowMaze.EnemyCharactor;
using UnityEngine;

namespace ShadowMaze.InteractSystem
{
    public class lampObject : MoveableObject
    {
        [SerializeField] private Light _light;
        [SerializeField] private float onLightD = 1f;
        [SerializeField] private float deSpeed = 3f;
        [SerializeField] private float checkRange = 2f;
        [SerializeField] private LayerMask _puzzlelayerMask; 
        private float cIl;
        private float lightTime;
        private bool f = false;

        private void Awake()
        {
            _light.enabled = false;
            cIl = _light.intensity;
        }

        protected override void Update()
        {
            base.Update();
            if (_light.enabled)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, checkRange, _puzzlelayerMask);
                foreach (Collider collider in colliders)
                {
                    GameObject GO = collider.gameObject;
                    if (GO.CompareTag("Enemy"))
                    {
                        if (GO.TryGetComponent<SimpleEnemyAi>(out SimpleEnemyAi sei))
                        {
                            if (GO.TryGetComponent<ShadowEnemyAI>(out ShadowEnemyAI enemy))
                            {
                                enemy.HitByFlashlight(true);
                            }
                            else
                            {
                                sei.HitByFlashlight();
                            }
                        }
                        else if (GO.transform.parent.CompareTag("Enemy"))
                        {
                            if (GO.transform.parent.TryGetComponent<SimpleEnemyAi>(out SimpleEnemyAi psei))
                            {
                                if (GO.transform.parent.TryGetComponent<ShadowEnemyAI>(out ShadowEnemyAI enem))
                                {
                                    enem.HitByFlashlight(true);
                                }
                                else
                                {
                                    psei.HitByFlashlight();
                                }
                            }
                        }

                        if (GO.TryGetComponent<TheWatchingEyesAI>(out TheWatchingEyesAI twe))
                        {
                            twe.HitByFlashlight();
                        }
                    }

                    if (GO.TryGetComponent<Mirror>(out Mirror m))
                    {
                        m.HitByFlashlight(transform, true);
                    }
                }

                lightTime -= Time.deltaTime * deSpeed;
                _light.intensity = cIl * Mathf.Clamp01(lightTime);
                if (lightTime < 0)
                {
                    _light.enabled = false;
                    f = true;
                }
            }
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position,checkRange);
        }

        public override void Interact()
        {
            base.Interact();
            if (f || _light == null) return;
            _light.enabled = true;
            lightTime = onLightD;
        }
    }
}