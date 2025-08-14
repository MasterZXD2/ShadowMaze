using UnityEngine;

public class TriggerPlaySound : MonoBehaviour
{
    public AudioSource source;      // ลาก AudioSource ของโซนมาใส่
    public bool playOnce = true;    // ให้เล่นครั้งเดียว
    private bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;   // ให้ Player ตั้ง Tag = "Player"
        if (playOnce && hasPlayed) return;

        source.Play();   // หรือ source.PlayOneShot(source.clip);
        hasPlayed = true;
    }
}
