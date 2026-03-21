using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StoryScene.Sentence))]
public class StorySceneSentenceDrawer : PropertyDrawer
{
    private const float Spacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty sentenceTypeProp = property.FindPropertyRelative("sentenceType");
        SerializedProperty speakerProp = property.FindPropertyRelative("speaker");
        SerializedProperty textProp = property.FindPropertyRelative("text");
        SerializedProperty actionsProp = property.FindPropertyRelative("actions");
        SerializedProperty musicProp = property.FindPropertyRelative("music");
        SerializedProperty music2Prop = property.FindPropertyRelative("music2");
        SerializedProperty soundProp = property.FindPropertyRelative("sound");
        SerializedProperty choiceProp = property.FindPropertyRelative("choice");
        SerializedProperty endingSceneProp = property.FindPropertyRelative("endingScene");

        float y = position.y;

        Rect foldoutRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
        y += EditorGUIUtility.singleLineHeight + Spacing;

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            DrawProperty(ref y, position, sentenceTypeProp);

            bool isChoice = sentenceTypeProp.enumValueIndex == (int)StoryScene.Sentence.SentenceType.CHOICE;
            bool isEnding = sentenceTypeProp.enumValueIndex == (int)StoryScene.Sentence.SentenceType.ENDING;
            if (isChoice)
            {
                SerializedProperty promptProp = choiceProp.FindPropertyRelative("prompt");
                SerializedProperty optionsProp = choiceProp.FindPropertyRelative("options");

                DrawProperty(ref y, position, promptProp);
                DrawProperty(ref y, position, optionsProp, true);
            }
            else if (isEnding)
            {
                EditorGUI.PropertyField(
                    new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight),
                    endingSceneProp,
                    new GUIContent("Ending Scene (Input)")
                );
                y += EditorGUIUtility.singleLineHeight + Spacing;
            }
            else
            {
                DrawProperty(ref y, position, speakerProp);
                DrawProperty(ref y, position, textProp, true);
                DrawProperty(ref y, position, actionsProp, true);
                DrawProperty(ref y, position, musicProp);
                DrawProperty(ref y, position, music2Prop);
                DrawProperty(ref y, position, soundProp);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty sentenceTypeProp = property.FindPropertyRelative("sentenceType");
        SerializedProperty speakerProp = property.FindPropertyRelative("speaker");
        SerializedProperty textProp = property.FindPropertyRelative("text");
        SerializedProperty actionsProp = property.FindPropertyRelative("actions");
        SerializedProperty musicProp = property.FindPropertyRelative("music");
        SerializedProperty music2Prop = property.FindPropertyRelative("music2");
        SerializedProperty soundProp = property.FindPropertyRelative("sound");
        SerializedProperty choiceProp = property.FindPropertyRelative("choice");
        SerializedProperty endingSceneProp = property.FindPropertyRelative("endingScene");

        float height = EditorGUIUtility.singleLineHeight;

        if (!property.isExpanded)
        {
            return height;
        }

        height += Spacing;
        height += EditorGUI.GetPropertyHeight(sentenceTypeProp, true) + Spacing;

        bool isChoice = sentenceTypeProp.enumValueIndex == (int)StoryScene.Sentence.SentenceType.CHOICE;
        bool isEnding = sentenceTypeProp.enumValueIndex == (int)StoryScene.Sentence.SentenceType.ENDING;
        if (isChoice)
        {
            SerializedProperty promptProp = choiceProp.FindPropertyRelative("prompt");
            SerializedProperty optionsProp = choiceProp.FindPropertyRelative("options");

            height += EditorGUI.GetPropertyHeight(promptProp, true) + Spacing;
            height += EditorGUI.GetPropertyHeight(optionsProp, true) + Spacing;
        }
        else if (isEnding)
        {
            height += EditorGUI.GetPropertyHeight(endingSceneProp, true) + Spacing;
        }
        else
        {
            height += EditorGUI.GetPropertyHeight(speakerProp, true) + Spacing;
            height += EditorGUI.GetPropertyHeight(textProp, true) + Spacing;
            height += EditorGUI.GetPropertyHeight(actionsProp, true) + Spacing;
            height += EditorGUI.GetPropertyHeight(musicProp, true) + Spacing;
            height += EditorGUI.GetPropertyHeight(music2Prop, true) + Spacing;
            height += EditorGUI.GetPropertyHeight(soundProp, true) + Spacing;
        }

        return height;
    }

    private static void DrawProperty(ref float y, Rect totalRect, SerializedProperty property, bool includeChildren = false)
    {
        float propertyHeight = EditorGUI.GetPropertyHeight(property, includeChildren);
        Rect propertyRect = new Rect(totalRect.x, y, totalRect.width, propertyHeight);
        EditorGUI.PropertyField(propertyRect, property, includeChildren);
        y += propertyHeight + Spacing;
    }
}

