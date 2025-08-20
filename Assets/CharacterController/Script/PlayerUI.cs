using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShadowMaze.MyCharacterController {
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private PlayerStats _playerStats;
        [SerializeField] private float BBLerpSpeed = 0.05f;
        [SerializeField] private float SSLerpSpeed = 0.05f;
        [SerializeField] private string StartScene = "StartScene";
        private TimelineTrigger trigger;
        private TimelineTrigger doorTrigger;
        private GameObject _door;
        private GameObject _mainUI;
        private GameObject _batteryBar;
        private Slider _batteryBS;
        private GameObject _sanityHeart;
        private GameObject _sanityBar;
        private Slider _sanityBS;
        private GameObject _resumeUI;
        private Button _resumeBU;
        private GameObject _MF;
        private GameObject _FMF;
        private GameObject _GO;
        private GameObject _quitUI;
        private Button _quitBU;
        private GameObject _retryUI;
        private Button _retryBU;

        private float _normalSanitySize = 1f;
        private float _beatSanitySize = 1.3f;

        private int _MFV = 0;
        public bool _esc = false;

        private bool b = false;

        private void SetupUI()
        {
            _mainUI = GameObject.FindGameObjectWithTag("MainUI");
            _batteryBar = _mainUI.transform.Find("BatteryBar").gameObject;
            _batteryBS = _batteryBar.GetComponentInChildren<Slider>();
            _sanityHeart = _mainUI.transform.Find("SanityHeart").gameObject;
            _sanityBar = _mainUI.transform.Find("SanityBar").gameObject;
            _sanityBS = _sanityBar.GetComponentInChildren<Slider>();
            _resumeUI = _mainUI.transform.Find("Resume").gameObject;
            _resumeBU = _resumeUI.GetComponent<Button>();
            _MF = _mainUI.transform.Find("MFUI").gameObject;
            _FMF = _mainUI.transform.Find("FullMF").gameObject;
            _door = GameObject.FindGameObjectWithTag("TMDoorBoss");
            if (_door) doorTrigger = _door.GetComponent<TimelineTrigger>();
            GameObject g = GameObject.FindGameObjectWithTag("CrystalAnimation");
            if (g) trigger = g.GetComponent<TimelineTrigger>();
            _GO = _mainUI.transform.Find("GO").gameObject;
            _quitUI = _mainUI.transform.Find("QGO").gameObject;
            _quitBU = _quitUI.GetComponent<Button>();
            _retryUI = _mainUI.transform.Find("ReGO").gameObject;
            _retryBU = _retryUI.GetComponent<Button>();
        }

        private void Awake() => SetupUI();

        private void Start()
        {
            b = true;
            _normalSanitySize = _sanityHeart.transform.localScale.x;
            _beatSanitySize = _normalSanitySize * 1.15f;

            _FMF.SetActive(false);
            _resumeUI.SetActive(false);
            _resumeBU.onClick.RemoveAllListeners();
            _resumeBU.onClick.AddListener(() =>
            {
                PlayerController.Instance.OnEsc(true);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            });

            _GO.SetActive(false);
            _quitUI.SetActive(false);
            _retryUI.SetActive(false);
            _quitBU.onClick.RemoveAllListeners();
            _retryBU.onClick.RemoveAllListeners();
            _quitBU.onClick.AddListener(() => { 
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            });
            _retryBU.onClick.AddListener(() => {
                Destroy(PlayerController.Instance.gameObject);
                SceneManager.LoadScene(StartScene);
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
            if (_esc) return;

            if (_batteryBS.value != _playerStats.Battery) { 
                if (!b) _playerStats.Battery = _batteryBS.value;
                else _batteryBS.value = Mathf.Lerp(_batteryBS.value, _playerStats.Battery, BBLerpSpeed);
            }
            if (_MFV != _playerStats.memoryFragment && b) {
                for (int i = 0; i <= _MFV; i++)
                {
                    if (i < 3) _MF.transform.Find(i.ToString()).Find("C").GetComponent<RawImage>().color = new Color(1,1,1,1);
                }
                _MFV = _playerStats.memoryFragment;
                if (_MFV >= 3) FullMF(); //StartCoroutine(FullMF());
                //_memoryFragmentText.text = $"memory fragment {_MFV.ToString()}/3";
            }

            if (_sanityHeart && _playerStats)
            {
                if (_sanityBS.value != _playerStats.Sanity) _sanityBS.value = Mathf.Lerp(_sanityBS.value,_playerStats.Sanity,SSLerpSpeed);
                
                float normalizedSanity = Mathf.Clamp01(_playerStats.Sanity / _playerStats.MaxSanity); // 1 = ปกติ, 0 = กลัวสุด
                float beatSpeed = Mathf.Lerp(3f, 8f, 1f - normalizedSanity); // ยิ่งกลัว → เต้นเร็ว (ความถี่สูง)
                float scaleFactor = Mathf.Lerp(_normalSanitySize, _beatSanitySize, Mathf.Sin(Time.time * beatSpeed) * 0.5f + 0.5f); // scale: 1 → 1.3

                _sanityHeart.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
            }
        }

        private void FullMF()
        {
            //trigger.PlayTimeline();
            //yield return new WaitUntil(() => !trigger.IsPlaying);
            _FMF.SetActive(true);
            _MF.SetActive(false);
        }

        public void On()
        {
            b = true;
            e();
            if (_MFV >= 3) FullMF();
        }

        public void Off()
        {
            b = false;
            e();
        }
        
        public void Toggle()
        {
            b = !b;
            e();
        }

        private void e()
        {
            _batteryBar.SetActive(b);
            _sanityHeart.SetActive(b);
            _sanityBar.SetActive(b);
            _MF.SetActive(b);
            _FMF.SetActive(false);
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

        public void GameOver()
        {
            _GO.SetActive(true);
            _quitUI.SetActive(true);
            _retryUI.SetActive(true);
        }
    }

}