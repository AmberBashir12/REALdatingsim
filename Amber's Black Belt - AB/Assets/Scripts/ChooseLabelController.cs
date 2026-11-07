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


    void Awake()
    {

        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh != null)
        {
            textMesh.enabled = true;
            textMesh.color = defaultColor;
        }


        enabled = true;
    }

    public void Setup(ChooseScene.ChooseLabel label, ChooseController controller, float y)
    {
        isInlineChoice = false;
        inlineChoiceIndex = -1;
        scene = label.nextScene;
        labelText = label.text;
        if (textMesh != null)
        {
            textMesh.text = PlayerProfile.ResolveText(label.text);
            textMesh.enabled = true;
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
            textMesh.text = PlayerProfile.ResolveText(text);
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
}
