using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeManager : MonoBehaviour
{
    [System.Serializable]
    public class TreesResource
    {
        public string name; // Nama pohon
        public float health; // Kesehatan pohon
        public GameObject pohonPrefab; // Referensi prefab pohon
        public Item kayu;   // Item kayu
        public Item getah;  // Item getah
        public Item daun;   // Item daun
        public Item bibit;  // Item bibit
    }

    public TreesResource[] TreesResourcesList; // Daftar jenis pohon

    // Variabel untuk pengaturan radius dan jumlah pohon maksimal
    public float radius = 5f;  // Radius area di mana pohon akan ditanam
    public int maxTrees = 10;  // Jumlah pohon maksimal
    public float treeRadius = 0.5f; // Radius pohon (untuk mencegah tumpang tindih)

    private List<Vector2> pohonPositions = new List<Vector2>(); // Menyimpan posisi pohon yang sudah ditanam
    private Vector2 center; // Titik tengah area penanaman pohon

    void Start()
    {
        center = transform.position; // Menggunakan posisi objek ini sebagai titik tengah
        TanamSemuaPohon(); // Menanam semua pohon sekaligus saat game dimulai
    }

    // Fungsi untuk menanam semua pohon sekaligus
    void TanamSemuaPohon()
    {
        int pohonYangDitambahkan = 0; // Jumlah pohon yang berhasil ditanam

        // Lanjutkan hingga mencapai jumlah pohon maksimal
        while (pohonYangDitambahkan < maxTrees)
        {
            // Ambil pohon secara acak dari TreesResourcesList
            TreesResource tree = TreesResourcesList[Random.Range(0, TreesResourcesList.Length)];

            // Dapatkan posisi acak di dalam lingkaran
            Vector2 newPosition = GetRandomPositionInCircle();

            // Cek apakah posisi valid
            if (IsPositionValid(newPosition))
            {
                // Tambahkan posisi ke daftar dan instantiate pohon
                pohonPositions.Add(newPosition);
                Instantiate(tree.pohonPrefab, newPosition, Quaternion.identity);

                pohonYangDitambahkan++; // Tambahkan jumlah pohon yang berhasil ditanam
            }
        }
    }


    // Fungsi untuk mendapatkan posisi acak di dalam lingkaran
    Vector2 GetRandomPositionInCircle()
    {
        float angle = Random.Range(0f, 2f * Mathf.PI); // Sudut acak
        float distance = Random.Range(0f, radius); // Jarak acak dari pusat lingkaran

        // Hitung posisi X dan Y menggunakan trigonometri
        float x = center.x + distance * Mathf.Cos(angle);
        float y = center.y + distance * Mathf.Sin(angle);

        return new Vector2(x, y);
    }

    // Fungsi untuk memeriksa apakah posisi pohon valid (tidak tumpang tindih)
    bool IsPositionValid(Vector2 position)
    {
        foreach (Vector2 pohon in pohonPositions)
        {
            // Cek jarak antara pohon yang akan ditanam dengan pohon yang sudah ada
            if (Vector2.Distance(pohon, position) < treeRadius * 2)
            {
                return false; // Jika terlalu dekat, posisi tidak valid
            }
        }
        return true; // Posisi valid
    }
}
