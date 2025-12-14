using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MiniQuest;

public class QuestInfoUI : MonoBehaviour
{
    public static QuestInfoUI Instance { get; private set; }



    [Header("UI STUFF")]
    public Transform ContentInfo;
    public Transform SlotTemplateInfo;
    public Transform ContentDetail;
    public Transform SlotTemplateDetail;

    [Header("image UI")]
    public Sprite sliderActive;
    public Sprite sliderInactive;
    public Transform deskripsi;
    public Transform deskripsiContent;
    public bool isDeskripsi;
    [Header("Menu Panels QuestInfo")]
    public TMP_Text questInfoPanelName;
    public TMP_Text deskripsiQuestPanelName;
    public Transform itemQuestContentGo;
    public Transform itemQuestSlotTemplate;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }



    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    //public void DisplayActiveQuest(TemplateQuest questActive)
    //{
    //    Debug.Log("DisplayActiveQuest di jalankan");
    //    if (!quest.Contains(questActive)) // Cek apakah quest sudah ada
    //    {
    //        quest.Add(questActive);

    //    }
    //}


    public void RefreshActiveQuest()
    {

        //Hapus elemen lama sebelum menampilkan yang baru
        ClearChildrenExceptTemplate(ContentDetail, SlotTemplateDetail);
        ClearChildrenExceptTemplate(ContentInfo, SlotTemplateInfo);
        int questIndex = 0;
        foreach (var item in QuestManager.Instance.questActive)
        {
            foreach (var item1 in item.sideQuests)
            {
                TemplateQuest questData = item1;

                //Instansiasi UI untuk Quest
                Transform questDetail = Instantiate(SlotTemplateDetail, ContentDetail);
                Transform questInfo = Instantiate(SlotTemplateInfo, ContentInfo);
                questDetail.gameObject.SetActive(true);
                questInfo.gameObject.SetActive(true);
                questDetail.name = questData.questName;
                questInfo.name = questData.questName;

                //Ambil referensi komponen
                Image questImage = questInfo.GetChild(0).GetComponent<Image>();
                Image questImageInfo = questInfo.GetComponent<Image>();
                Image questDetailImage = questDetail.GetComponent<Image>();
                TMP_Text questTextInfo = questInfo.GetChild(1).GetComponent<TMP_Text>();
                TMP_Text questTextDetail = questDetail.GetChild(1).GetComponent<TMP_Text>();

                //Cek apakah quest aktif atau tidak
                if (questData.questProgress == QuestProgress.Accepted)
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
                questDetail.GetChild(1).GetComponent<TMP_Text>().text = questData.questName;
                questInfo.GetChild(1).GetComponent<TMP_Text>().text = questData.npcName;

                //Mengatur itemID berdasarkan indeks
                ItemDragandDrop itemDragAndDrop = questDetail.GetComponent<ItemDragandDrop>();
                if (itemDragAndDrop != null)
                {
                    itemDragAndDrop.index = questIndex; // <-- Gunakan counter
                }
                Button btnDeskripsi = questDetail.GetComponent<Button>();
                btnDeskripsi.onClick.AddListener(() =>
                {
                    if (!isDeskripsi)
                    {
                        // Menampilkan deskripsi
                        deskripsi.gameObject.SetActive(true);
                        deskripsiContent.gameObject.SetActive(true);


                        // Mengatur nama quest
                        questInfoPanelName.text = questData.questName;

                        // Menyusun deskripsi quest
                        string fullDescription = questData.DeskripsiAwal;

                        //Hapus elemen lama sebelum menampilkan yang baru
                        ClearChildrenExceptTemplate(itemQuestContentGo, itemQuestSlotTemplate);

                        // Menambahkan jumlah item yang ada di itemQuest ke dalam deskripsi
                        foreach (ItemData itemDataQuest in questData.itemRequirements)
                        {
                            //fullDescription += $" {itemQuest.stackCount} buah {itemQuest.itemName} kepada {quest.NPC.name} "; // Menggunakan itemName atau field lainnya dari Item
                            Item itemQuest = ItemPool.Instance.GetItem(itemDataQuest.itemName);                                                                                                  //Instansiasi UI untuk Quest
                            Transform itemQuestDetail = Instantiate(itemQuestSlotTemplate, itemQuestContentGo);
                            itemQuestDetail.gameObject.SetActive(true);

                            // Mengakses objek Image pertama yang ada pada objek instansiasi
                            Image itemImage = itemQuestDetail.GetChild(0).GetComponent<Image>();
                            TMP_Text itemName = itemQuestDetail.GetChild(1).GetComponent<TMP_Text>();
                            TMP_Text jumlah = itemQuestDetail.GetChild(2).GetComponent<TMP_Text>();

                            itemImage.sprite = itemQuest.sprite;
                            itemName.text = itemQuest.itemName;
                            jumlah.text = itemDataQuest.count.ToString();


                        }

                        // Menambahkan deskripsi akhir
                        fullDescription += questData.DeskripsiAkhir;

                        // Memasukkan ke dalam questDeskripsi
                        deskripsiQuestPanelName.text = fullDescription;

                        isDeskripsi = true;
                    }
                    else
                    {
                        deskripsi.gameObject.SetActive(false);
                        isDeskripsi = false;

                    }
                });
                questIndex++;
            }
        }

        //for (int i = 0; i < quest.Count; i++)
        //{


        //}
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



    //public void SetQuestInActive(string questName)
    //{
    //    Debug.Log("SetQuestInActive di jalankan");
    //    //Set quest menjadi tidak aktif
    //    foreach (var quest in quest)
    //    {
    //        if (quest.questName == questName)
    //        {
    //            quest.questProgress = QuestProgress.Completed;

    //        }
    //    }

    //    //Hapus quest dari list jika sudah tidak aktif
    //    quest.RemoveAll(q => q.questName == questName && q.questProgress == QuestProgress.Completed);

    //    // Refresh UI untuk menghilangkan quest yang sudah tidak aktif
    //    RefreshActiveQuest();
    //}

    public void AddMiniQuestActive(MiniQuestList miniQuest)
    {
        //questManager.currentMiniQuest = miniQuest;
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
                ClearChildrenExceptTemplate(itemQuestContentGo, itemQuestSlotTemplate);

                // Menambahkan jumlah item yang ada di itemQuest ke dalam deskripsi
                foreach (Item itemQuest in miniQuest.itemsQuest)
                {
                    //fullDescription += $" {itemQuest.stackCount} buah {itemQuest.itemName} kepada {miniQuest.npc.name} "; // Menggunakan itemName atau field lainnya dari Item
                    //Instansiasi UI untuk Quest
                    Transform itemQuestDetail = Instantiate(itemQuestContentGo, itemQuestSlotTemplate);
                    itemQuestDetail.gameObject.SetActive(true);

                    // Mengakses objek Image pertama yang ada pada objek instansiasi
                    Image itemImage = itemQuestDetail.GetChild(0).GetComponent<Image>();
                    TMP_Text itemName = itemQuestDetail.GetChild(1).GetComponent<TMP_Text>();
                    TMP_Text jumlah = itemQuestDetail.GetChild(2).GetComponent<TMP_Text>();

                    itemImage.sprite = itemQuest.sprite;
                    itemName.text = itemQuest.itemName;
                    //jumlah.text = itemQuest.stackCount.ToString();


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
