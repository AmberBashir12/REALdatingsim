using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingReturnToMenuButton : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private GameObject root;

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }
    }

    public void SetMainMenuSceneName(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            mainMenuSceneName = sceneName;
        }
    }

    public void SetVisible(bool visible)
    {
        if (root != null)
        {
            root.SetActive(visible);
        }
    }

    // Hook this up to your Unity UI Button's OnClick.
    public void OnClickReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }
}
