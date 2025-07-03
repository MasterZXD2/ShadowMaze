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
        [SerializeField] private GameObject SanUI;
        [SerializeField] private GameObject skipUI;
        [SerializeField] private Button skipBU;
        [SerializeField] private GameObject playUI;
        [SerializeField] private Button playBU;

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
            SanUI.SetActive(false);

            menuCutScene.SetVideoPrepared(() => {
                menuCutScene.Play();
            });
            menuCutScene.Prepare();

            playBU.onClick.AddListener(OnPlayCutscene);
        }

        private void Start()
        {
            foreach (CutScene cutScene in cutSceneList)
            {
                cutScene.Prepare();
                cutScene.Stop();
            }
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

                if (current == null)
                {
                    Debug.LogWarning("Cutscene is missing or clip is null. Skipping...");
                    _currCutScene++;
                    continue;
                }

                current.SetVideoCamera(_camera);

                skipBU.onClick.RemoveAllListeners();
                skipBU.onClick.AddListener(() => {
                    current.Skip();
                });

                current.SetVideoEnd(() => {
                    current.Stop();
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
            SanUI.SetActive(true);
            skipUI.SetActive(false);

            GameManager.Instance.SetActivePlayer(true);
        }


    }

}