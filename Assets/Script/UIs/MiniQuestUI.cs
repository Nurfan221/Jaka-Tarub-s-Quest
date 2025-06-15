using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static MiniQuest;

public class MiniQuestUI : MonoBehaviour
{
    [SerializeField] MiniQuest miniQuest;
    [SerializeField] QuestManager questManager;
    [SerializeField] QuestInfoUI questInfoUI;
    public Transform miniQuest1;
    public Transform miniQuest2;
    public Transform tempalteItem1;
    public Transform tempalteItem2;
    public Transform contentItem1;
    public Transform contentItem2;
    public Transform bgActiveQuest1;
    public Transform bgActiveQuest2;
    public Button btnClose;

    public void Start()
    {
        
    }
    public void Open()
    {
        GameController.Instance.ShowPersistentUI(false);
        GameController.Instance.PauseGame();
        gameObject.SetActive(true);
        UpdateUI();

        btnClose.onClick.RemoveAllListeners();

        btnClose.onClick.AddListener(() =>
        {
            //miniQuest.RandomMiniQuest();
            //miniQuest.RandomMiniQuest();
            //UpdateUI();
            Close();
            Debug.Log("button close di panggil");
        }
        );

    }

    private void Close()
    {
      
        GameController.Instance.ResumeGame();
        // Tutup UI Storage
        gameObject.SetActive(false);
        GameController.Instance.ShowPersistentUI(true);
        gameObject.SetActive(false);
    }



    public void UpdateUI()
    {
        if (miniQuest.miniQuestLists.Count > 0 && miniQuest.miniQuestLists[0] != null)
            SetQuestUI(miniQuest1, miniQuest.miniQuestLists[0], contentItem1, tempalteItem1);

        if (miniQuest.miniQuestLists.Count > 1 && miniQuest.miniQuestLists[1] != null)
            SetQuestUI(miniQuest2, miniQuest.miniQuestLists[1], contentItem2, tempalteItem2);

    }

    void SetQuestUI(Transform parent, MiniQuestList questData, Transform contentContainer, Transform itemTemplate)
    {
        TextMeshProUGUI judul = parent.Find("JudulQuest").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI deskripsiAwal = parent.Find("DeskripsiAwalQuest").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI reward = parent.Find("Reward").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI deskripsiAkhir = parent.Find("DeskripsiAkhirQuest").GetComponent<TextMeshProUGUI>();
        Transform contentRewardItem = parent.Find("ItemReward").GetComponent<Transform>();
        Transform templateRewardItem = contentRewardItem.Find("itemTemplate").GetComponent<Transform>();
        Image imageTemplateRewardItem = templateRewardItem.Find("Image").GetComponent<Image>();
        Button buttonTake = parent.Find("Ambil").GetComponent<Button>();
        buttonTake.onClick.RemoveAllListeners();

        ClearChildUI(contentContainer, itemTemplate);

        foreach (var item in questData.itemsQuest)
        {
            Transform itemSlot = Instantiate(itemTemplate, contentContainer);
            itemSlot.gameObject.SetActive(true);
            Image itemSlotImage = itemSlot.Find("Image").GetComponent<Image>();
            TextMeshProUGUI itemSlotCount = itemSlot.Find("Jumlah").GetComponent<TextMeshProUGUI>();

            itemSlotImage.sprite = item.sprite;
            itemSlotCount.text = item.stackCount.ToString();
        }

        judul.text = questData.judulQuest;
        deskripsiAwal.text = questData.deskripsiAwal;
        deskripsiAkhir.text = questData.deskripsiAkhir;
        imageTemplateRewardItem.sprite = questData.rewardItemQuest.sprite;
        buttonTake.onClick.AddListener(() =>
        {
            questManager.CreateQuestDisplay(questData.judulQuest);
            TakeMiniQuest(questData.questID);
            questInfoUI.AddMiniQuestActive(questData);
            buttonTake.gameObject.SetActive(false);
        });
        reward.text = $"Hadiah yang kamu dapatkan : {questData.rewardQuest.ToString()} Tod" ;
    }


    private void ClearChildUI(Transform parent, Transform template)
    {
        foreach (Transform child in parent)
        {
            if (child != template)
                Destroy(child.gameObject);
        }
    }

    public void TakeMiniQuest(int id)
    {
        foreach(var quest in miniQuest.miniQuestLists)
        {
            if (quest.questID == id)
            {
                quest.questActive = true;

                
            }
        }

        switch(id)
        {
            case 0:
                bgActiveQuest2.gameObject.SetActive(true);
                break;
            case 1:
                bgActiveQuest1.gameObject.SetActive(true);
                break;
            default:
                bgActiveQuest1.gameObject.SetActive(false);
                bgActiveQuest2.gameObject.SetActive(false);
                break ;


        }
    }

    
}
