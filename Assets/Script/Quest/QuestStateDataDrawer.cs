using UnityEditor;
using UnityEngine;

// Memberitahu Unity untuk menggunakan drawer ini setiap kali menemukan tipe QuestStateData di Inspector
[CustomPropertyDrawer(typeof(QuestStateData))]
public class QuestStateDataDrawer : PropertyDrawer
{
    // Override fungsi untuk menggambar properti di Inspector
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Cari properti 'state' di dalam kelas QuestStateData
        SerializedProperty stateProperty = property.FindPropertyRelative("state");

        // Ambil nama dari enum yang terpilih sebagai judul baru
        // enumNames[property.enumValueIndex] akan mengambil nama string dari enum
        label.text = stateProperty.enumNames[stateProperty.enumValueIndex];

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