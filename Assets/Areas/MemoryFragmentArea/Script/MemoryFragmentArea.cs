using ShadowMaze.EnemyCharactor;
using ShadowMaze.InteractSystem;
using UnityEngine;

namespace ShadowMaze.Area
{
    public class MemoryFragmentArea : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            GameObject GO = other.gameObject;
            Debug.Log(GO);
            if (GO.TryGetComponent<InteractObject>(out InteractObject IO))
            {
                if (GO.CompareTag("Enemy"))
                {
                    if (GO.transform.parent && GO.transform.parent.TryGetComponent<SimpleEnemyAi>(out SimpleEnemyAi sei))
                    {
                        sei.InArea("MFA");
                    }
                    if (LightReaction.FindReceiver(GO) is CubeOnLight col)
                    {
                        col.InArea("MFA");
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            GameObject GO = other.gameObject;
            if (GO.TryGetComponent<InteractObject>(out InteractObject IO))
            {
                if (GO.CompareTag("Enemy"))
                {
                    if (GO.transform.parent && GO.transform.parent.TryGetComponent<SimpleEnemyAi>(out SimpleEnemyAi sei))
                    {
                        sei.OutArea();
                    }
                    if (LightReaction.FindReceiver(GO) is CubeOnLight col)
                    {
                        col.OutArea();
                    }
                }
            }
        }
    }
}
