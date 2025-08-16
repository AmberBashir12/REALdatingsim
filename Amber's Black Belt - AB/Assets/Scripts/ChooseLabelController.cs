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
        scene = label.nextScene;
        if (textMesh != null)
        {
            textMesh.text = label.text;
            textMesh.enabled = true; // Ensure TextMeshPro is enabled when setting up
        }
        this.controller = controller;

        Vector3 position = textMesh.rectTransform.localPosition;
        position.y = y;
        textMesh.rectTransform.localPosition = position;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (enabled && controller != null)
        {
            controller.PerformChoose(scene);
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
