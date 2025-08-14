using ShadowMaze.MyCharacterController;
using UnityEngine;

public class StraightPath : MonoBehaviour
{
    [SerializeField] private GameObject sound;
    private AudioSource source;
    private bool plrInZone = false;
    private Vector3 offset = Vector3.zero;

    private void Start()
    {
        source = sound.GetComponent<AudioSource>();

        source.rolloffMode = AudioRolloffMode.Linear; // เลือกแบบ Linear หรือ Logarithmic ก็ได้
        source.minDistance = 2f;  // ระยะที่ยังคงดังเท่าเดิม
        source.maxDistance = 15f; // เกินนี้จะเงียบลง
    }

    private void Update()
    {
        if (!plrInZone) return;
        if (PlayerController.Instance.IsMovinglaterally())
        {
            offset += Vector3.forward * 2f * Time.deltaTime;
            Play(PlayerController.Instance.transform);
        }
        else
        {
            offset = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        plrInZone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        plrInZone = false;
    }

    private void Play(Transform transform)
    {
        //offset += transform.forward * -2f * Time.deltaTime;
        sound.transform.position = transform.position - offset - transform.forward * 14;
        
        if (!source.isPlaying)
            source.Play();
    }
}
