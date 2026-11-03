using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSaveMenu : MonoBehaviour
{
    public SaveSceneCatalog catalog;
    public GameObject slotCanvas;
    public Image[] slotImages = new Image[SceneSaveSlots.Count];
    public Button openButton;
    public Button closeButton;
    public PauseMenu pauseMenu;
    public GameController gameController;
    public string gameplayScene = "SampleScene";

    private readonly Sprite[] emptyImages = new Sprite[SceneSaveSlots.Count];
    private TMP_Text status;
    private bool loading;

    private void Start()
    {
        if (catalog == null || slotCanvas == null || openButton == null
            || closeButton == null || slotImages == null || slotImages.Length != SceneSaveSlots.Count)
        {
            Debug.LogError("SceneSaveMenu is missing its canvas, catalog, buttons, or four slot images.");
            enabled = false;
            return;
        }

        for (int i = 0; i < slotImages.Length; i++)
        {
            Image image = slotImages[i];
            if (image == null)
            {
                Debug.LogError($"SceneSaveMenu slot {i + 1} is missing its image.");
                enabled = false;
                return;
            }

            emptyImages[i] = image.sprite;
            Button button = image.GetComponent<Button>();
            if (button == null)
            {
                button = image.gameObject.AddComponent<Button>();
            }
            button.targetGraphic = image;
            int slot = i;
            button.onClick.AddListener(() => SelectSlot(slot));
        }

        CreateInputBlocker();
        CreateStatusText();
        openButton.onClick.AddListener(Open);
        closeButton.onClick.AddListener(Close);
        slotCanvas.SetActive(false);
    }

    private void CreateInputBlocker()
    {
        GameObject blocker = new GameObject("SaveSlotBackdrop", typeof(RectTransform), typeof(Image));
        blocker.transform.SetParent(slotCanvas.transform, false);
        blocker.transform.SetAsFirstSibling();
        blocker.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
        RectTransform rect = blocker.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void CreateStatusText()
    {
        GameObject label = new GameObject("SaveSlotStatus", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(slotCanvas.transform, false);
        status = label.GetComponent<TextMeshProUGUI>();
        status.fontSize = 24;
        status.alignment = TextAlignmentOptions.Center;
        status.raycastTarget = false;
        RectTransform rect = label.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.02f);
        rect.anchorMax = new Vector2(0.9f, 0.12f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public void Open()
    {
        if (pauseMenu != null)
        {
            pauseMenu.Pause();
        }
        slotCanvas.SetActive(true);
        status.text = gameController != null ? "Choose a slot to save." : "Choose a saved scene to load.";
        Refresh();
    }

    public void Close()
    {
        slotCanvas.SetActive(false);
        if (pauseMenu != null)
        {
            pauseMenu.Resume();
        }
    }

    private void Refresh()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            bool occupied = SceneSaveSlots.TryRead(i, out SceneSlotData data);
            SaveSceneCatalog.Entry entry = occupied ? catalog.Find(data.sceneId) : null;
            SaveSceneCatalog.Entry background = occupied ? catalog.Find(data.backgroundSceneId) : null;
            slotImages[i].sprite = background != null && background.Background != null
                ? background.Background : emptyImages[i];
            slotImages[i].GetComponent<Button>().interactable = gameController != null || entry != null;
            TMP_Text label = slotImages[i].GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = entry != null ? entry.scene.name
                    : occupied ? $"Slot {i + 1} - Scene unavailable" : $"Save Slot {i + 1} - Empty";
            }
            if (occupied && entry == null)
            {
                Debug.LogError($"Slot {i + 1} references a scene that is missing from the save catalog.");
            }
        }
    }

    private void SelectSlot(int slot)
    {
        if (loading)
        {
            return;
        }

        if (gameController != null)
        {
            if (gameController.IsTransitioning)
            {
                status.text = "Wait for the scene transition to finish before saving.";
                return;
            }

            SaveSceneCatalog.Entry entry = catalog.Find(gameController.currentScene);
            if (entry == null)
            {
                Debug.LogError($"Current scene '{gameController.currentScene?.name}' is missing from the save catalog.");
                status.text = "Cannot save: scene is missing from the catalog.";
                return;
            }

            Sprite background = entry.Background;
            if (background == null && gameController.backgroundController != null)
            {
                background = gameController.backgroundController.CurrentBackground;
            }
            SaveSceneCatalog.Entry backgroundEntry = background != null ? catalog.FindBackground(background) : null;
            SceneSlotData data = new SceneSlotData
            {
                sceneId = entry.id,
                backgroundSceneId = backgroundEntry != null ? backgroundEntry.id : entry.id,
                playerName = PlayerProfile.Name,
                characterChoice = PlayerProfile.CharacterChoice,
                choices = GameStateManager.Instance != null
                    ? GameStateManager.Instance.GetUnlockedChoices() : new string[0]
            };
            if (SceneSaveSlots.Write(slot, data))
            {
                Refresh();
                status.text = $"Saved to slot {slot + 1}.";
            }
            else
            {
                status.text = "Save failed. Check the Console.";
            }
            return;
        }

        if (!SceneSaveSlots.TryRead(slot, out SceneSlotData saved) || catalog.Find(saved.sceneId) == null)
        {
            Debug.LogError($"Slot {slot + 1} is empty, invalid, or references a removed scene.");
            status.text = "This save cannot be loaded.";
            return;
        }
        if (!Application.CanStreamedLevelBeLoaded(gameplayScene))
        {
            Debug.LogError($"Gameplay scene '{gameplayScene}' is missing from Build Settings.");
            status.text = "Gameplay scene is not available.";
            return;
        }

        loading = true;
        SceneSaveSlots.QueueLoad(saved);
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplayScene);
    }
}
