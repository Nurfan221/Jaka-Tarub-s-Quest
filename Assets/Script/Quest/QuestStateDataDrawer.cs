using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(QuestStateData))]
public class QuestStateDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Cari properti 'state' di dalam kelas QuestStateData
        SerializedProperty stateProperty = property.FindPropertyRelative("state");

        if (stateProperty != null)
        {
            // Ambil index saat ini dan array nama-nama enum
            int currentIndex = stateProperty.enumValueIndex;
            string[] names = stateProperty.enumNames;

            // Cek apakah index aman (tidak lebih besar dari jumlah item enum)
            if (currentIndex >= 0 && currentIndex < names.Length)
            {
                // Jika aman, gunakan nama enum sebagai label
                label.text = names[currentIndex];
            }
            else
            {
                // Jika error (index di luar batas), tampilkan pesan error di label
                // Ini mencegah crash Unity Editor
                label.text = $"Invalid State (Index: {currentIndex})";

                // Reset ke 0 agar error hilang permanen
                stateProperty.enumValueIndex = 0;
            }
        }

        // Gambar field properti dengan judul yang sudah diubah
        EditorGUI.PropertyField(position, property, label, true);

        EditorGUI.EndProperty();
    }

    // Override fungsi untuk mengatur ketinggian elemen di Inspector
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}