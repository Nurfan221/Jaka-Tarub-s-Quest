using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "WorldStoneDatabaseSO", menuName = "Database/WorldStoneDatabase")]
public class WorldStoneDatabaseSO : ScriptableObject
{
    public List<ResourceData> stoneBehaviors = new List<ResourceData>();
    public List<TemplateStoneObject> templateStoneObject;

}
