using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShadowMaze.MyCharacterController {
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private PlayerStats _playerStats;
        [SerializeField] private float BBLerpSpeed = 0.05f;
        private GameObject _mainUI;
        private GameObject _batteryBar;
        private Slider _batteryBS;

        private void SetupUI()
        {
            _mainUI = GameObject.FindGameObjectWithTag("MainUI");
            _batteryBar = _mainUI.transform.Find("BatteryBar").gameObject;
            _batteryBS = _batteryBar.GetComponentInChildren<Slider>();
        }

        private void Awake() => SetupUI();

        private void Start()
        {
            if (_batteryBS)
            {
                _batteryBS.minValue = 0;
                _batteryBS.maxValue = _playerStats.MaxBattery;
            }
        }

        private void Update()
        {
            if (_batteryBS.value != _playerStats.Health) _batteryBS.value = Mathf.Lerp(_batteryBS.value, _playerStats.Battery, BBLerpSpeed);
        }

        public void die()
        {
            _batteryBar.SetActive(false);
        }

        public void restart()
        {
            SetupUI();
            _batteryBS.minValue = 0;
            _batteryBS.maxValue = _playerStats.MaxBattery;
        }
    }

}