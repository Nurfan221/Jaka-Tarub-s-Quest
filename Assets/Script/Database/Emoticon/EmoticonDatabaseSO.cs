using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EmoticonDatabase", menuName = "Database/Emoticon Database")]
public class EmoticonDatabaseSO : ScriptableObject
{
    public List<EmoticonTemplate> emoticonDatabase;
}