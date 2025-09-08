using System;
using UnityEngine;

// Skrip ini akan memastikan setiap objek memiliki ID yang unik dan tidak berubah.
public class UniqueID : MonoBehaviour
{
    [Tooltip("ID unik untuk objek ini. Jangan diubah manual.")]
    [SerializeField] private string id;

    // Properti publik untuk mengakses ID dari skrip lain
    public string ID => id;

    // OnValidate dipanggil di Editor saat komponen ditambahkan atau di-reset.
    private void OnValidate()
    {
        // Jika ID masih kosong, buat yang baru.
        if (string.IsNullOrEmpty(id))
        {
            // Gunakan GUID untuk memastikan ID benar-benar unik.
            id = Guid.NewGuid().ToString();
        }
    }
}