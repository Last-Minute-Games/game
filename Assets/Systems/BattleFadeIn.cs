using UnityEngine;
using System.Collections;

public class BattleFadeIn : MonoBehaviour
{
    public ScreenFader screenFader;

    void Start()
    {
        if (screenFader != null)
            StartCoroutine(screenFader.FadeIn());
    }
}
