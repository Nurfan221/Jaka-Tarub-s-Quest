using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

public class PerangkapBehavior : UniqueIdentifiableObject
{   
    public EnvironmentHardnessLevel hardnessLevel;
    public TypeObject typeObject;
    public TypePlant typePlant;
    public ArahObject arahObject;
    public EnvironmentType environmentType;

    public SpriteRenderer spriteRenderer; // Renderer untuk menampilkan sprite perangkap
    public Item[] itemPerangkap; // Item yang digunakan untuk perangkap
    public Item itemTertangkap; // Item hewan yang tertangkap
    public int perangkapHealth; // Kesehatan perangkap
    public event System.Action<bool> OnFullChanged;
    public bool _isFull;
    public bool IsFull
    {
        get => _isFull;
        set
        {
            if (_isFull != value)
            {
                _isFull = value;
                OnFullChanged?.Invoke(_isFull);
            }
        }
    }

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
        if (typePlant == TypePlant.None && arahObject != ArahObject.None)
        {
            return arahObject.ToString();
        }
        else
        {
            return typePlant.ToString();
        }

    }

    public override string GetVariantName()
    {
        return environmentType.ToString();

    }

    #endregion

    private void OnEnable()
    {
        TimeManager.OnDayChanged += NewDay;
    }
    private void OnDisable()
    {
        TimeManager.OnDayChanged -= NewDay;
    }
    private void Start()
    {

    }
    
    public void NewDay()
    {
        if (IsFull)
        {
            GetRandomAnimal();

        }
    }
    public void GetRandomAnimal()
    {
        if (IsFull)
        {
            Debug.Log("Perangkap sudah penuh, tidak bisa menangkap hewan lagi.");
            return;
        }

        // Ambil nilai keberuntungan hari ini (0 - 3, bertipe float)
        float dayLuck = TimeManager.Instance.GetDayLuck();
        Debug.Log($"[Perangkap] Day Luck hari ini: {dayLuck:F2}");

        // Gunakan dayLuck untuk meningkatkan peluang tangkapan
        float luckMultiplier = 1f + (dayLuck * 0.25f);

        // Tentukan apakah perangkap berhasil menangkap sesuatu hari ini
        float catchChance = Random.value * luckMultiplier;
        if (catchChance < 0.4f)
        {
            Debug.Log($"[Perangkap] Gagal menangkap hewan hari ini. (Chance={catchChance:F2})");
            return;
        }

        // Jika berhasil, tentukan hewan berdasarkan keberuntungan
        // Konversi hasil menjadi integer agar bisa digunakan untuk index
        int randomIndex = Mathf.Clamp(
            Mathf.FloorToInt(Random.Range(0f, itemPerangkap.Length) + dayLuck),
            0,
            itemPerangkap.Length - 1
        );

        itemTertangkap = itemPerangkap[randomIndex];
        Debug.Log($"[Perangkap] Menangkap hewan: {itemTertangkap.itemName} (Luck={dayLuck:F2}, Index={randomIndex})");

        _isFull = true; 
        HandlePerangkapFull(IsFull);
        UpdatePerangkapInListManager();
    }

    public void HandlePerangkapFull(bool full)
    {
        if (full)
        {
            spriteRenderer.sprite = itemTertangkap.sprite;
        }else
        {
            spriteRenderer.sprite = null;
        }
        spriteRenderer.gameObject.SetActive(full);

    }

    public void UpdatePerangkapInListManager()
    {
        foreach (var perangkap in PerangkapManager.Instance.perangkapListActive)
        {
            if (this.UniqueID == perangkap.id)
            {
                perangkap.hasilTangkap = this.itemTertangkap;
                perangkap.isfull = this._isFull;
                perangkap.healthPerangkap = this.perangkapHealth;
            }
        }
    }

    public void IfDestroy(bool force = false)
    {
        // Jika bukan dipaksa, periksa health seperti biasa
        if (!force)
        {
            if (perangkapHealth > 0)
                return;
        }

        // Logika penghapusan perangkap
        Debug.Log("[Perangkap] Perangkap dihancurkan atau diambil.");

        // Hapus data perangkap dari PerangkapManager
        PerangkapManager.Instance.perangkapListActive.RemoveAll(p => p.id == UniqueID);

        // Hancurkan gameobject-nya
        Destroy(gameObject);
    }

    public void TakePerangkap()
    {
        if (!IsFull)
        {
            
            Debug.Log("Mengambil perangkap kosong.");
            // Logika untuk mengambil perangkap kosong
            ItemData itemData = new ItemData
            {
                itemName = "Perangkap",
                count = 1,
                quality = ItemQuality.Normal,
                itemHealth = perangkapHealth
            }; 
            bool isSuccess = ItemPool.Instance.AddItem(itemData);

            if (isSuccess)
            {
                // Hapus item dari perangkap
                IfDestroy(force: true);

            }
            else
            {
                // Jangan hapus, biarkan di perangkap
                Debug.Log("Tas penuh, item tetap di tungku.");
                // Opsional: Munculkan teks "Tas Penuh!"
            }


        }
        else
        {
            Debug.Log("Perangkap penuh, tidak bisa diambil.");
        }
    }

    public void TakeAnimal()
    {
        if (IsFull)
        {
            
            // Logika untuk mengambil hewan dari perangkap
            Debug.Log("Mengambil hewan dari perangkap.");
            _isFull = false;
            ItemData itemData = new ItemData
            {
                itemName = itemTertangkap.itemName,
                count = 1,
                quality = itemTertangkap.quality,
                itemHealth = itemTertangkap.health
            };

            // Update data manager dan hapus perangkap secara paksa
            bool isSuccess = ItemPool.Instance.AddItem(itemData);

            if (isSuccess)
            {
                // Hapus item dari perangkap
                HandlePerangkapFull(IsFull);
                perangkapHealth -= 1;
                itemTertangkap = null;
                UpdatePerangkapInListManager();
                IfDestroy();
            }
            else
            {
                // Jangan hapus, biarkan di perangkap
                Debug.Log("Tas penuh, item tetap di tungku.");
                // Opsional: Munculkan teks "Tas Penuh!"
            }
            //ItemPool.Instance.AddItem(itemData);
          

        }
        else
        {
            Debug.Log("Perangkap kosong, tidak ada hewan untuk diambil.");
        }
    }


}
