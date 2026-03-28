using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
  

public class ChooseLabelController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler 
{

    public Color defaultColor = Color.white;
    public Color hoverColor = new Color(1f, 1f, 1f, 0.8f);
    private StoryScene scene;
    private TextMeshProUGUI textMesh;
    private ChooseController controller;
    private ChooseScene chooseScene;
    private string labelText;
    private bool isInlineChoice;
    private int inlineChoiceIndex = -1;

    // Start is called before the first frame update
    void Awake()
    {
        // Get and enable TextMeshPro component
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh != null)
        {
            textMesh.enabled = true;
            textMesh.color = defaultColor;
        }

        // Enable this script
        enabled = true;
    }

    public float GetHeight()
    {
        return textMesh.rectTransform.sizeDelta.y * textMesh.rectTransform.localScale.y;
    }

    public void Setup(ChooseScene.ChooseLabel label, ChooseController controller, float y)
    {
        isInlineChoice = false;
        inlineChoiceIndex = -1;
        scene = label.nextScene;
        labelText = label.text;
        if (textMesh != null)
        {
            textMesh.text = TextTemplate.Resolve(label.text);
            textMesh.enabled = true; // Ensure TextMeshPro is enabled when setting up
        }
        this.controller = controller;

        Vector3 position = textMesh.rectTransform.localPosition;
        position.y = y;
        textMesh.rectTransform.localPosition = position;
    }

    public void SetupInline(string text, ChooseController controller, float y, int optionIndex)
    {
        isInlineChoice = true;
        inlineChoiceIndex = optionIndex;
        chooseScene = null;
        scene = null;
        labelText = text;
        this.controller = controller;

        if (textMesh != null)
        {
            textMesh.text = TextTemplate.Resolve(text);
            textMesh.enabled = true;
        }

        Vector3 position = textMesh.rectTransform.localPosition;
        position.y = y;
        textMesh.rectTransform.localPosition = position;
    }





    public void SetChooseScene(ChooseScene chooseScene)
    {
        this.chooseScene = chooseScene;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (enabled && controller != null)
        {
            if (isInlineChoice)
            {
                controller.PerformInlineChoose(inlineChoiceIndex);
                return;
            }

            ChooseScene.ChoiceResult result = new ChooseScene.ChoiceResult
            {
                nextScene = scene
            };
            
            if (chooseScene != null && !string.IsNullOrEmpty(labelText))
            {
                // Use ChooseScene.TryGetChoiceResult to handle choice key unlocking.
                if (!chooseScene.TryGetChoiceResult(labelText, out result))
                {
                    result.nextScene = scene;
                }
            }
            
            controller.PerformChoose(result);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (textMesh != null)
        {
            textMesh.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (textMesh != null)
        {
            textMesh.color = defaultColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
