using UnityEditor;

[CustomEditor(typeof(EndingScene))]
public class EndingSceneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        SerializedProperty backgroundProp = serializedObject.FindProperty("background");
        SerializedProperty sentencesProp = serializedObject.FindProperty("sentences");

        if (backgroundProp != null)
        {
            EditorGUILayout.PropertyField(backgroundProp);
        }

        if (sentencesProp != null)
        {
            EditorGUILayout.PropertyField(sentencesProp, true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
