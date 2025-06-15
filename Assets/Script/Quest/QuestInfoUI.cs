using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MiniQuest;
using static QuestManager;

public class QuestInfoUI : MonoBehaviour
{
    [SerializeField] QuestManager questManager;
    public List<Quest> quests = new List<Quest>(); // List untuk menampung data Quest dari QuestManager


    [Header("UI STUFF")]
    [SerializeField] Transform ContentInfo;
    [SerializeField] Transform SlotTemplateInfo;
    [SerializeField] Transform ContentDetail;
    [SerializeField] Transform SlotTemplateDetail;

    [Header("image UI")]
    public Sprite sliderActive;
    public Sprite sliderInactive;
    public Transform deskripsi;
    public Transform deskripsiContent;
    public bool isDeskripsi;
    public Transform itemDeskripsi;
    public Transform templateItemDeskripsi;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DisplayActiveQuest(Quest questActive)
    {
        if (!quests.Contains(questActive)) // Cek apakah quest sudah ada
        {
            quests.Add(questActive);
            RefreshActiveQuest();
        }
    }


    public void RefreshActiveQuest()
    {
        //Hapus elemen lama sebelum menampilkan yang baru
        ClearChildrenExceptTemplate(ContentDetail, SlotTemplateDetail);
        ClearChildrenExceptTemplate(ContentInfo, SlotTemplateInfo);

        for (int i = 0; i < quests.Count; i++)
        {
            Quest quest = quests[i];

            //Instansiasi UI untuk Quest
            Transform questDetail = Instantiate(SlotTemplateDetail, ContentDetail);
            Transform questInfo = Instantiate(SlotTemplateInfo, ContentInfo);
            questDetail.gameObject.SetActive(true);
            questInfo.gameObject.SetActive(true);
            questDetail.name = quest.questDetail;
            questInfo.name = quest.questInfo;

            //Ambil referensi komponen
            Image questImage = questInfo.GetChild(0).GetComponent<Image>();
            Image questImageInfo = questInfo.GetComponent<Image>();
            Image questDetailImage = questDetail.GetComponent<Image>();
            TMP_Text questTextInfo = questInfo.GetChild(1).GetComponent<TMP_Text>();
            TMP_Text questTextDetail = questDetail.GetChild(1).GetComponent<TMP_Text>();

            //Cek apakah quest aktif atau tidak
            if (quest.questActive)
            {
                Debug.Log("Quest Active");
                questImage.sprite = sliderActive;
                SetOpacity(questImage, 1f);
            }
            else
            {
                Debug.Log("Quest Inactive");
                questImage.sprite = sliderInactive;
                SetOpacity(questDetailImage, 0.5f);
                SetOpacity(questImageInfo, 0.5f);
                SetOpacity(questImage, 0.8f);
                SetOpacity(questTextInfo, 0.5f);
                SetOpacity(questTextDetail, 0.5f);
            }

            //Set teks quest
            questDetail.GetChild(1).GetComponent<TMP_Text>().text = quest.questDetail;
            questInfo.GetChild(1).GetComponent<TMP_Text>().text = quest.questInfo;

            //Mengatur itemID berdasarkan indeks
            ItemDragandDrop itemDragAndDrop = questDetail.GetComponent<ItemDragandDrop>();
            if (itemDragAndDrop != null)
            {
                itemDragAndDrop.itemID = i;
            }
            Button btnDeskripsi = questDetail.GetComponent<Button>();
            btnDeskripsi.onClick.AddListener(() =>
            {
                if (!isDeskripsi)
                {
                    // Menampilkan deskripsi
                    deskripsi.gameObject.SetActive(true);
                    deskripsiContent.gameObject.SetActive(true);


                    // Mengambil teks quest name
                    TMP_Text questName = deskripsiContent.GetChild(1).GetComponent<TMP_Text>();
                    TMP_Text questDeskripsi = deskripsiContent.GetChild(2).GetComponent<TMP_Text>();

                    // Mengatur nama quest
                    questName.text = quest.questName;

                    // Menyusun deskripsi quest
                    string fullDescription = quest.deskripsiAwal;

                    //Hapus elemen lama sebelum menampilkan yang baru
                    ClearChildrenExceptTemplate(itemDeskripsi, templateItemDeskripsi);

                    // Menambahkan jumlah item yang ada di itemQuest ke dalam deskripsi
                    foreach (Item itemQuest in quest.itemQuests)
                    {
                        fullDescription += $" {itemQuest.stackCount} buah {itemQuest.itemName} kepada {quest.NPC.name} "; // Menggunakan itemName atau field lainnya dari Item
                                                                                                                          //Instansiasi UI untuk Quest
                        Transform itemQuestDetail = Instantiate(templateItemDeskripsi, itemDeskripsi);
                        itemQuestDetail.gameObject.SetActive(true);

                        // Mengakses objek Image pertama yang ada pada objek instansiasi
                        Image itemImage = itemQuestDetail.GetChild(0).GetComponent<Image>();
                        TMP_Text itemName = itemQuestDetail.GetChild(1).GetComponent<TMP_Text>();
                        TMP_Text jumlah = itemQuestDetail.GetChild(2).GetComponent<TMP_Text>();

                        itemImage.sprite = itemQuest.sprite;
                        itemName.text = itemQuest.itemName;
                        jumlah.text = itemQuest.stackCount.ToString();


                    }

                    // Menambahkan deskripsi akhir
                    fullDescription += quest.deskripsiAkhir;

                    // Memasukkan ke dalam questDeskripsi
                    questDeskripsi.text = fullDescription;

                    isDeskripsi = true;
                } else
                {
                    deskripsi.gameObject.SetActive(false);
                    isDeskripsi = false;

                }
            });

        }
    }

