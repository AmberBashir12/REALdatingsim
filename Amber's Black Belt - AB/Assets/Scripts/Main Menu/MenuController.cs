using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuController : MonoBehaviour
{
    public string gameScene;
    public AudioClip clickSound;
    public CanvasGroup fadePanel;
    public float fadeDuration = 3f;

    private void Start()
    {
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(false);
        }
    }

    public void NewGame()
    {
        PlayClickSound();
        StartCoroutine(FadeToBlackAndLoad());
    }

    private IEnumerator FadeToBlackAndLoad()
    {
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.alpha = 0f;
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                fadePanel.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }
            fadePanel.alpha = 1f;
        }
        SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        PlayClickSound();
        Application.Quit();
    }

    private void PlayClickSound()
    {
        if (clickSound != null)
        {
            AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
        }
    }
}

