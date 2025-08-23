using System;
using System.Collections.Generic;
using UnityEngine;

namespace Narrative
{
    [Serializable]
    public class NarrativeRoot
    {
        public List<SpeakerData> speakers = new();
        public List<BackgroundData> backgrounds = new();
        public List<SceneData> scenes = new();
        public string startScene; // id of first scene
    }

    [Serializable]
    public class SpeakerData
    {
        public string id;
        public string name;
        public string textColor = "#FFFFFFFF";
        public List<string> sprites = new();
        public string prefab; // Resources path
    }

    [Serializable]
    public class BackgroundData
    {
        public string id;
        public string sprite; // Resources path
    }

    [Serializable]
    public class SceneData
    {
        public string id;
        public SceneType type; // story | choice | ending
        public string background; // background id
        public List<SentenceData> sentences = new(); // story only
        public string next; // story -> next or ending -> null
        public List<ChoiceData> choices = new(); // choice only
    }

    public enum SceneType { story, choice, ending }

    [Serializable]
    public class SentenceData
    {
        public string speaker; // speaker id
        public string text;
        public List<ActionData> actions = new();
    }

    [Serializable]
    public class ActionData
    {
        public string speaker; // speaker id
        public ActionType type; // APPEAR, MOVE, DISAPPEAR, BOUNCE, SWITCH
        public int spriteIndex = 0;
        public float x = 0;
        public float y = 0;
        public float moveSpeed = 0f;
    }

    public enum ActionType { NONE, APPEAR, MOVE, DISAPPEAR, BOUNCE, SWITCH }

    [Serializable]
    public class ChoiceData
    {
        public string text;
        public string next; // scene id
    }
}
