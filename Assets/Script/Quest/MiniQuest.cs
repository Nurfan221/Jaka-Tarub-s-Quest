using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public enum KategoriItem
{
    Sayuran,
    BuahBuahan,
    HasilBuruan
}

public class MiniQuest : MonoBehaviour
{
    [Serializable]
    public class MiniQuestList
    {
        public string judulQuest;
        public List<Item> itemsQuest = new List<Item>();
        public List<int> countItemQuest = new List<int>();
        public int DateMiniQuest;
        public int rewardQuest;
        public Item rewardItemQuest;
        public GameObject npc;
    }

    [System.Serializable]
    public class RencanaMiniQuest
    {
        public KategoriItem kategori;
        public List<string> judul;
        public List<string> deskripsiAwal;
        public List<string> deskripsiAkhir;
    }

    public List<MiniQuestList> miniQuestLists = new List<MiniQuestList>();
    public List<RencanaMiniQuest> semuaRencanaMiniQuest = new List<RencanaMiniQuest>();
    public int currentDateMiniQuest;

    

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RandomMiniQuest()
    {
        if (semuaRencanaMiniQuest.Count == 0)
        {
            Debug.LogWarning("List Mini Quest kosong!");
            return;
        }

        int index = UnityEngine.Random.Range(0, semuaRencanaMiniQuest.Count);
        RencanaMiniQuest rencanaDipilih = semuaRencanaMiniQuest[index];

        // Pilih judul dan deskripsi secara acak dari kategori tersebut
        string judul = rencanaDipilih.judul[UnityEngine.Random.Range(0, rencanaDipilih.judul.Count)];
        string deskripsiAwal = rencanaDipilih.deskripsiAwal[UnityEngine.Random.Range(0, rencanaDipilih.deskripsiAwal.Count)];
        string deskripsiAkhir = rencanaDipilih.deskripsiAkhir[UnityEngine.Random.Range(0, rencanaDipilih.deskripsiAkhir.Count)];

        Debug.Log($"Mini Quest Acak: {judul}\n{deskripsiAwal}\n{deskripsiAkhir}");
    }

}
