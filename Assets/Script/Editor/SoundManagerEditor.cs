using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

[CustomEditor(typeof(MusicLibrary))]
public class SoundManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MusicLibrary manager = (MusicLibrary)target;
        GUILayout.Space(10);

        if (GUILayout.Button("Update Sound Enum", GUILayout.Height(40)))
        {
            UpdateEnumFile(manager);
        }
    }

    private void UpdateEnumFile(MusicLibrary manager)
    {
        string enumName = "SoundName";

        string filePath = "Assets/Script/SoundManager/SoundLibrary.cs";

     

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("// GENERATED CODE - DO NOT MODIFY MANUALLY");
        sb.AppendLine("public enum " + enumName);
        sb.AppendLine("{");
        sb.AppendLine("    None,");

        if (manager.sfxList != null)
        {
            foreach (var sfx in manager.sfxList)
            {
                string cleanName = sfx.soundName.Replace(" ", "").Replace("-", "_");
                if (!string.IsNullOrEmpty(cleanName))
                {
                    sb.AppendLine("    " + cleanName + ",");
                }
            }
        }

        sb.AppendLine("}");

        File.WriteAllText(filePath, sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log("Sound Enum berhasil diupdate ke file: " + filePath);
    }
}