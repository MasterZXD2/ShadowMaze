using System.Collections.Generic;
using UnityEngine;

public class PlayerLight : MonoBehaviour
{
    public GameObject FlashLight;

    private Light _light;
    private bool _flashLightOn = false;

    [SerializeField] private string[] targetTags;
    [SerializeField] private LayerMask targetLayerMask;
    [SerializeField] private LayerMask occlusionLayerMask;
    [SerializeField] private float OnOffSpeed = 2f;

    private void Awake()
    {
        if (FlashLight.TryGetComponent(out Light lightComponent)) _light = lightComponent;
        else Debug.LogError("FlashLight GameObject has no Light component.");
    }

    private void Update()
    {
        if (!_light) return;

        float targetIntensity = _flashLightOn ? 1f : 0f;
        _light.intensity = Mathf.MoveTowards(_light.intensity, targetIntensity, OnOffSpeed * Time.deltaTime);
    }

    public void On() => _flashLightOn = true;
    public void Off() => _flashLightOn = false;
    public void Toggle() => _flashLightOn = !_flashLightOn;

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
            if (angle <= halfAngle && distance <= range)
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
