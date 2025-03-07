using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
        // Hapus elemen lama sebelum menampilkan yang baru
        foreach (Transform child in ContentDetail)
        {
            if (child == SlotTemplateDetail) continue;
            Destroy(child.gameObject);
        }

        foreach (Transform child in ContentInfo)
        {
            if (child == SlotTemplateInfo) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < quests.Count; i++)
        {
            Quest quest = quests[i];
            Transform questDetail = Instantiate(SlotTemplateDetail, ContentDetail);
            Transform questInfo = Instantiate(SlotTemplateInfo, ContentInfo);
            questInfo.gameObject.SetActive(true);
            questInfo.gameObject.name = quest.questInfo;
            questDetail.gameObject.SetActive(true);
            questDetail.gameObject.name = quest.questDetail;

            // Cek apakah quest aktif atau tidak
            if (quest.questActive)
            {
                // Jika aktif, gunakan sliderActive
                questDetail.GetChild(0).GetComponent<Image>().sprite = sliderActive;
                questInfo.GetChild(0).GetComponent<Image>().sprite = sliderActive;
            }
            else
            {
                // Jika tidak aktif, gunakan sliderInactive
                questDetail.GetChild(0).GetComponent<Image>().sprite = sliderInactive;
                questInfo.GetChild(0).GetComponent<Image>().sprite = sliderInactive;
            }

            // Set teks quest
            questDetail.GetChild(1).GetComponent<TMP_Text>().text = quest.questDetail;
            questInfo.GetChild(1).GetComponent<TMP_Text>().text = quest.questInfo;

            // Mengatur itemID berdasarkan indeks
            ItemDragandDrop itemDragAndDrop = questDetail.GetComponent<ItemDragandDrop>();
            if (itemDragAndDrop != null)
            {
                itemDragAndDrop.itemID = i; // Set itemID dengan indeks item
            }
        }
    }


    public void SetQuestInActive(string questName)
    {
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

}
