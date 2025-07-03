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

        public float DeHealth;
        public float Health = 100f;
        public float MaxHealth = 100f;
        public float DeBattery;
        public float Battery = 100f;
        public float MaxBattery = 100f;
        public float DeSanity;
        public float Sanity = 100f;
        public float MaxSanity = 100f;

        public bool isDie;

        private void Start()
        {
            DeHealth = Health;
            DeBattery = Battery;
            DeSanity = Sanity;
        }

        public void die()
        {
            Health = DeHealth;
            MaxHealth = DeHealth;
            Battery = DeBattery;
            MaxBattery = DeBattery;
            Sanity = DeSanity;
            MaxSanity = DeSanity;
            isDie = true;
        }

        public void restart()
        {
            isDie = false;
        }
    }

}