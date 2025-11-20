using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;

    private Vector3 originalPos;
    private Coroutine shakeRoutine;

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

        if (instance.shakeRoutine != null)
            instance.StopCoroutine(instance.shakeRoutine);

        instance.shakeRoutine = instance.StartCoroutine(instance.ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        shakeRoutine = null;
    }
}
