using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.UI;

namespace ShadowMaze.MyCharacterController
{
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;

        public float Health = 100.0f;
        public float MaxHealth = 100.0f;

        public bool isDie;

        private void Start()
        {

        }

        public void die()
        {
            Health = 100.0f;
            MaxHealth = Health;
            isDie = true;
        }

        public void restart()
        {
            isDie = false;
        }
    }

}