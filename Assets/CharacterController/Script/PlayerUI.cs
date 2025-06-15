using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShadowMaze.MyCharacterController {
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] PlayerStats _playerStats;

        private void SetupUI()
        {
            
        }

        private void Awake() => SetupUI();

        private void Start()
        {
            
        }

        private void Update()
        {
            
        }

        public void die()
        {

        }

        public void restart()
        {
            SetupUI();
        }
    }

}