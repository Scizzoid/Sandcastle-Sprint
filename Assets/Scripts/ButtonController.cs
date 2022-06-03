using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{

    public GameObject blackSquare;
    public AudioSource oceanNoise;

    public void NewGameButton()
    {
        StartCoroutine(FadeToBlack("NewGame", 0.5f));
        
    }

    public void QuitButton()
    {
        StartCoroutine(FadeToBlack("Quit", 0.5f));
    }

    public void NextLevelButton()
    {
        StartCoroutine(FadeToBlack("NextLevel", 0.75f));
    }

    public void RestartLevelButton()
    {
        StartCoroutine(FadeToBlack("RestartLevel", 1.0f));
    }

    public void MainMenuButton()
    {
        StartCoroutine(FadeToBlack("MainMenu", 0.5f));
    }

    private IEnumerator FadeToBlack(string buttonType, float fadeSpeed)
    {
        Color fadeColor = blackSquare.GetComponent<Image>().color;
        float fade;

        while (blackSquare.GetComponent<Image>().color.a < 1)
        {
            fade = fadeColor.a + Time.deltaTime * fadeSpeed;
            fadeColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fade);
            blackSquare.GetComponent<Image>().color = fadeColor;
            oceanNoise.volume = (1.0f - fade);
            yield return null;
        }

        if (buttonType == "Quit")
        {
            Application.Quit();
        }

        else if (buttonType == "NewGame" || buttonType == "NextLevel")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        else if (buttonType == "RestartLevel")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        else if (buttonType == "MainMenu")
        {
            SceneManager.LoadScene(0);
        }
    }
}
