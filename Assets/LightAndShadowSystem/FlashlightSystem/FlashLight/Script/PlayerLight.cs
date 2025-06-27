using ShadowMaze.EnemyCharactor;
using ShadowMaze.MyCharacterController;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLight : MonoBehaviour
{
    [SerializeField] private PlayerStats _playerStats;
    [SerializeField] private float reducedBattery = 1;
    [SerializeField] private string[] targetTags;
    [SerializeField] private LayerMask targetLayerMask;
    [SerializeField] private LayerMask occlusionLayerMask;
    [SerializeField] private float OnOffSpeed = 2f;

    [SerializeField] private float flickerSpeed = 5f;             // ความเร็วในการกระพริบเมื่อแบตใกล้หมด
    [SerializeField] private float baseFlickerSpeed = 1f;         // ความเร็วปกติในการสั่น
    [SerializeField] private float minIntensity = 0.6f;           // ค่าต่ำสุดของการสว่าง
    [SerializeField] private float maxIntensity = 1f;             // ค่าสูงสุดของการสว่าง
    [SerializeField] private float lowBatteryThreshold = 20f;    // แบตเตอรี่ที่ต่ำกว่าถือว่าใกล้หมด (20%)

    public GameObject FlashLight;

    private Light _light;
    private bool _flashLightOn = false;
    private float _flickerTimer;

    private void Awake()
    {
        if (FlashLight.TryGetComponent(out Light lightComponent)) _light = lightComponent;
        else Debug.LogError("FlashLight GameObject has no Light component.");
    }

    private void Update()
    {
        if (!_light) return;
        if (_playerStats.Battery <= 0f) {
            if (_light.intensity != 0) _light.intensity = Mathf.MoveTowards(_light.intensity, 0f, OnOffSpeed * Time.deltaTime);
            else _flashLightOn = false;
        }

        if (_flashLightOn) _playerStats.Battery = Mathf.Max(0f, _playerStats.Battery - reducedBattery * Time.deltaTime);

        float targetIntensity = 0f;
        if (_flashLightOn)
        {
            float batteryPercent = _playerStats.Battery;
            float flickerSpeedNow = batteryPercent < lowBatteryThreshold ? flickerSpeed : baseFlickerSpeed;

            _flickerTimer += Time.deltaTime * flickerSpeedNow;
            float flicker = Mathf.PingPong(_flickerTimer, 1f);
            targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, flicker);

            foreach (GameObject GO in GetObjectsInFlashlightCone())
            {
                if (GO.CompareTag("Enemy"))
                {
                    if (GO.TryGetComponent<SimpleEnemy>(out SimpleEnemy se))
                    {
                        se.hit?.Invoke();
                    }
                    else if (GO.transform.parent.CompareTag("Enemy"))
                    {
                        if (GO.transform.parent.TryGetComponent<SimpleEnemy>(out SimpleEnemy pse))
                        {
                            pse.hit?.Invoke();
                        }
                    }
                }
            }
        }

        if (_playerStats.Battery > 0f) _light.intensity = targetIntensity;
    }
    public void On()
    {
        if (_playerStats.Battery > 0f)
        {
            _flashLightOn = true;
            _flickerTimer = 0f;
        }
    }

    public void Off() => _flashLightOn = false;
    public void Toggle()
    {
        if (_flashLightOn) Off();
        else On();
    }

    public List<GameObject> GetObjectsInFlashlightCone()
    {
        if (!_light || !_flashLightOn) return null;

        List<GameObject> detectedObjects = new List<GameObject>();

        Vector3 origin = FlashLight.transform.position;
        Vector3 forward = FlashLight.transform.forward;
        float range = _light.range;
        float halfAngle = _light.spotAngle * 0.5f;

        Collider[] hits = Physics.OverlapSphere(origin, range, targetLayerMask);

        foreach (var hit in hits)
        {
            Transform target = hit.transform;
            Vector3 dirToTarget = (target.position - origin).normalized;
            float angle = Vector3.Angle(forward, dirToTarget);
            float distance = Vector3.Distance(origin, target.position);

            // อยู่ในกรวย
            if (angle <= halfAngle - 1 && distance <= range)
            {
                // ตรวจว่า tag ตรงหรือไม่
                bool tagMatch = false;
                foreach (var tag in targetTags)
                {
                    if (hit.CompareTag(tag))
                    {
                        tagMatch = true;
                        break;
                    }
                }

                if (!tagMatch) continue;

                // ตรวจว่าไม่ถูกบัง: Raycast ต้องโดนเป้าหมายก่อนวัตถุอื่น
                if (Physics.Raycast(origin, dirToTarget, out RaycastHit rayHit, distance, targetLayerMask | occlusionLayerMask))
                {
                    if (rayHit.collider == hit)  // ไม่มีอะไรบัง
                    {
                        detectedObjects.Add(hit.gameObject);
                    }
                }
            }
        }

        return detectedObjects;
    }
}
