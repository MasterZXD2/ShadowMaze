using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShadowMaze.CutSceneSystem
{
    public class StartSceneUI : MonoBehaviour
    {
        [SerializeField] private CutScene menuCutScene;
        [field: SerializeField] private List<CutScene> cutSceneList; 
        [SerializeField] private GameObject Cam;
        [SerializeField] private GameObject BattUI;
        [SerializeField] private GameObject skipUI;
        [SerializeField] private Button skipBU;
        [SerializeField] private GameObject playUI;
        [SerializeField] private Button playBU;

        [SerializeField] private GameObject _spawnPoint;
        [SerializeField] private GameObject _player;

        private Camera _camera;
        private int _currCutScene = 0;
        private GameObject _playerIN;
        private bool hasStarted = false;

        private void Awake()
        {
            _camera = Cam.GetComponent<Camera>();
            menuCutScene.SetVideoCamera(_camera);

            skipUI.SetActive(false);
            BattUI.SetActive(false);

            menuCutScene.SetVideoPrepared(() => {
                menuCutScene.Play();
            });
            menuCutScene.Prepare();

            playBU.onClick.AddListener(OnPlayCutscene);
        }

        private void OnPlayCutscene()
        {
            if (hasStarted) return;
            hasStarted = true;

            playUI.SetActive(false);

            menuCutScene.Stop();
            StartCoroutine(StartCutScene());
        }

        private IEnumerator StartCutScene()
        {
            skipUI.SetActive(true);

            while (_currCutScene < cutSceneList.Count)
            {
                bool isFinished = false;
                CutScene current = cutSceneList[_currCutScene];

                current.SetVideoCamera(_camera);
                skipBU.onClick.RemoveAllListeners();
                skipBU.onClick.AddListener(() => {
                    current.Skip();
                });
                current.SetVideoEnd(() => {
                    isFinished = true;
                });

                current.Play();

                yield return new WaitUntil(() => isFinished);

                _currCutScene++;
            }
            skipBU.onClick.RemoveAllListeners();

            Cam.tag = "Untagged";
            Cam.SetActive(false);

            BattUI.SetActive(true);
            skipUI.SetActive(false);

            _playerIN = Instantiate(_player);
            _playerIN.transform.position = _spawnPoint.transform.position;
        }

    }

}