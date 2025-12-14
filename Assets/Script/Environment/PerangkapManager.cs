using System.Collections.Generic;
using UnityEngine;

public class PerangkapManager : MonoBehaviour, ISaveable
{
    public static PerangkapManager Instance { get; private set; }

    public List<PerangkapSaveData> perangkapListActive = new List<PerangkapSaveData>();

    private void Awake()
    {
        // Pastikan hanya ada satu instance dari PerangkapManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opsional: Pertahankan instance ini saat memuat scene baru
        }
        else
        {
            Destroy(gameObject); // Hancurkan instance duplikat
        }
    }

    public object CaptureState()
    {
        Debug.Log("[SAVE-CAPTURE] PerangkapManager menangkap data Perangkap aktif...");


        return perangkapListActive;
    }

    public void RestoreState(object state)
    {
        Debug.Log("[LOAD-RESTORE] PerangkapManager merestorasi data quest aktif...");
        perangkapListActive.Clear();
        // Coba cast 'state' yang datang kembali ke tipe aslinya.
        var loadedData = state as List<PerangkapSaveData>;

        if (loadedData != null)
        {
            perangkapListActive = loadedData;

            Debug.Log($"Data perangkap berhasil direstorasi. {perangkapListActive.Count} perangkap aktif dimuat.");

            AddPerangkapToGame();
        }
        else
        {
            Debug.LogWarning("Gagal merestorasi data quest: data tidak valid atau corrupt.");
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void AddPerangkapToGame()
    {
        // Validasi awal
        if (DatabaseManager.Instance == null || DatabaseManager.Instance.perangkapWorldPrefab == null)
        {
            Debug.LogError("[PerangkapManager] Prefab perangkap tidak ditemukan di DatabaseManager!");
            return;
        }

        if (MainEnvironmentManager.Instance == null || MainEnvironmentManager.Instance.perangkapManager == null)
        {
            Debug.LogError("[PerangkapManager] MainEnvironmentManager atau parent perangkap tidak tersedia!");
            return;
        }

        GameObject perangkapPrefab = DatabaseManager.Instance.perangkapWorldPrefab;
        Transform parentTransform = MainEnvironmentManager.Instance.perangkapManager.transform;

        Debug.Log($"[PerangkapManager] Memunculkan {perangkapListActive.Count} perangkap dari save...");

        foreach (var perangkapData in perangkapListActive)
        {
            // Instantiate prefab sebagai GameObject baru
            GameObject newPerangkapGO = Instantiate(perangkapPrefab, perangkapData.perangkapPosition, Quaternion.identity);

            // Set parent (true: pertahankan world position)
            newPerangkapGO.transform.SetParent(parentTransform, true);

            // Beri nama sesuai id supaya mudah dicari nanti
            newPerangkapGO.name = perangkapData.id;

            // Ambil komponen PerangkapBehavior pada instance
            PerangkapBehavior perangkapBehavior = newPerangkapGO.GetComponent<PerangkapBehavior>();
            if (perangkapBehavior == null)
            {
                Debug.LogError("[PerangkapManager] Prefab perangkap tidak memiliki komponen PerangkapBehavior!");
                continue;
            }

            // Isi properti dari data save
            perangkapBehavior.UniqueID = perangkapData.id; // atau properti yang sesuai
            perangkapBehavior.perangkapHealth = perangkapData.healthPerangkap;
            perangkapBehavior._isFull = perangkapData.isfull;

            // Jika hasilTangkap adalah ItemData / serializable type, copy/assign dengan aman
            perangkapBehavior.itemTertangkap = perangkapData.hasilTangkap; // asumsi tipenya cocok

            if (perangkapBehavior._isFull)
            {
                perangkapBehavior.HandlePerangkapFull(perangkapBehavior._isFull);
            }
            else
            {
                perangkapBehavior.GetRandomAnimal();
            }

            // Jika perlu inisialisasi visual / state internal, panggil metode ready
            // misal: perangkapBehavior.InitializeFromSave(perangkapData);

            Debug.Log($"[PerangkapManager] Perangkap {perangkapData.id} dipasang di {perangkapData.perangkapPosition} (isFull={perangkapData.isfull}).");
        }
    }

}
