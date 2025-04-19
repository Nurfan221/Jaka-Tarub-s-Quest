using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatuManager : MonoBehaviour
{
    [Serializable]
    public class ResourceData
    {
        public string nameResource;
        public Resource[] resources;
    }

    [Serializable]
    public class Resource
    {
        public GameObject resourceObject;
        public Vector2 location;
        public bool isHarvested;
    }

    public ResourceData[] minerResource;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckLocationResource()
    {
        foreach (var item in minerResource)
        {
            foreach (var itemObject in item.resources)
            {
                if (itemObject.resourceObject != null)
                {
                    itemObject.location = (Vector2)itemObject.resourceObject.transform.position;
                }
            }
        }
    }


    public void UpdatePositionMiner(float luckValue)
    {
        if (minerResource == null) return;

        foreach (var itemObject in minerResource)
        {
            int totalItems = itemObject.resources.Length;

            // Menentukan berapa persen item yang akan dimunculkan
            float percentage = 0.3f; // default 30%
            if (luckValue == 2) percentage = 0.5f;
            else if (luckValue == 3) percentage = 0.7f;

            // Hitung jumlah item yang ingin dimunculkan berdasarkan luck
            int itemsToShow = Mathf.CeilToInt(totalItems * percentage);

            // Buat list acak dari resource
            List<Resource> shuffledList = new List<Resource>(itemObject.resources);
            ShuffleList(shuffledList); // Kita acak urutannya

            // Tampilkan hanya sejumlah itemToShow
            for (int i = 0; i < shuffledList.Count; i++)
            {
                var item = shuffledList[i];
                bool show = i < itemsToShow;
                item.resourceObject.SetActive(show);
            }
        }
    }


    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }

}
