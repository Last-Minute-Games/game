using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;

    [Header("Prefab Reference")]
    public GameObject floatingTextPrefab; // assign a prefab with TMP_Text

    [Header("Settings")]
    public float floatDistance = 1.5f;
    public float duration = 1.0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SpawnText(Vector3 position, string text, Color color)
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogWarning("FloatingTextManager: No prefab assigned!");
            return;
        }

        GameObject obj = Instantiate(floatingTextPrefab, position, Quaternion.identity);
        TMP_Text tmp = obj.GetComponentInChildren<TMP_Text>();

        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
        }

        StartCoroutine(FloatAndFade(obj));
    }

    private IEnumerator FloatAndFade(GameObject obj)
    {
        Vector3 startPos = obj.transform.position;
        Vector3 endPos = startPos + Vector3.up * floatDistance;
        TMP_Text tmp = obj.GetComponentInChildren<TMP_Text>();
        float timer = 0f;

        while (timer < duration)
        {
            float t = timer / duration;
            obj.transform.position = Vector3.Lerp(startPos, endPos, t);

            if (tmp != null)
            {
                Color c = tmp.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                tmp.color = c;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(obj);
    }
}
