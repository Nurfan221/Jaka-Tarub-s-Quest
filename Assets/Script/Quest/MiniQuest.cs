using System;
using System.Collections.Generic;
using System.Linq;
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
        public string deskripsi;

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
    private int maxItemCount = 3;
    private int maxItem = 3;
    public int countquest;

    [Header("Daftar Hubungan")]
    [SerializeField] public NPCManager npcManager;
    [SerializeField] public TimeManager timeManager;

    void Start()
    {
        RandomMiniQuest();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RandomMiniQuest()
    {
        // Cek apakah list rencana tersedia
        if (semuaRencanaMiniQuest.Count == 0)
        {
            Debug.LogWarning("List Mini Quest kosong!");
            return;
        }

        // Pilih rencana quest secara acak
        int indexRencana = UnityEngine.Random.Range(0, semuaRencanaMiniQuest.Count);
        Debug.Log("nilai index rencana" + indexRencana);
        RencanaMiniQuest rencanaDipilih = semuaRencanaMiniQuest[indexRencana];

        // Pilih NPC acak dari manager
        int indexNPC = UnityEngine.Random.Range(0, npcManager.npcDataArray.Length);
        GameObject npcDipilih = npcManager.npcDataArray[indexNPC].prefab;

        // Buat objek MiniQuest baru
        MiniQuestList inputMiniQuest = new MiniQuestList
        {
            npc = npcDipilih
        };

        // Ambil item quest berdasarkan kategori
        List<Item> itemQuest = RandomItemforMiniQuest(indexRencana);

        inputMiniQuest.itemsQuest = itemQuest;

        // Pilih satu item untuk ditampilkan di judul
        string GetSatuan(Item item)
        {
            if ((item.categories & ItemCategory.Fruit) != 0) return "buah";
            if ((item.categories & ItemCategory.Vegetable) != 0) return "buah";
            if ((item.categories & ItemCategory.Meat) != 0) return "potong";
            return "unit";
        }
        string semuaNamaItem = string.Join(", ",
    inputMiniQuest.itemsQuest.Select(item => $"{item.itemName} x{item.stackCount} {GetSatuan(item)}"));


        // Pilih judul dan deskripsi acak lalu gabungkan
        string judulAcak = rencanaDipilih.judul[UnityEngine.Random.Range(0, rencanaDipilih.judul.Count)];
        string deskripsiAwal = rencanaDipilih.deskripsiAwal[UnityEngine.Random.Range(0, rencanaDipilih.deskripsiAwal.Count)];
        string deskripsiAkhir = rencanaDipilih.deskripsiAkhir[UnityEngine.Random.Range(0, rencanaDipilih.deskripsiAkhir.Count)];

        // ambil tanggal hari ini
        currentDateMiniQuest = timeManager.date + 3;

        // Bangun kalimat utuh
        string judulLengkap = $"{npcDipilih.name} {judulAcak}";
        string deskripsiGabungan = $"{deskripsiAwal} {semuaNamaItem} Berikan item itu Sebelum tanggal {currentDateMiniQuest} , {deskripsiAkhir}";

        //input reward berdasarkan jumlah item
        int randomReward = 0;

        switch (countquest)
        {
            case 1:
                randomReward = UnityEngine.Random.Range(100, 300);
                break;
            case 2:
                randomReward = UnityEngine.Random.Range(300, 500);
                break;
            case 3:
                randomReward = UnityEngine.Random.Range(500, 700);
                break;
            default:
                randomReward = 100; // fallback nilai default jika indexRencana tidak valid
                break;
        }

        //ambil item untuk hadiah quest 
        Item randomItemReward = null;

        foreach (var group in ItemPool.Instance.itemCategoryGroups)
        {
            if (group.categories == ItemCategory.Food && group.items.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, group.items.Count);
                randomItemReward = group.items[index];
                break; // keluar setelah menemukan kategori yang cocok
            }
        }



        // Masukkan ke dalam quest
        inputMiniQuest.judulQuest = judulLengkap;
        inputMiniQuest.DateMiniQuest = currentDateMiniQuest; // Jika kamu ingin menyimpan tanggal
        inputMiniQuest.countItemQuest = itemQuest.Select(i => i.stackCount).ToList(); // jika kamu pakai stackCount
        inputMiniQuest.deskripsi = deskripsiGabungan;
        inputMiniQuest.rewardQuest = randomReward;
        inputMiniQuest.rewardItemQuest = randomItemReward;

        //ubah nilai stackcount item menjadi 0
        foreach (Item item in itemQuest)
        {
            item.stackCount = 0;
        }
        // Simpan ke list
        miniQuestLists.Add(inputMiniQuest);

        // Debug
        Debug.Log($"Mini Quest Terpilih:\nJudul: {judulLengkap}\nDeskripsi: {deskripsiGabungan}\n");
    }


    public List<Item> RandomItemforMiniQuest(int random)
    {
        List<Item> itemtoMiniQuest = new List<Item>();

        switch (random)
        {
            case 0: // Vegetable
                foreach (var group in ItemPool.Instance.itemCategoryGroups)
                {
                    if (group.categories == ItemCategory.Vegetable)
                    {
                        int randomItemValue = UnityEngine.Random.Range(1, maxItem + 1); // Hindari 0
                        countquest = randomItemValue;
                        for (int i = 0; i < randomItemValue; i++)
                        {
                            int randomItemIndex = UnityEngine.Random.Range(0, group.items.Count);
                            int randomItemCount = UnityEngine.Random.Range(1, maxItemCount + 1); // Hindari 0

                            Item originalItem = group.items[randomItemIndex];
                            Item newItem = ItemPool.Instance.AddNewItem(originalItem, randomItemCount);
                            newItem.stackCount = randomItemCount;

                            itemtoMiniQuest.Add(newItem);
                        }

                        return itemtoMiniQuest;
                    }
                }
                break;
            case 1:
                foreach (var group in ItemPool.Instance.itemCategoryGroups)
                {
                    if (group.categories == ItemCategory.Fruit)
                    {
                        int randomItemValue = UnityEngine.Random.Range(1, maxItem + 1); // Hindari 0
                        countquest = randomItemValue;
                        for (int i = 0; i < randomItemValue; i++)
                        {
                            int randomItemIndex = UnityEngine.Random.Range(0, group.items.Count);
                            int randomItemCount = UnityEngine.Random.Range(1, maxItemCount + 1); // Hindari 0

                            Item originalItem = group.items[randomItemIndex];
                            Item newItem = ItemPool.Instance.AddNewItem(originalItem, randomItemCount);
                            newItem.stackCount = randomItemCount;

                            itemtoMiniQuest.Add(newItem);
                        }

                        return itemtoMiniQuest;
                    }
                }
                break;
            case 2:
                foreach (var group in ItemPool.Instance.itemCategoryGroups)
                {
                    if (group.categories == ItemCategory.Hunt || group.categories == ItemCategory.Meat)
                    {
                        int randomItemValue = UnityEngine.Random.Range(1, maxItem + 1); // Hindari 0
                        countquest = randomItemValue;
                        for (int i = 0; i < randomItemValue; i++)
                        {
                            int randomItemIndex = UnityEngine.Random.Range(0, group.items.Count);
                            int randomItemCount = UnityEngine.Random.Range(1, maxItemCount + 1); // Hindari 0

                            Item originalItem = group.items[randomItemIndex];
                            Item newItem = ItemPool.Instance.AddNewItem(originalItem, randomItemCount);
                            newItem.stackCount = randomItemCount;

                            itemtoMiniQuest.Add(newItem);
                        }

                        return itemtoMiniQuest;
                    }
                }
                break;
        }

        return null; // Jika tidak ada kategori yang cocok atau switch case lain
    }

}