    //Fungsi untuk menghapus child kecuali template
    private void ClearChildrenExceptTemplate(Transform parent, Transform template)
    {
        foreach (Transform child in parent)
        {
            if (child != template)
                Destroy(child.gameObject);
        }
    }

    //Fungsi untuk mengatur opacity elemen UI
    private void SetOpacity(Graphic element, float alpha)
    {
        if (element != null)
        {
            Color newColor = element.color;
            newColor.a = alpha;
            element.color = newColor;
        }
    }



    public void SetQuestInActive(string questName)
    {
        Debug.Log("SetQuestInActive di jalankan");
        // Set quest menjadi tidak aktif
        foreach (var quest in quests)
        {
            if (quest.questName == questName)
            {
                quest.questActive = false;

            }
        }

        // Hapus quest dari list jika sudah tidak aktif
        //quests.RemoveAll(q => q.questName == questName && !q.questActive);

        // Refresh UI untuk menghilangkan quest yang sudah tidak aktif
        RefreshActiveQuest();
    }

    public void AddMiniQuestActive(MiniQuestList miniQuest)
    {
        questManager.currentMiniQuest = miniQuest;
        //Instansiasi UI untuk Quest
        Transform questDetail = Instantiate(SlotTemplateDetail, ContentDetail);
        Transform questInfo = Instantiate(SlotTemplateInfo, ContentInfo);
        questDetail.gameObject.SetActive(true);
        questInfo.gameObject.SetActive(true);
        questDetail.name = miniQuest.judulQuest;
        questInfo.name = miniQuest.judulQuest;

        //Ambil referensi komponen
        Image questImage = questInfo.GetChild(0).GetComponent<Image>();
        Image questImageInfo = questInfo.GetComponent<Image>();
        Image questDetailImage = questDetail.GetComponent<Image>();
        TMP_Text questTextInfo = questInfo.GetChild(1).GetComponent<TMP_Text>();
        TMP_Text questTextDetail = questDetail.GetChild(1).GetComponent<TMP_Text>();

        //Cek apakah quest aktif atau tidak
        if (miniQuest.questActive)
        {
            Debug.Log("Quest Active");
            questImage.sprite = sliderActive;
            SetOpacity(questImage, 1f);
        }
        else
        {
            Debug.Log("Quest Inactive");
            questImage.sprite = sliderInactive;
            SetOpacity(questDetailImage, 0.5f);
            SetOpacity(questImageInfo, 0.5f);
            SetOpacity(questImage, 0.8f);
            SetOpacity(questTextInfo, 0.5f);
            SetOpacity(questTextDetail, 0.5f);
        }

        //Set teks quest
        questDetail.GetChild(1).GetComponent<TMP_Text>().text = miniQuest.judulQuest;
        questInfo.GetChild(1).GetComponent<TMP_Text>().text = miniQuest.judulQuest;

        //Mengatur itemID berdasarkan indeks
        //ItemDragandDrop itemDragAndDrop = questDetail.GetComponent<ItemDragandDrop>();
        //if (itemDragAndDrop != null)
        //{
        //    itemDragAndDrop.itemID = i;
        //}
        Button btnDeskripsi = questDetail.GetComponent<Button>();
        btnDeskripsi.onClick.AddListener(() =>
        {
            if (!isDeskripsi)
            {
                // Menampilkan deskripsi
                deskripsi.gameObject.SetActive(true);
                deskripsiContent.gameObject.SetActive(true);


                // Mengambil teks quest name
                TMP_Text questName = deskripsiContent.GetChild(1).GetComponent<TMP_Text>();
                TMP_Text questDeskripsi = deskripsiContent.GetChild(2).GetComponent<TMP_Text>();

                // Mengatur nama quest
                questName.text = miniQuest.judulQuest;

                // Menyusun deskripsi quest
                string fullDescription = $"{miniQuest.deskripsiAwal} {miniQuest.deskripsiAkhir}";

                //Hapus elemen lama sebelum menampilkan yang baru
                ClearChildrenExceptTemplate(itemDeskripsi, templateItemDeskripsi);

                // Menambahkan jumlah item yang ada di itemQuest ke dalam deskripsi
                foreach (Item itemQuest in miniQuest.itemsQuest)
                {
                    fullDescription += $" {itemQuest.stackCount} buah {itemQuest.itemName} kepada {miniQuest.npc.name} "; // Menggunakan itemName atau field lainnya dari Item
                                                                                                                          //Instansiasi UI untuk Quest
                    Transform itemQuestDetail = Instantiate(templateItemDeskripsi, itemDeskripsi);
                    itemQuestDetail.gameObject.SetActive(true);

                    // Mengakses objek Image pertama yang ada pada objek instansiasi
                    Image itemImage = itemQuestDetail.GetChild(0).GetComponent<Image>();
                    TMP_Text itemName = itemQuestDetail.GetChild(1).GetComponent<TMP_Text>();
                    TMP_Text jumlah = itemQuestDetail.GetChild(2).GetComponent<TMP_Text>();

                    itemImage.sprite = itemQuest.sprite;
                    itemName.text = itemQuest.itemName;
                    jumlah.text = itemQuest.stackCount.ToString();


                }

                // Menambahkan deskripsi akhir
                fullDescription += miniQuest.deskripsiAkhir;

                // Memasukkan ke dalam questDeskripsi
                questDeskripsi.text = fullDescription;

                isDeskripsi = true;
            }
            else
            {
                deskripsi.gameObject.SetActive(false);
                isDeskripsi = false;

            }

        });

    }
}