[CustomPropertyDrawer(typeof(StoryScene.FollowUpSentence))]
public class StorySceneFollowUpSentenceDrawer : PropertyDrawer
{
    private const float Spacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty typeProp = property.FindPropertyRelative("followUpType");
        SerializedProperty speakerProp = property.FindPropertyRelative("speaker");
        SerializedProperty textProp = property.FindPropertyRelative("text");
        SerializedProperty actionsProp = property.FindPropertyRelative("actions");
        SerializedProperty musicProp = property.FindPropertyRelative("music");
        SerializedProperty soundProp = property.FindPropertyRelative("sound");
        SerializedProperty choiceProp = property.FindPropertyRelative("choice");
        SerializedProperty endingSceneProp = property.FindPropertyRelative("endingScene");

        float y = position.y;
        Rect foldoutRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
        y += EditorGUIUtility.singleLineHeight + Spacing;

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            DrawProperty(ref y, position, typeProp);

            bool isChoice = typeProp.enumValueIndex == (int)StoryScene.FollowUpSentence.FollowUpType.CHOICE;
            bool isEnding = typeProp.enumValueIndex == (int)StoryScene.FollowUpSentence.FollowUpType.ENDING;
            if (isChoice)
            {
                SerializedProperty promptProp = choiceProp.FindPropertyRelative("prompt");
                SerializedProperty optionsProp = choiceProp.FindPropertyRelative("options");
                DrawProperty(ref y, position, promptProp);
                DrawProperty(ref y, position, optionsProp, true);
            }
            else if (isEnding)
            {
                EditorGUI.PropertyField(
                    new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight),
                    endingSceneProp,
                    new GUIContent("Ending Scene (Input)")
                );
                y += EditorGUIUtility.singleLineHeight + Spacing;
            }
            else
            {
                DrawProperty(ref y, position, speakerProp);
                DrawProperty(ref y, position, textProp, true);
                DrawProperty(ref y, position, actionsProp, true);
                DrawProperty(ref y, position, musicProp);
                DrawProperty(ref y, position, soundProp);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty typeProp = property.FindPropertyRelative("followUpType");
        SerializedProperty speakerProp = property.FindPropertyRelative("speaker");
        SerializedProperty textProp = property.FindPropertyRelative("text");
        SerializedProperty actionsProp = property.FindPropertyRelative("actions");
        SerializedProperty musicProp = property.FindPropertyRelative("music");
        SerializedProperty soundProp = property.FindPropertyRelative("sound");
        SerializedProperty choiceProp = property.FindPropertyRelative("choice");
        SerializedProperty endingSceneProp = property.FindPropertyRelative("endingScene");

        float height = EditorGUIUtility.singleLineHeight;
        if (!property.isExpanded)
        {
            return height;
        }

        height += Spacing;
        height += EditorGUI.GetPropertyHeight(typeProp, true) + Spacing;

        bool isChoice = typeProp.enumValueIndex == (int)StoryScene.FollowUpSentence.FollowUpType.CHOICE;
        bool isEnding = typeProp.enumValueIndex == (int)StoryScene.FollowUpSentence.FollowUpType.ENDING;
        if (isChoice)
        {
            SerializedProperty promptProp = choiceProp.FindPropertyRelative("prompt");
            SerializedProperty optionsProp = choiceProp.FindPropertyRelative("options");

            height += EditorGUI.GetPropertyHeight(promptProp, true) + Spacing;
            height += EditorGUI.GetPropertyHeight(optionsProp, true) + Spacing;
        }
        else if (isEnding)
        {
            height += EditorGUI.GetPropertyHeight(endingSceneProp, true) + Spacing;
        }
        else
        {
            height += EditorGUI.GetPropertyHeight(speakerProp, true) + Spacing;
            height += EditorGUI.GetPropertyHeight(textProp, true) + Spacing;
            height += EditorGUI.GetPropertyHeight(actionsProp, true) + Spacing;
            height += EditorGUI.GetPropertyHeight(musicProp, true) + Spacing;
            height += EditorGUI.GetPropertyHeight(soundProp, true) + Spacing;
        }

        return height;
    }

    private static void DrawProperty(ref float y, Rect totalRect, SerializedProperty property, bool includeChildren = false)
    {
        float propertyHeight = EditorGUI.GetPropertyHeight(property, includeChildren);
        Rect propertyRect = new Rect(totalRect.x, y, totalRect.width, propertyHeight);
        EditorGUI.PropertyField(propertyRect, property, includeChildren);
        y += propertyHeight + Spacing;
    }
}

