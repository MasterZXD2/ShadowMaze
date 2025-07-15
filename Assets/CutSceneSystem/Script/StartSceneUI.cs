using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        [SerializeField] private GameObject SanBar;
        [SerializeField] private GameObject skipUI;
        [SerializeField] private Button skipBU;
        [SerializeField] private GameObject playUI;
        [SerializeField] private Button playBU;
        [SerializeField] private GameObject MF;
        [SerializeField] private GameObject settUI;
        [SerializeField] private Button settBU;
        [SerializeField] private GameObject exUI;
        [SerializeField] private Button exBU;

        [SerializeField] private GameObject ST;

        [SerializeField] private string nextScene;
        [SerializeField] private bool InStartScene = false;

        private Camera _camera;
        private int _currCutScene = 0;
        private GameObject _playerIN;
        private bool hasStarted = false;

        private void Awake()
        {
            _camera = Cam.GetComponent<Camera>();
            menuCutScene.SetVideoCamera(_camera);

            if (skipUI) skipUI.SetActive(false);
            if (BattUI) BattUI.SetActive(false);
            if (SanUI) SanUI.SetActive(false);
            if (SanBar) SanBar.SetActive(false);
            if (MF) MF.SetActive(false);

            menuCutScene.SetVideoPrepared(() => {
                menuCutScene.Play();
            });
            menuCutScene.Prepare();

            playBU.onClick.RemoveAllListeners();
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
            settUI.SetActive(false);
            exUI.SetActive(false);

            menuCutScene.Stop();
            StartCoroutine(StartCutScene());
        }

        private IEnumerator StartCutScene()
        {
            ST.SetActive(false);
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

            if (InStartScene)
            {
                SceneManager.LoadScene(nextScene);
            } else
            {
                skipBU.onClick.RemoveAllListeners();

                Cam.tag = "Untagged";
                Cam.SetActive(false);
                foreach (var comp in Cam.GetComponents<Behaviour>())
                {
                    comp.enabled = false;
                }

                BattUI.SetActive(true);
                SanUI.SetActive(true);
                SanBar.SetActive(true);
                MF.SetActive(true);
                skipUI.SetActive(false);

                GameManager.Instance.SetActivePlayer(true);
            }
        }


    }

}