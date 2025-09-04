using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteDatabaseSO", menuName = "Database/Sprite Recipe Database")]
public class SpriteDatabaseSO : ScriptableObject
{
    public List<SpriteImageTemplate> spriteImageTemplates;
}
