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
                    if (GO.transform.parent.TryGetComponent<SimpleEnemyAi>(out SimpleEnemyAi sei))
                    {
                        sei.InArea("MFA");
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
                    if (GO.transform.parent.TryGetComponent<SimpleEnemyAi>(out SimpleEnemyAi sei))
                    {
                        sei.OutArea();
                    }
                }
            }
        }
    }
}
