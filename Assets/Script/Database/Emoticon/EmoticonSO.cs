using UnityEngine;

[CreateAssetMenu(fileName = "New Database", menuName = "Database/EmoticonDatabase")]
public class EmoticonSO : ScriptableObject
{
    public string emoticonName;
    public Sprite emoticonSprite;
}
