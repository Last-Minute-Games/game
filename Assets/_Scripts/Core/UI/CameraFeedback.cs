using UnityEngine;
using DG.Tweening;

public class CameraFeedback : MonoBehaviour
{
    public static CameraFeedback Instance { get; private set; }

    [Header("Shake Settings")]
    public float damageShakeDuration = 0.25f;
    public float damageShakeStrength = 0.3f;
    public int damageShakeVibrato = 20;

    public float blockShakeDuration = 0.15f;
    public float blockShakeStrength = 0.15f;

    public float healShakeDuration = 0.1f;
    public float healShakeStrength = 0.1f;

    private Camera cam;
    private Vector3 originalPos;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        cam = Camera.main;
        originalPos = cam.transform.localPosition;
    }

    public void PlayDamageShake()
    {
        if (cam == null) return;
        cam.transform.DOShakePosition(damageShakeDuration, damageShakeStrength, damageShakeVibrato, 90, false, true)
            .OnComplete(() => cam.transform.localPosition = originalPos);
    }

    public void PlayBlockShake()
    {
        if (cam == null) return;
        cam.transform.DOShakePosition(blockShakeDuration, blockShakeStrength, 15, 90, false, true)
            .OnComplete(() => cam.transform.localPosition = originalPos);
    }

    public void PlayHealShake()
    {
        if (cam == null) return;
        cam.transform.DOShakePosition(healShakeDuration, healShakeStrength, 10, 90, false, true)
            .OnComplete(() => cam.transform.localPosition = originalPos);
    }
}
