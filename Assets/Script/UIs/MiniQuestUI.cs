using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static MiniQuest;

public class MiniQuestUI : MonoBehaviour
{
    [SerializeField] MiniQuest miniQuest;
    public Transform miniQuest1;
    public Transform miniQuest2;
    public Transform tempalteItem1;
    public Transform tempalteItem2;
    public Transform contentItem1;
    public Transform contentItem2;
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
            miniQuest.RandomMiniQuest();
            miniQuest.RandomMiniQuest();
            UpdateUI();
            Debug.Log("button close di panggil");
        }
        );

    }

    private void CloseShop()
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
        TextMeshProUGUI deskripsiAkhir = parent.Find("DeskripsiAkhirQuest").GetComponent<TextMeshProUGUI>();

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
    }


    private void ClearChildUI(Transform parent, Transform template)
    {
        foreach (Transform child in parent)
        {
            if (child != template)
                Destroy(child.gameObject);
        }
    }
}
