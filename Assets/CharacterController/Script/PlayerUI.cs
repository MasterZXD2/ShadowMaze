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
        private GameObject _sanity;
        private GameObject _memoryFragment;
        private TextMeshProUGUI _memoryFragmentText;
        private GameObject _resumeUI;
        private Button _resumeBU;

        private float _normalSanitySize = 1f;
        private float _beatSanitySize = 1.3f;

        private int _MFV = 0;
        private bool _esc = false;

        private void SetupUI()
        {
            _mainUI = GameObject.FindGameObjectWithTag("MainUI");
            _batteryBar = _mainUI.transform.Find("BatteryBar").gameObject;
            _batteryBS = _batteryBar.GetComponentInChildren<Slider>();
            _sanity = _mainUI.transform.Find("Sanity").gameObject;
            _memoryFragment = _mainUI.transform.Find("MFText").gameObject;
            _memoryFragmentText = _memoryFragment.GetComponent<TextMeshProUGUI>();
            _resumeUI = _mainUI.transform.Find("Resume").gameObject;
            _resumeBU = _resumeUI.GetComponent<Button>();
        }

        private void Awake() => SetupUI();

        private void Start()
        {
            _normalSanitySize = _sanity.transform.localScale.x;
            _beatSanitySize = _normalSanitySize * 1.3f;

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
        }

        private void Update()
        {
            if (_batteryBS.value != _playerStats.Health) _batteryBS.value = Mathf.Lerp(_batteryBS.value, _playerStats.Battery, BBLerpSpeed);
            if (_MFV != _playerStats.memoryFragment) {
                _MFV = _playerStats.memoryFragment;
                _memoryFragmentText.text = $"memory fragment {_MFV.ToString()}/3";
            }

            if (_sanity && _playerStats)
            {
                float normalizedSanity = Mathf.Clamp01(_playerStats.Sanity / _playerStats.MaxSanity); // 1 = ปกติ, 0 = กลัวสุด
                float beatSpeed = Mathf.Lerp(3f, 8f, 1f - normalizedSanity); // ยิ่งกลัว → เต้นเร็ว (ความถี่สูง)
                float scaleFactor = Mathf.Lerp(_normalSanitySize, _beatSanitySize, Mathf.Sin(Time.time * beatSpeed) * 0.5f + 0.5f); // scale: 1 → 1.3

                _sanity.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
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
            //_sanityBar.SetActive(false);
        }

        public void restart()
        {
            SetupUI();
            _batteryBS.minValue = 0;
            _batteryBS.maxValue = _playerStats.MaxBattery;
            //_sanityBS.minValue = 0;
            //_sanityBS.maxValue = _playerStats.MaxSanity;
        }
    }

}