using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Speaker", menuName = "Data/New Speaker")]
public class Speaker : ScriptableObject
{
    public string speakerName;
    public Color textColor;
    public List<Sprite> sprites;
    public SpriteController prefab;
}
