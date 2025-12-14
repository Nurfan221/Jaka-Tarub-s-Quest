using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EmoticonDatabase", menuName = "Database/Emoticon Database")]
public class EmoticonDatabaseSO : ScriptableObject
{
    public List<EmoticonTemplate> emoticonDatabase;
}