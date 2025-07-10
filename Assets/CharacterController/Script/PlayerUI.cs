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
        private GameObject _sanityBar;
        private Slider _sanityBS;
        private GameObject _memoryFragment;
        private TextMeshProUGUI _memoryFragmentText;
        private GameObject _resumeUI;
        private Button _resumeBU;
        private int _MFV = 0;

        private bool _esc = false;

        private void SetupUI()
        {
            _mainUI = GameObject.FindGameObjectWithTag("MainUI");
            _batteryBar = _mainUI.transform.Find("BatteryBar").gameObject;
            _batteryBS = _batteryBar.GetComponentInChildren<Slider>();
            _sanityBar = _mainUI.transform.Find("SanityBar").gameObject;
            _sanityBS = _sanityBar.GetComponentInChildren<Slider>();
            _memoryFragment = _mainUI.transform.Find("MFText").gameObject;
            _memoryFragmentText = _memoryFragment.GetComponent<TextMeshProUGUI>();
            _resumeUI = _mainUI.transform.Find("Resume").gameObject;
            _resumeBU = _resumeUI.GetComponent<Button>();
        }

        private void Awake() => SetupUI();

        private void Start()
        {
            _resumeUI.SetActive(false);
            _resumeBU.onClick.RemoveAllListeners();
            _resumeBU.onClick.AddListener(() =>
            {
                OnEsc();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            });

            if (_batteryBS)
            {
                _batteryBS.minValue = 0;
                _batteryBS.maxValue = _playerStats.MaxBattery;
            }
            if (_sanityBS)
            {
                _sanityBS.minValue = 0;
                _sanityBS.maxValue = _playerStats.MaxSanity;
            }
        }

        private void Update()
        {
            if (_batteryBS.value != _playerStats.Health) _batteryBS.value = Mathf.Lerp(_batteryBS.value, _playerStats.Battery, BBLerpSpeed);
            if (_sanityBS.value != _playerStats.Sanity) _sanityBS.value = Mathf.Lerp(_sanityBS.value, _playerStats.Sanity, BBLerpSpeed);
            if (_MFV != _playerStats.memoryFragment) {
                _MFV = _playerStats.memoryFragment;
                _memoryFragmentText.text = $"memory fragment {_MFV.ToString()}/3";
            }
        }

        public void OnEsc()
        {
            _esc = !_esc;
            _resumeUI.SetActive(_esc);
        }

        public void die()
        {
            _batteryBar.SetActive(false);
            _sanityBar.SetActive(false);
        }

        public void restart()
        {
            SetupUI();
            _batteryBS.minValue = 0;
            _batteryBS.maxValue = _playerStats.MaxBattery;
            _sanityBS.minValue = 0;
            _sanityBS.maxValue = _playerStats.MaxSanity;
        }
    }

}