using ShadowMaze.CutSceneSystem;
using ShadowMaze.EnemyCharactor;
using ShadowMaze.MyCharacterController;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.SceneManagement;

public class TheWatchingEyesArea : Singleton<TheWatchingEyesArea>
{
    [SerializeField] private string StartScene = "StartScene";
    [SerializeField] private BoxCollider Collider;
    [SerializeField] private BoxCollider AreaSpawn;
    [field: SerializeField] private List<GameObject> prefabsEnemys = new List<GameObject>();
    [SerializeField] private GameObject bossPrefabs;
    [SerializeField] private Transform bossSpawn;

    [SerializeField] private float minPerSpawn;
    [SerializeField] private float maxPerSpawn;

    [SerializeField] private GameObject mirrorParent;
    [field: SerializeField] private List<LightPuzzleChanger> lightPuzzles = new List<LightPuzzleChanger>();

    [SerializeField] private CutScene cutScene;

    [SerializeField] private LayerMask _spawnAreaLayerMask;
    [SerializeField] private LayerMask _groundlayerMask;

    private List<GameObject> _currEnemies = new List<GameObject>();
    private TheWatchingEyesAI boss;
    private Coroutine _waveCoroutine;

    private bool _isPaused = false;
    private bool res = false;
    private bool die = false; 

    private void Start()
    {
        cutScene.SetVideoCamera(PlayerController.Instance._plrCamera);
        GameObject b = Instantiate(bossPrefabs,transform);
        b.transform.position = bossSpawn.position;
        if (Physics.Raycast(b.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, _groundlayerMask))
        {
            b.transform.position = hit.point;
        }
        EnemyManager.Instance.Add(b);
        boss = b.GetComponent<TheWatchingEyesAI>();
        boss.set(lightPuzzles, mirrorParent);

        cutScene.Prepare();
        cutScene.Stop();
        cutScene.SetVideoEnd(() =>
        {
            Destroy(PlayerController.Instance.gameObject);
            SceneManager.LoadScene(StartScene);
        });
    }

    private void Update()
    {
        if (boss.Health <= 0 && !die)
        {
            die = true;
            cutScene.Play();
            boss.pause();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;
        if (go.CompareTag("Player")) boss.EnterArea();
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject go = other.gameObject;
        if (go.CompareTag("Player")) boss.ExitArea();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (Collider) Gizmos.DrawWireCube(Collider.bounds.center,Collider.size);
        if (AreaSpawn) Gizmos.DrawWireCube(AreaSpawn.bounds.center, AreaSpawn.size);
    }

    public void Play() => _waveCoroutine = StartCoroutine(PlayWaves());

    private IEnumerator PlayWaves()
    {
        while (true)
        {
            yield return new WaitWhile(() => _isPaused || res);
            if (res) yield break;

            GameObject enemy = PickRandomEnemy();

            Vector3 randomPoint = GetRandomPointInBounds(AreaSpawn.bounds);
            if (Physics.Raycast(randomPoint + Vector3.up * 10f,
                                            Vector3.down,
                                            out RaycastHit hit,
                                            20f,
                                            _spawnAreaLayerMask))
            {
                // ยกขึ้นจากพื้นตามขนาด Collider ของ prefab
                float yOffset = GetColliderYOffset(enemy);
                Vector3 spawnPos = hit.point + Vector3.up * yOffset;

                GameObject GO = Instantiate(enemy, spawnPos, Quaternion.identity,transform);
                _currEnemies.Add(GO);
            }

            float waitTime = Random.Range(minPerSpawn, maxPerSpawn);
            yield return new WaitForSeconds(waitTime);
         }
    }

    public void Stop()
    {
        res = true;
        if (_waveCoroutine != null)
        {
            StopCoroutine(_waveCoroutine);
            _waveCoroutine = null;
        }

        foreach (GameObject enemy in _currEnemies)
        {
            if (enemy != null)
            {
                SimpleEnemyAi SE = enemy.GetComponent<SimpleEnemyAi>();
                if (SE != null)
                    SE.die();
            }
        }
        _currEnemies.Clear();
    }

    // สุ่ม GameObject จาก Wave.enemies
    private GameObject PickRandomEnemy()
    {
        int idx = Random.Range(0, prefabsEnemys.Count);
        return prefabsEnemys[idx];
    }

    // ฟังก์ชันสุ่มจุดใน Bounds
    private Vector3 GetRandomPointInBounds(Bounds b)
    {
        float x = Random.Range(b.min.x, b.max.x);
        float y = b.max.y;      // เริ่ม Raycast จากด้านบน
        float z = Random.Range(b.min.z, b.max.z);
        return new Vector3(x, y, z);
    }

    public void pause()
    {
        _isPaused = true;
        boss.pause();
    }

    public void resume()
    {
        if (!_isPaused)
            return;
        boss.resume();
        _isPaused = false;
    }

    // อ่านขนาด Collider เพื่อ offset Y
    private float GetColliderYOffset(GameObject prefab)
    {
        if (prefab.TryGetComponent<Collider>(out var col))
            return (col.bounds.extents.y == 0) ? col.bounds.extents.y : 0.5f;
        return 1f;
    }

    bool IsPointInsideMesh(Collider mesh, Vector3 point)
    {
        // ยิง Ray ขึ้นบนจากจุดนั้น
        var ray = new Ray(point, Vector3.up);

        // หาครบทุกครั้งที่ชน
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, _spawnAreaLayerMask);

        // ถ้าชนเป็นเลขคี่ → point อยู่ภายใน
        return hits.Length % 2 == 1;
    }
}
