using UnityEngine;
using System.Collections.Generic;
using System.Linq; 

public class WorldManager : MonoBehaviour
{

    public static WorldManager Instance { get; private set; }

    private void Awake()
    {
        // Pola Singleton sederhana
        if (Instance != null && Instance != this)
        {
            // Jika sudah ada instance lain, hancurkan yang ini
            Destroy(gameObject);
        }
        else
        {
            // Jika belum ada, jadikan ini sebagai instance utama
            Instance = this;
        }
    }

    private void Start()
    {
        GenerateAllUniqueIDs();
    }
    [ContextMenu("Generate Unique IDs For All Identifiable Objects")]
    [ContextMenu("Generate Unique IDs For All Identifiable Objects")]
    private void GenerateAllUniqueIDs()
    {
        // Temukan SEMUA objek di scene yang punya kontrak IUniqueIdentifiable
        var allIdentifiables = FindObjectsOfType<MonoBehaviour>().OfType<IUniqueIdentifiable>();

        // Gunakan Dictionary untuk melacak hitungan untuk setiap nama dasar
        var nameCounter = new Dictionary<string, int>();

        // Loop melalui setiap objek yang ditemukan
        foreach (var item in allIdentifiables)
        {
            // Buat ID dasar dari informasi yang disediakan oleh objek
            string baseID = $"{item.GetObjectType()}_{item.GetHardness()}_{item.GetBaseName()}";

            // Cek sudah ada berapa objek dengan ID dasar yang sama
            if (!nameCounter.ContainsKey(baseID))
            {
                nameCounter[baseID] = 1; // Jika ini yang pertama, hitungannya 1
            }
            else
            {
                nameCounter[baseID]++; // Jika sudah ada, naikkan hitungannya
            }

            // Gabungkan ID dasar dengan hitungan uniknya
            string finalID = $"{baseID}_{nameCounter[baseID]}";

            // Set ID unik kembali ke objeknya
            item.UniqueID = finalID;

            // --- TAMBAHAN BARU: Ubah nama GameObject agar sama dengan ID ---
            (item as MonoBehaviour).gameObject.name = finalID;

            // Tandai objek agar perubahan tersimpan di scene
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(item as MonoBehaviour);
#endif
        }

        Debug.Log($"Selesai! {allIdentifiables.Count()} objek telah diberi ID unik dan nama GameObject telah diubah.");
    }
}