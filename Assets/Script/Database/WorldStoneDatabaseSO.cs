using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "WorldStoneDatabaseSO", menuName = "Database/WorldStoneDatabase")]
public class WorldStoneDatabaseSO : ScriptableObject
{
    public List<ListBatuManager> stoneBehaviors = new List<ListBatuManager>();
    public List<TemplateStoneObject> templateStoneObject;

}
