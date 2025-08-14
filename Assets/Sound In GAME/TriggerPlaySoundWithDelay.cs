using UnityEngine;

public class TriggerPlaySoundWithDelay : MonoBehaviour
{
    [Header("References")]
    public AudioSource source;          // ลาก AudioSource ของโซนมาใส่
    [Tooltip("กำหนด Tag ของผู้เล่น เช่น Player")]
    public string playerTag = "Player";

    [Header("Timing")]
    [Tooltip("ดีเลย์ก่อนเล่นเสียง (วินาที)")]
    public float delaySeconds = 1f;

    [Tooltip("ถ้าเปิด: ต้องอยู่ในโซนครบเวลาถึงจะเล่นเสียง (ออกก่อน = ยกเลิก)")]
    public bool requireStayFullDelay = false;

    [Tooltip("คูลดาวน์หลังเล่นเสียง (วินาที) ก่อนจะเล่นได้อีกครั้ง")]
    public float cooldown = 0f;

    [Header("Behavior")]
    [Tooltip("เล่นครั้งเดียวเท่านั้น")]
    public bool playOnce = true;

    private bool hasPlayed = false;
    private bool playerInside = false;
    private float lastPlayTime = -Mathf.Infinity;
    private Coroutine pending;

    private void Reset()
    {
        // auto add AudioSource ถ้าไม่ได้ใส่ไว้
        if (source == null) source = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = true;

        if (playOnce && hasPlayed) return;
        if (Time.time < lastPlayTime + cooldown) return;

        // เริ่มนับดีเลย์ใหม่ทุกครั้งที่เข้า
        if (pending != null) StopCoroutine(pending);
        pending = StartCoroutine(PlayAfterDelay());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;

        // ถ้าบังคับให้ยืนครบเวลา แต่ผู้เล่นออกก่อน → ยกเลิก
        if (requireStayFullDelay && pending != null)
        {
            StopCoroutine(pending);
            pending = null;
        }
    }

    private System.Collections.IEnumerator PlayAfterDelay()
    {
        float t = 0f;
        while (t < delaySeconds)
        {
            // ต้องยืนอยู่ในโซนครบเวลาถึงจะเล่น
            if (requireStayFullDelay && !playerInside)
                yield break;

            t += Time.deltaTime;
            yield return null;
        }

        // เล่นเสียง
        if (source != null && source.clip != null)
        {
            source.Play();                   // หรือ source.PlayOneShot(source.clip);
            lastPlayTime = Time.time;
            hasPlayed = true;
        }
        pending = null;
    }
}
