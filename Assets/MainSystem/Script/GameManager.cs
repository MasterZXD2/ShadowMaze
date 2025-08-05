using ShadowMaze.MyCharacterController;
using UnityEngine;

namespace ShadowMaze
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private GameObject _spawnPoint;
        [SerializeField] private GameObject _player;
        [SerializeField] private float ct = .5f;
        private GameObject _playerIN;
        public bool isTest = false;
        public GameObject PlayerIN => _playerIN;

        private float _ct = 0f;

        private void Awake()
        {
            spawnPlayer();
            SetActivePlayer(isTest);
        }

        public void spawnPlayer()
        {
            _playerIN = Instantiate(_player);
            _playerIN.transform.position = _spawnPoint.transform.position;
        }

        private void Update()
        {
            if (_ct < ct)
            {
                _ct += Time.deltaTime;
                if (_playerIN.transform.position !=  _spawnPoint.transform.position) _playerIN.transform.position = _spawnPoint.transform.position;
            } 
        }

        public void SetActivePlayer(bool b)
        {
            if (b)
            {
                PlayerController.Instance.ChangeCam(CamMode.FirstPerson);
                PlayerController.Instance.replay();
            }
            else {
                PlayerController.Instance.ChangeCam(CamMode.None);
                PlayerController.Instance.pause();
            }
        }
    }
}