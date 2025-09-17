using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Atribut ini membuat script bisa berjalan di mode Editor, penting untuk OnValidate.
[ExecuteInEditMode]
public abstract class UniqueIdentifiableObject : MonoBehaviour, IUniqueIdentifiable
{
    [Header("Unique ID (Generated Automatically)")]
    [SerializeField]
    private string uniqueID;

    public string UniqueID { get => uniqueID; set => uniqueID = value; }

    // Metode "kosong" yang harus diisi oleh kelas turunan.
    public abstract string GetObjectType();
    public abstract EnvironmentHardnessLevel GetHardness();
    public abstract string GetBaseName();
    public abstract string GetVariantName();

    // --- LOGIKA BARU YANG LEBIH PINTAR ---
    protected virtual void OnValidate()
    {
        // Jangan berjalan saat game sedang dimainkan (play mode).
        if (Application.isPlaying) return;

        // --- PERBAIKAN KUNCI ---
        // Cek apakah ini adalah Prefab Asset di folder Project, BUKAN instance di scene.
#if UNITY_EDITOR
        if (PrefabUtility.IsPartOfPrefabAsset(this))
        {
            // Jika ini adalah "blueprint", pastikan ID-nya SELALU kosong.
            // Ini mencegah prefab menyimpan ID yang akan disalin oleh semua instancenya.
            if (!string.IsNullOrEmpty(uniqueID))
            {
                uniqueID = ""; // Hapus ID dari prefab asset.
                EditorUtility.SetDirty(this);
            }
            return; // Hentikan proses untuk prefab asset.
        }
#endif
        // Kode di bawah ini sekarang hanya akan berjalan untuk objek di dalam Scene.
        // Jika ID kosong (objek baru) atau namanya tidak cocok (objek duplikasi),
        // buat ID baru yang benar.
        if (string.IsNullOrEmpty(uniqueID) || uniqueID != gameObject.name)
        {
            GenerateAndAssignUniqueID();
        }
    }
    // Ini berguna untuk "memaksa" pembuatan ulang ID secara manual.
    [ContextMenu("Force Generate Unique ID")]
    public void ForceGenerateUniqueID()
    {
        // Panggil fungsi utama untuk membuat ID.
        // Ini akan berjalan bahkan jika ID sudah ada, efektif untuk mereset.
        GenerateAndAssignUniqueID();
    }

    // Fungsi ini bisa dipanggil dari spawner saat runtime.
    public void GenerateRuntimeUniqueID()
    {
        GenerateAndAssignUniqueID();
    }

    // Logika pembuatan ID yang lebih kuat untuk mencegah duplikasi.
    private void GenerateAndAssignUniqueID()
    {
        string baseID = $"{GetObjectType()}_{GetHardness()}_{GetBaseName()}_{GetVariantName()}";

        // Cari semua objek lain untuk memastikan ID kita benar-benar unik.
        var allIdentifiables = FindObjectsOfType<UniqueIdentifiableObject>();

        string potentialID;
        int count = 1;

        // Terus coba nomor berikutnya sampai menemukan yang belum terpakai.
        while (true)
        {
            potentialID = $"{baseID}_{count}";

            // Cek apakah ada objek LAIN yang sudah memakai ID potensial ini.
            bool isDuplicate = allIdentifiables.Any(obj => obj != this && obj.UniqueID == potentialID);

            if (!isDuplicate)
            {
                // Jika tidak ada yang pakai, ini adalah ID unik kita.
                break;
            }

            // Jika sudah dipakai, coba angka berikutnya.
            count++;
        }

        uniqueID = potentialID;
        gameObject.name = potentialID;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
        }
#endif
        Debug.Log($"ID otomatis dibuat untuk '{gameObject.name}': {uniqueID}", this);
    }
}

