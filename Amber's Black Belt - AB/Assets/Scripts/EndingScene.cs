using UnityEngine;

[CreateAssetMenu(fileName = "NewEndingScene", menuName = "Data/New Ending Scene")]
public class EndingScene : StoryScene
{
    [SerializeField, HideInInspector] private string endingId;

    public bool IsObtained => !string.IsNullOrEmpty(endingId)
        && PlayerPrefs.GetInt("EndingUnlock." + endingId, 0) == 1;

    public void MarkObtained()
    {
        if (string.IsNullOrEmpty(endingId))
        {
            return;
        }

        if (!IsObtained)
        {
            PlayerPrefs.SetInt("EndingUnlock." + endingId, 1);
            PlayerPrefs.Save();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        string path = UnityEditor.AssetDatabase.GetAssetPath(this);
        if (!string.IsNullOrEmpty(path))
        {
            string id = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            if (endingId != id)
            {
                endingId = id;
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
    }
#endif
}
