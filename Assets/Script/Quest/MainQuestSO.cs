// Di MainQuestSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Main Quest", menuName = "Quest System/Main Quest")]
public class MainQuestSO : ScriptableObject
{
    public string questName;
    [TextArea(3, 5)]
    public string description;

    // Tambahkan variabel ini untuk menentukan kapan quest bisa dimulai
    public int dateToActivate;
    public int monthToActivate; // Jika perlu bulan juga

    public GameObject questControllerPrefab;
}