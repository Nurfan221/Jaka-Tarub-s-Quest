using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameTutorialDatabase", menuName = "Database/GameTutorialDatabase")]
public class GameTutorialDatabase : ScriptableObject
{
    public List<TutorialData> allTutorials;

    // Fungsi Helper untuk mengubah List menjadi Dictionary agar pencarian Cepat
    public Dictionary<string, TutorialData> GetTutorialDictionary()
    {
        Dictionary<string, TutorialData> dict = new Dictionary<string, TutorialData>();
        foreach (var tut in allTutorials)
        {
            if (!dict.ContainsKey(tut.tutorialID))
            {
                dict.Add(tut.tutorialID, tut);
            }
        }
        return dict;
    }
}
