using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelFadeIn : MonoBehaviour
{
    public GameObject blackSquare;
    public AudioSource oceanNoise;

    // Start is called before the first frame update
    void Start()
    {
        blackSquare.SetActive(true);
        StartCoroutine(levelFadeIn());
    }

    private IEnumerator levelFadeIn()
    {
        Color fadeColor = blackSquare.GetComponent<Image>().color;
        float fade;

        // Fade in Scene & Audio
        while (blackSquare.GetComponent<Image>().color.a > 0)
        {
            fade = fadeColor.a - Time.deltaTime * 0.75f;
            fadeColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fade);
            blackSquare.GetComponent<Image>().color = fadeColor;
            oceanNoise.volume = (1.0f - fade);
            yield return null;
        }
    }
}
