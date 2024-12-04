using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatuManager : MonoBehaviour
{
    [System.Serializable]
    public class Mine
    {
        public Resource[] resources; // Satu array untuk menampung semua batu
    }

    [System.Serializable]
    public class Resource
    {
       
        public GameObject resourceObject; // Objek game
        public Vector3 position; // Posisi batu
        public ResourceType type; // Jenis batu (misalnya Stone atau Rock)
    }

    public enum ResourceType
    {
        Stone,
        Rock
    }



    [Header("Array Objek Batu ")]

    public Mine[] mines;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
