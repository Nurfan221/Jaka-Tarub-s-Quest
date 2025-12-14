using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteDatabaseSO", menuName = "Database/Sprite Recipe Database")]
public class SpriteDatabaseSO : ScriptableObject
{
    public List<SpriteImageTemplate> spriteImageTemplates;
}
