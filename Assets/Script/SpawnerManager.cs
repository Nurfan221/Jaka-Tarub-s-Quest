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
    public bool chapter1IsDone;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckChapter1IsDone(float random)
    {
        for(int i =0; i < spawner[0].spawner.Length; i++)
        {
            spawner[0].spawner[i].gameObject.SetActive(false);
        }
        if (chapter1IsDone)
        {
            if (spawner[0] != null && spawner[0].spawner.Length > 0)
            {
                // Loop sebanyak "random"
                for (int i = 0; i < Mathf.FloorToInt(random); i++)
                {
                    // Menghasilkan angka acak dalam rentang jumlah elemen spawner[0].spawner
                    int randomIntSpawner = UnityEngine.Random.Range(0, spawner[0].spawner.Length);

                    // Aktifkan objek yang sesuai dengan index acak
                    spawner[0].spawner[randomIntSpawner].gameObject.SetActive(true);
                }
            }
        }
    }

}
