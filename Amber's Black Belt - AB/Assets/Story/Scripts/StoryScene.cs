using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewStoryScene", menuName = "Data/New Story Scene")]
[System.Serializable]
public class StoryScene : GameScene
{
    public List<Sentence> sentences;
    public Sprite background;
    public GameScene nextScene;

    [System.Serializable]
    public struct Sentence
    {
        public Speaker speaker;
        public string text;
        public List<Action> actions;

        [System.Serializable]
        public struct Action
        {
            public Speaker speaker;
            public int spriteIndex;
            public Type actionType;
            public Vector2 coords;
            public float moveSpeed;

            [System.Serializable]

            public enum Type
            {
                NONE, APPEAR, MOVE, DISAPPEAR, BOUNCE
            }
        }
    }
}

public class GameScene : ScriptableObject { }


