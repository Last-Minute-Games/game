using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;
    public GameObject floatingTextPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SpawnText(Vector3 position, string text, Color color)
    {
        if (floatingTextPrefab == null) return;

        GameObject obj = Instantiate(floatingTextPrefab, position, Quaternion.identity);
        TMP_Text tmp = obj.GetComponent<TMP_Text>();
        tmp.text = text;
        tmp.color = color;

        tmp.transform.DOMoveY(position.y + 0.8f, 0.8f);
        tmp.DOFade(0, 0.8f).OnComplete(() => Destroy(obj, 0.8f));
    }
}
