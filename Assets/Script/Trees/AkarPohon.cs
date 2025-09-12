using UnityEngine;

public class AkarPohon : MonoBehaviour
{
    public int health;
    public ItemData kayu;
    public int minKayu;
    public int maxKayu;

    public ParticleSystem hitEffectPrefab; // Variabel untuk Prefab partikel
    void Start()
    {
        hitEffectPrefab = gameObject.GetComponentInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void TakeDamage(int damage)
    {
        if (hitEffectPrefab != null)
        {
            Debug.Log("Menampilkan efek pukulan pada posisi: " + gameObject.transform.position);
            // Buat instance dari prefab efek di lokasi pukulan dengan rotasi yang sesuai
            hitEffectPrefab.Play();
        }
        health -= Mathf.Min(damage, health);
        Debug.Log($"Pohon terkena damage. Sisa HP: {health}");

        // Hitung jumlah random untuk masing-masing item
        int woodCount = Random.Range(minKayu, maxKayu + 1);
        if (health <= 0)
        {
            for (int i = 0; i < woodCount; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
                if (kayu != null)
                {
                    Vector3 posisi = transform.position;
                    ItemPool.Instance.DropItem(kayu.itemName, kayu.itemHealth, kayu.quality, posisi + offset);
                }
            }
            Destroy(gameObject);
        }

    }

}