[CustomPropertyDrawer(typeof(StoryScene.FollowUpLine))]
public class StorySceneFollowUpLineDrawer : PropertyDrawer
{
    private const float Spacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty typeProp = property.FindPropertyRelative("lineType");
        SerializedProperty speakerProp = property.FindPropertyRelative("speaker");
        SerializedProperty textProp = property.FindPropertyRelative("text");
        SerializedProperty actionsProp = property.FindPropertyRelative("actions");
        SerializedProperty musicProp = property.FindPropertyRelative("music");
        SerializedProperty music2Prop = property.FindPropertyRelative("music2");
        SerializedProperty soundProp = property.FindPropertyRelative("sound");
        SerializedProperty endingSceneProp = property.FindPropertyRelative("endingScene");

        float y = position.y;
        Rect foldoutRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
        y += EditorGUIUtility.singleLineHeight + Spacing;

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            EditorGUI.PropertyField(
                new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(typeProp, true)),
                typeProp,
                new GUIContent("Sentence Type"),
                true
            );
            y += EditorGUI.GetPropertyHeight(typeProp, true) + Spacing;

            bool isEnding = typeProp.enumValueIndex == (int)StoryScene.FollowUpLine.LineType.ENDING;
            if (isEnding)
            {
                EditorGUI.PropertyField(
                    new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight),
                    endingSceneProp,
                    new GUIContent("Ending Scene (Input)")
                );
                y += EditorGUIUtility.singleLineHeight + Spacing;
            }
            else
            {
                DrawProperty(ref y, position, speakerProp);
                DrawProperty(ref y, position, textProp, true);
                DrawProperty(ref y, position, actionsProp, true);
                DrawProperty(ref y, position, musicProp);
                DrawProperty(ref y, position, music2Prop);
                DrawProperty(ref y, position, soundProp);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty typeProp = property.FindPropertyRelative("lineType");
        SerializedProperty speakerProp = property.FindPropertyRelative("speaker");
        SerializedProperty textProp = property.FindPropertyRelative("text");
        SerializedProperty actionsProp = property.FindPropertyRelative("actions");
        SerializedProperty musicProp = property.FindPropertyRelative("music");
        SerializedProperty music2Prop = property.FindPropertyRelative("music2");
        SerializedProperty soundProp = property.FindPropertyRelative("sound");
        SerializedProperty endingSceneProp = property.FindPropertyRelative("endingScene");

        float height = EditorGUIUtility.singleLineHeight;
        if (!property.isExpanded)
        {
            return height;
        }

        height += Spacing;
        height += EditorGUI.GetPropertyHeight(typeProp, true) + Spacing;

        bool isEnding = typeProp.enumValueIndex == (int)StoryScene.FollowUpLine.LineType.ENDING;
        if (isEnding)
        {
            height += EditorGUI.GetPropertyHeight(endingSceneProp, true) + Spacing;
        }
        else
        {
            height += EditorGUI.GetPropertyHeight(speakerProp, true) + Spacing;
            height += EditorGUI.GetPropertyHeight(textProp, true) + Spacing;
            height += EditorGUI.GetPropertyHeight(actionsProp, true) + Spacing;
            height += EditorGUI.GetPropertyHeight(musicProp, true) + Spacing;
            height += EditorGUI.GetPropertyHeight(music2Prop, true) + Spacing;
            height += EditorGUI.GetPropertyHeight(soundProp, true) + Spacing;
        }

        return height;
    }

    private static void DrawProperty(ref float y, Rect totalRect, SerializedProperty property, bool includeChildren = false)
    {
        float propertyHeight = EditorGUI.GetPropertyHeight(property, includeChildren);
        Rect propertyRect = new Rect(totalRect.x, y, totalRect.width, propertyHeight);
        EditorGUI.PropertyField(propertyRect, property, includeChildren);
        y += propertyHeight + Spacing;
    }
}
