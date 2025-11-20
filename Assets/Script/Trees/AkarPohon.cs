using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class AkarPohon : UniqueIdentifiableObject
{
    //  Implementasi dari Kontrak IUniqueIdentifiable 
    public string IdObjectUtama;
    public EnvironmentHardnessLevel hardnessLevel;
    public TypeObject typeObject;
    public TypePlant typePlant;
    public GrowthTree currentStage;
    public int health;
    public ItemData kayu;
    public int minKayu;
    public int maxKayu;

    public bool ditebang = false;

    #region Unique ID Implementation

    public override string GetObjectType()
    {
        // Berikan kategori umum untuk objek ini.
        return typeObject.ToString();
    }

    public override EnvironmentHardnessLevel GetHardness()
    {
        // Ambil nilai dari variabel yang bisa diatur di Inspector.
        return hardnessLevel;
    }

    public override string GetBaseName()
    {
        // Ambil nama dasar dari variabel yang bisa diatur di Inspector.
        return typePlant.ToString();
    }

    public override string GetVariantName()
    {
        return currentStage.ToString();
    }

    #endregion

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
        if(ditebang) return;
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
            ditebang = true;
            for (int i = 0; i < woodCount; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
                if (kayu != null)
                {
                    Vector3 posisi = transform.position;
                    ItemPool.Instance.DropItem(kayu.itemName, kayu.itemHealth, kayu.quality, posisi + offset);
                }
            }
            MainEnvironmentManager.Instance.pohonManager.CheckTreefromSecondList(IdObjectUtama);
            StartCoroutine(PlayDelay());
        }

    }

   public IEnumerator PlayDelay()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    public void InstantlyDestroy()
    {
        // Kirim damage sebesar HP saat ini (pasti mati)
        TakeDamage(9999);
    }
}
