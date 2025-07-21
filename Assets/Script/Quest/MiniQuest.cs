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
        public int questID; 
        public string judulQuest;
        public List<Item> itemsQuest = new List<Item>();
        public int DateMiniQuest;
        public int rewardQuest;
        public Item rewardItemQuest;
        public GameObject npc;
        public string deskripsiAwal;
        public string deskripsiAkhir;
        public bool questActive = false;
        public bool questComplete = false;
        public Dialogues finishDialogue;
        public Dialogues rewardDialogueQuest;

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
    private int maxItemCount = 10;
    private int maxItem = 3;
    public int countquest;
    private int currentIndexNpc;
    public Dialogues finishQuest;
    public Dialogues rewardQuest;

    [Header("Daftar Hubungan")]
    //[SerializeField] public NPCManager npcManager;
    [SerializeField] public TimeManager timeManager;

    void Start()
    {
        //RandomMiniQuest();
        //RandomMiniQuest();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //public void RandomMiniQuest()
    //{
    //    if (miniQuestLists.Count > 1)
    //    {
    //        Debug.Log("hapus item dari mini qeust list");
    //        miniQuestLists.Clear();
    //    }

    //    // Cek apakah list rencana tersedia
    //    if (semuaRencanaMiniQuest.Count == 0)
    //    {
    //        Debug.LogWarning("List Mini Quest kosong!");
    //        return;
    //    }

    //    // Pilih rencana quest secara acak
    //    int indexRencana = UnityEngine.Random.Range(0, semuaRencanaMiniQuest.Count);

    //    Debug.Log("nilai index rencana" + indexRencana);
    //    RencanaMiniQuest rencanaDipilih = semuaRencanaMiniQuest[indexRencana];

    //    // Pilih NPC acak dari manager
    //    int indexNPC = UnityEngine.Random.Range(0, npcManager.npcDataArray.Length);

    //    // Pastikan NPC yang dipilih berbeda dari NPC yang sebelumnya
    //    while (indexNPC == currentIndexNpc)
    //    {
    //        indexNPC = UnityEngine.Random.Range(0, npcManager.npcDataArray.Length);
    //    }

    //    currentIndexNpc = indexNPC;
    //    GameObject npcDipilih = npcManager.npcDataArray[indexNPC].prefab;


    //    // Buat objek MiniQuest baru
    //    MiniQuestList inputMiniQuest = new MiniQuestList
    //    {
    //        npc = npcDipilih
    //    };

    //    // Ambil item quest berdasarkan kategori
    //    List<Item> itemQuest = RandomItemforMiniQuest(indexRencana);

    //    inputMiniQuest.itemsQuest = itemQuest;

    //    // Pilih satu item untuk ditampilkan di judul
    //    string GetSatuan(Item item)
    //    {
    //        if ((item.categories & ItemCategory.Fruit) != 0) return "buah";
    //        if ((item.categories & ItemCategory.Vegetable) != 0) return "buah";
    //        if ((item.categories & ItemCategory.Meat) != 0) return "potong";
    //        return "unit";
    //    }
    //    string semuaNamaItem = string.Join(", ",
    //    inputMiniQuest.itemsQuest.Select(item => $"{item.itemName}  {GetSatuan(item)}"));


    //    // Pilih judul dan deskripsi acak lalu gabungkan
    //    string judulAcak = rencanaDipilih.judul[UnityEngine.Random.Range(0, rencanaDipilih.judul.Count)];
    //    string deskripsiAwal = rencanaDipilih.deskripsiAwal[UnityEngine.Random.Range(0, rencanaDipilih.deskripsiAwal.Count)];
    //    string deskripsiAkhir = rencanaDipilih.deskripsiAkhir[UnityEngine.Random.Range(0, rencanaDipilih.deskripsiAkhir.Count)];

    //    // ambil tanggal hari ini
    //    currentDateMiniQuest = timeManager.timeData_SO.date + 3;

    //    // Bangun kalimat utuh
    //    string judulLengkap = $"{npcDipilih.name} {judulAcak}";
    //    string deskripsiGabunganAwal = $"{deskripsiAwal} Berikan item itu Sebelum tanggal {currentDateMiniQuest}";
    //    string deskripsiGabunganAkhir = deskripsiAkhir;

    //    //input reward berdasarkan jumlah item
    //    int randomReward = 0;
    //    foreach (Item item in itemQuest)
    //    {
    //        //randomReward += item.stackCount;
    //    }

    //    if (randomReward > 0 && randomReward <= 10)
    //    {
    //        randomReward = UnityEngine.Random.Range(250, 500);
    //    }else if (randomReward > 10 && randomReward <= 20)
    //    {
    //        randomReward = UnityEngine.Random.Range(500, 750);
    //    }else if(randomReward > 20 && randomReward <= 30)
    //    {
    //        randomReward = UnityEngine.Random.Range(750, 1000);
    //    }

    //    //ambil item untuk hadiah quest 
    //    Item randomItemReward = null;

    //    foreach (var group in ItemPool.Instance.itemCategoryGroups)
    //    {
    //        if (group.categories == ItemCategory.Food && group.items.Count > 0)
    //        {
    //            int index = UnityEngine.Random.Range(0, group.items.Count);
    //            randomItemReward = group.items[index];
    //            break; // keluar setelah menemukan kategori yang cocok
    //        }
    //    }



    //    // Masukkan ke dalam quest
    //    inputMiniQuest.judulQuest = judulLengkap;
    //    inputMiniQuest.DateMiniQuest = currentDateMiniQuest; // Jika kamu ingin menyimpan tanggal
    //    inputMiniQuest.deskripsiAwal = deskripsiGabunganAwal;
    //    inputMiniQuest.deskripsiAkhir = deskripsiAkhir;
    //    inputMiniQuest.rewardQuest = randomReward;
    //    inputMiniQuest.rewardItemQuest = randomItemReward;
    //    inputMiniQuest.finishDialogue = finishQuest;
    //    inputMiniQuest.rewardDialogueQuest = rewardQuest;

    //    ////ubah nilai stackcount item menjadi 0
    //    //foreach (Item item in itemQuest)
    //    //{
    //    //    item.stackCount = 0;
    //    //}
    //    // Simpan ke list

    //    miniQuestLists.Add(inputMiniQuest);
    //    for(int i = 0; i < miniQuestLists.Count; i++)
    //    {
    //        miniQuestLists[i].questID = i;
    //    }

    //    // Debug

    //}


    public List<Item> RandomItemforMiniQuest(int random)
    {
        List<Item> itemToMiniQuest = new List<Item>();
        List<Item> poolItems = new List<Item>();

        switch (random)
        {
            case 0: // Vegetable
            case 1: // Fruit
                ItemCategory targetCategory = (random == 0) ? ItemCategory.Vegetable : ItemCategory.Fruit;

                foreach (var group in ItemPool.Instance.itemCategoryGroups)
                {
                    if (group.categories == targetCategory)
                    {
                        poolItems.AddRange(group.items);
                    }
                }
                break;

            case 2: // Hunt or Meat
                foreach (var group in ItemPool.Instance.itemCategoryGroups)
                {
                    if (group.categories == ItemCategory.Hunt || group.categories == ItemCategory.Meat)
                    {
                        poolItems.AddRange(group.items);
                    }
                }
                break;

            default:
                return itemToMiniQuest;
        }

        // Jika tidak ada item ditemukan
        if (poolItems.Count == 0) return itemToMiniQuest;

        int randomItemValue = UnityEngine.Random.Range(1, maxItem + 1);
        countquest = randomItemValue;

        HashSet<Item> usedItems = new HashSet<Item>();


        for (int i = 0; i < randomItemValue; i++)
        {
            Item originalItem;
            int tries = 0; // Untuk jaga-jaga supaya gak infinite loop

            do
            {
                int randomItemIndex = UnityEngine.Random.Range(0, poolItems.Count);
                originalItem = poolItems[randomItemIndex];
                tries++;
            } while (usedItems.Contains(originalItem) && tries < 100);

            usedItems.Add(originalItem);


            int randomItemCount = UnityEngine.Random.Range(1, maxItemCount + 1);
            Item newItem = ItemPool.Instance.GetItemWithQuality(originalItem.name, originalItem.quality);
            //newItem.stackCount = randomItemCount;
            itemToMiniQuest.Add(newItem);

        }

        return itemToMiniQuest;
    }



}
