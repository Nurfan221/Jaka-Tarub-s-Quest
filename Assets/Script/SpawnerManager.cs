using System;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    [Serializable]
    public class Spawner
    {
        public string nameSpawner;
        public int idSpawner;
        public GameObject[] spawner;
    }
    public Spawner[] spawner;
    [SerializeField] QuestManager questManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        // Berlangganan ke event saat objek aktif
        TimeManager.OnDayChanged += HandleNewDay;
    }

    private void OnDisable()
    {
        // Selalu berhenti berlangganan saat objek nonaktif untuk menghindari error
        TimeManager.OnDayChanged -= HandleNewDay;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleNewDay()
    {
        float luck = TimeManager.Instance.GetDayLuck();
        SetSpawnerActive(luck);
    }
    public void SetSpawnerActive(float random)
    {
        // Loop untuk menonaktifkan semua spawner
        for (int i = 0; i < spawner[0].spawner.Length; i++)
        {
            // Ambil referensi AnimalSpawner dari setiap spawner yang ada
            AnimalSpawner animalSpawner = spawner[0].spawner[i].GetComponent<AnimalSpawner>();

            // Hapus musuh lama dari daftar enemies sebelum spawn baru
            animalSpawner.DeleteEnemiesFromArray();

            // Nonaktifkan spawner
            spawner[0].spawner[i].gameObject.SetActive(false);
        }

        if (spawner[0] != null && spawner[0].spawner.Length > 0)
        {
            // Loop sebanyak "random" untuk memilih spawner yang aktif
            for (int i = 0; i < Mathf.FloorToInt(random); i++)
            {
                // Menghasilkan angka acak dalam rentang jumlah elemen spawner[0].spawner
                int randomIntSpawner = UnityEngine.Random.Range(0, spawner[0].spawner.Length);

                // Aktifkan objek yang sesuai dengan index acak
                spawner[0].spawner[randomIntSpawner].gameObject.SetActive(true);

                // Ambil referensi AnimalSpawner dari spawner yang baru saja diaktifkan
                AnimalSpawner animalSpawner = spawner[0].spawner[randomIntSpawner].GetComponent<AnimalSpawner>();

                // Spawn hewan baru dan hewan spesial
                animalSpawner.SpawnAnimal();
                if (questManager.chapter1IsDone)
                {
                    animalSpawner.SpawnAnimalSpesial();
                }
            }
        }

        SetSpawnerBanditActive(random);
    }

    public void SetSpawnerBanditActive(float random)
    {
        // Loop untuk menonaktifkan semua spawner
        for (int i = 0; i < spawner[1].spawner.Length; i++)
        {
            // Nonaktifkan spawner
            spawner[1].spawner[i].gameObject.SetActive(false);
        }

        if (spawner[1] != null && spawner[1].spawner.Length > 0)
        {
            // Menggunakan dailyLuck untuk menentukan peluang
            float adjustedRandom = Mathf.Lerp(0.5f, 3f, 1 - random); // Nilai antara 0.5 dan 3 berdasarkan dailyLuck (lebih kecil dailyLuck, lebih besar chance-nya)

            // Kondisi untuk menentukan berapa banyak loop berdasarkan adjustedRandom
            if (adjustedRandom >= 0 && adjustedRandom < 1f)
            {
                // Loop sebanyak "adjustedRandom" untuk memilih spawner yang aktif
                for (int i = 0; i < 7; i++)  // Jika adjustedRandom berada antara 0 dan 1, aktifkan 7 spawner
                {
                    int randomIntSpawner = UnityEngine.Random.Range(0, spawner[1].spawner.Length);
                    spawner[1].spawner[randomIntSpawner].gameObject.SetActive(true);
                }
            }
            else if (adjustedRandom >= 1 && adjustedRandom < 2)
            {
                // Loop sebanyak "adjustedRandom" untuk memilih spawner yang aktif
                for (int i = 0; i < 4; i++)  // Jika adjustedRandom berada antara 1 dan 2, aktifkan 4 spawner
                {
                    int randomIntSpawner = UnityEngine.Random.Range(0, spawner[1].spawner.Length);
                    spawner[1].spawner[randomIntSpawner].gameObject.SetActive(true);
                }
            }
            else if (adjustedRandom >= 2)
            {
                // Loop sebanyak "adjustedRandom" untuk memilih spawner yang aktif
                for (int i = 0; i < 2; i++)  // Jika adjustedRandom lebih besar dari 2, aktifkan 2 spawner
                {
                    int randomIntSpawner = UnityEngine.Random.Range(0, spawner[1].spawner.Length);
                    spawner[1].spawner[randomIntSpawner].gameObject.SetActive(true);
                }
            }
        }
    }




}
