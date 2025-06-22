using ShadowMaze.MyCharacterController;
using UnityEngine;

namespace ShadowMaze.InteractSystem
{
    public class MoveableObject : InteractObject
    {
        private bool pick = false;

        [Space(10)]
        [Header("Environment Details")]
        [SerializeField] private LayerMask _groundLayers;

        protected override void Update()
        {
            base.Update();
            if (pick) MoveSelectObj();
        }

        public override void Interact()
        {
            pick = !pick;
            Debug.Log(pick);
        }

        public void MoveSelectObj()
        {
            Vector3 targetPosition = PlayerController.Instance.transform.position + PlayerController.Instance.transform.forward * 2 + Vector3.up * 1;
            transform.position = targetPosition;
        }
    }
}