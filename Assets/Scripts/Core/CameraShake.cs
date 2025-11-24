using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;

    private Vector3 originalPos;
    private Coroutine bobRoutine;
    private Coroutine shakeRoutine;
    
    // Separate offset tracking for bob and shake
    private Vector3 bobOffset;
    private Vector3 shakeOffset;

    private void Awake()
    {
        instance = this;
        originalPos = transform.localPosition;
    }

    public static void Shake(float duration = 0.2f, float magnitude = 0.2f)
    {
        if (instance == null)
        {
            Debug.LogWarning("CameraShake: No instance in scene. Add CameraShake to your Main Camera.");
            return;
        }

        // Start a new shake without interrupting the bobbing
        instance.StartCoroutine(instance.ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            shakeOffset = new Vector3(x, y, 0f);
            UpdateCameraPosition();

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset shake offset
        shakeOffset = Vector3.zero;
        UpdateCameraPosition();
    }

    public static void StartBobbing(float bobSpeed = 0.1f, float bobAmount = 0.05f)
    {
        if (instance == null)
        {
            Debug.LogWarning("CameraShake: No instance in scene. Add CameraShake to your Main Camera.");
            return;
        }

        // Stop existing bob if any
        if (instance.bobRoutine != null)
            instance.StopCoroutine(instance.bobRoutine);

        instance.bobRoutine = instance.StartCoroutine(instance.BobRoutine(bobSpeed, bobAmount));
    }

    public static void StopBobbing()
    {
        if (instance == null) return;

        if (instance.bobRoutine != null)
            instance.StopCoroutine(instance.bobRoutine);

        instance.bobOffset = Vector3.zero;
        instance.UpdateCameraPosition();
        instance.bobRoutine = null;
    }

    private IEnumerator BobRoutine(float bobSpeed, float bobAmount)
    {
        float timeOffset = 0f;

        while (true)
        {
            float yOffset = Mathf.Sin(timeOffset * bobSpeed) * bobAmount;
            bobOffset = new Vector3(0f, yOffset, 0f);
            UpdateCameraPosition();

            timeOffset += Time.deltaTime;
            yield return null;
        }
    }

    // Combine both bobbing and shaking offsets
    private void UpdateCameraPosition()
    {
        transform.localPosition = originalPos + bobOffset + shakeOffset;
    }
}
