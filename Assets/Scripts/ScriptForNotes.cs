using UnityEngine;

[CreateAssetMenu(fileName = "NewNote", menuName = "Tools/Note")]
public class Note : ScriptableObject
{
    [TextArea(5, 10)]
    public string text;
}
