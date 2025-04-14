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
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
    }



}
