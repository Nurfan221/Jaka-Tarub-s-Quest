using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldManager : MonoBehaviour
{
    [Header("Pengaturan")]
    [Tooltip("Jika true, akan mencoba menjalankan fungsi ini secara otomatis saat scene disimpan.")]
    public bool generateIDsOnSave = true;

    // --- Tombol Utama di Inspector ---
    [ContextMenu("Generate Unique IDs For ALL Identifiable Objects")]
    public void GenerateIDsForAllObjects()
    {
        // Temukan SEMUA objek di scene yang punya kontrak IUniqueIdentifiable
        var allIdentifiables = FindObjectsOfType<MonoBehaviour>().OfType<IUniqueIdentifiable>();

        if (allIdentifiables.Count() == 0)
        {
            Debug.Log("Tidak ada objek yang mengimplementasikan IUniqueIdentifiable ditemukan di scene.");
            return;
        }

        int count = 0;
        // Loop melalui setiap objek yang ditemukan
        foreach (var item in allIdentifiables)
        {
            // Panggil fungsi "Force" yang sudah kita buat sebelumnya di setiap objek.
            // Ini akan memaksa setiap objek untuk membuat ulang ID-nya.
            (item as UniqueIdentifiableObject)?.ForceGenerateUniqueID();
            count++;
        }

        Debug.Log($"PROSES SELESAI: {count} objek telah diperiksa dan diberi ID unik. Jangan lupa save scene (Ctrl+S).");
    }

#if UNITY_EDITOR
    // --- Fitur Tambahan (Opsional tapi Berguna) ---
    // Kode ini akan mencoba menjalankan pembuatan ID secara otomatis setiap kali Anda menyimpan scene.
    private void OnEnable()
    {
        EditorApplication.quitting += OnEditorQuit;
        UnityEditor.SceneManagement.EditorSceneManager.sceneSaving += OnSceneSaving;
    }

    private void OnDisable()
    {
        EditorApplication.quitting -= OnEditorQuit;
        UnityEditor.SceneManagement.EditorSceneManager.sceneSaving -= OnSceneSaving;
    }

    private void OnSceneSaving(UnityEngine.SceneManagement.Scene scene, string path)
    {
        if (generateIDsOnSave)
        {
            Debug.Log("Menyimpan scene, menjalankan pengecekan ID otomatis...");
            GenerateIDsForAllObjects();
        }
    }

    private void OnEditorQuit()
    {
        // Pastikan event listener dilepas saat editor ditutup
        OnDisable();
    }
#endif
}
