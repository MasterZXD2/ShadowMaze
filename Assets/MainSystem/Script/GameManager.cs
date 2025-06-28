using ShadowMaze.MyCharacterController;
using UnityEngine;

namespace ShadowMaze
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private GameObject _spawnPoint;
        [SerializeField] private GameObject _player;
        private GameObject _playerIN;

        public GameObject PlayerIN => _playerIN;

        private void Awake()
        {
            spawnPlayer();
            SetActivePlayer(false);
        }

        public void spawnPlayer()
        {
            _playerIN = Instantiate(_player);
            _playerIN.transform.position = _spawnPoint.transform.position;
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