using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneBehavior : MonoBehaviour
{
    public float health; // Kesehatan batu

    // Deklarasi resource hasil menghancurkan batu
    public GameObject stoneObject;
    public GameObject ironObject;
    public GameObject goldObject;
    public GameObject copperObject;
    public GameObject coalObject;

    // Logika menentukan jumlah minimal dan maksimal dari item yang akan dijatuhkan
    public int minStone = 1;  // Jumlah minimum batu
    public int maxStone = 5;  // Jumlah maksimum batu
    public int minSpecialItem = 2; // Jumlah item spesial seperti copper, iron, atau gold
    public int maxSpecialItem = 4;

    public void TakeDamage(int damage)
    {
        health -= Mathf.Min(damage, health);
        Debug.Log($"Batu terkena damage. Sisa HP: {health}");

        if (health <= 0)
        {
            DestroyStone();
        }
    }

    private void DestroyStone()
    {
        Debug.Log("Batu dihancurkan!");

        // Hitung jumlah acak untuk batu
        int stoneCount = Random.Range(minStone, maxStone + 1);
        Debug.Log($"Jumlah batu yang akan dijatuhkan: {stoneCount}");

        // Jatuhkan batu sebagai stack item tunggal
        if (stoneObject != null)
        {
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
            ItemPool.Instance.DropItem(stoneObject.name, transform.position + offset, stoneObject, stoneCount);
        }

        // Hitung jumlah acak untuk special item
        int specialItemCount = Random.Range(minSpecialItem, maxSpecialItem + 1);
        Debug.Log($"Jumlah total item spesial yang akan dijatuhkan: {specialItemCount}");

        if (specialItemCount > 0)
        {
            // Buat kamus untuk melacak jumlah masing-masing item spesial yang akan dijatuhkan
            Dictionary<GameObject, int> specialItemsToDrop = new Dictionary<GameObject, int>();

            for (int i = 0; i < specialItemCount; i++)
            {
                // Pilih item spesial secara acak berdasarkan nama prefab
                GameObject specialItem = GetSpecialItemBasedOnPrefab();
                if (specialItem != null)
                {
                    if (specialItemsToDrop.ContainsKey(specialItem))
                    {
                        // Tambahkan jumlah item jika sudah ada
                        specialItemsToDrop[specialItem]++;
                    }
                    else
                    {
                        // Tambahkan item baru ke kamus
                        specialItemsToDrop[specialItem] = 1;
                    }
                }
            }

            // Drop setiap jenis item spesial sesuai jumlah yang terhitung
            foreach (var kvp in specialItemsToDrop)
            {
                GameObject item = kvp.Key;
                int count = kvp.Value;

                Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                ItemPool.Instance.DropItem(item.name, transform.position + offset, item, count);
                Debug.Log($"Item spesial dijatuhkan: {item.name} dengan jumlah {count}");
            }
        }

        // Hancurkan Batu setelah menjatuhkan item
        Destroy(gameObject);
    }

    private GameObject GetSpecialItemBasedOnPrefab()
    {
        // Nama prefab menentukan peluang ore
        string prefabName = gameObject.name.ToLower(); // Pastikan nama dalam huruf kecil
        int randomChance = Random.Range(0, 100);

        switch (prefabName)
        {
            case "batuprefab":
                if (randomChance < 10) return ironObject;  // 10% peluang iron
                if (randomChance < 20) return goldObject;  // 10% peluang gold
                if (randomChance < 30) return copperObject; // 10% peluang copper
                if (randomChance < 40) return coalObject;   // 10% peluang coal
                break;

            case "copper":
                if (randomChance < 80) return copperObject; // 60% peluang copper
                if (randomChance < 30) return ironObject;   // 30% peluang iron
                if (randomChance < 40) return coalObject;   // 10% peluang coal
                break;

            case "iron":
                if (randomChance < 80) return ironObject;   // 60% peluang iron
                if (randomChance < 30) return copperObject; // 20% peluang copper
                if (randomChance < 40) return coalObject;   // 10% peluang coal
                break;

            case "gold":
                if (randomChance < 80) return goldObject;   // 60% peluang gold
                if (randomChance < 40) return ironObject;   // 20% peluang iron
                break;

            case "coal":
                if (randomChance < 80) return coalObject;   // 60% peluang coal
                if (randomChance < 40) return copperObject; // 40% peluang copper
                if (randomChance < 30) return stoneObject; // 30% peluang stone
                break;

            default:
                Debug.LogWarning($"Prefab {prefabName} tidak dikenal. Menggunakan default logic.");
                break;
        }

        return null; // Tidak ada item spesial yang dijatuhkan
    }
}
