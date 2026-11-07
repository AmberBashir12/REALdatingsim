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

    public void SetVisible(bool visible)
    {
        if (root != null)
        {
            root.SetActive(visible);
        }
    }


    public void OnClickReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }
}
