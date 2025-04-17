using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Trash
{
    public GameObject trashObject;
    public string trashName;
    public Vector2 trashLocation;
    public int trashCount;
}

public class TrashManager : MonoBehaviour
{
    // Enum untuk menggantikan ID kasta atau level sampah
    public enum TrashRank
    {
        Low = 1,
        Normal = 2,
        High = 3
    }

    [Serializable]
    public class TierTrash
    {
        public TrashRank rank; // Menentukan kasta atau level sampah
        public List<Trash> trashItems = new List<Trash>(); // List sampah berdasarkan rank

        // Constructor untuk memudahkan pembuatan objek TierTrash
        public TierTrash(TrashRank rank)
        {
            this.rank = rank;
        }

        // Fungsi untuk menambahkan sampah ke dalam rank
        public void AddTrash(Trash trash)
        {
            trashItems.Add(trash);
        }
    }

    // Daftar untuk menyimpan semua rank sampah
    public List<TierTrash> TierTrashList = new List<TierTrash>();
    public Transform trashTransform;

    void Start()
    {
    }

    // Fungsi untuk memeriksa dan memunculkan sampah sesuai tier
    public void CheckTrash()
    {
        Debug.Log("CheckTrash berjalan");

        // Periksa setiap tier untuk sampah yang ada
        foreach (var tierTrash in TierTrashList)
        {
            // Periksa setiap sampah dalam tier ini
            foreach (var trash in tierTrash.trashItems)
            {
                Debug.Log("objek trash di tambahkan ");

                // Cek apakah prefab trashObject sudah ada
                if (trash.trashObject != null)
                {
                    // Jika objek belum ada, buat objek dari prefab
                    GameObject newTrashObject = Instantiate(trash.trashObject);

                    // Menambahkan objek ke dalam hierarki tertentu (misalnya trashTransform)
                    newTrashObject.transform.SetParent(trashTransform);

                    // Menampilkan objek trash jika belum aktif
                    newTrashObject.SetActive(true);

                    // Memindahkan objek trash ke lokasi yang sesuai
                    newTrashObject.transform.position = new Vector3(trash.trashLocation.x, trash.trashLocation.y, 0);

                    // Bisa ditambahkan logika untuk mengatur jumlah atau interaksi lebih lanjut
                    Debug.Log($"Menampilkan {trash.trashName} di lokasi {trash.trashLocation}.");
                }
                else
                {
                    Debug.LogWarning("Prefab trashObject tidak ditemukan untuk " + trash.trashName);
                }
            }
        }
    }

}
