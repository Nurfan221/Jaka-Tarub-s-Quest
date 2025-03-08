using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NPCManager;
using static UnityEditor.Progress;

public class NPCListUI : MonoBehaviour
{
    [Header("Daftar Hubungan")]
    [SerializeField] NPCManager npcManager;
    public Transform npcDeskripsi;
    public NPCData npcData;

    [Header("UI STUFF")]
    [SerializeField] Transform ContentList;
    [SerializeField] Transform SlotTemplateList;


    void Start()
    {
        //RefreshNPCList(); // Tambahkan pemanggilan fungsi di Start jika ingin langsung memuat daftar NPC
    }

    public void RefreshNPCList()
    {
        Debug.Log("Memanggil fungsi RefreshNPCList");

        if (npcManager != null)
        {
            ClearChildrenExceptTemplate(ContentList, SlotTemplateList);

            foreach (var npc in npcManager.npcDataArray)
            {
                Transform npcList = Instantiate(SlotTemplateList, ContentList);
                npcList.gameObject.SetActive(true);
                npcList.name = npc.npcName;

                // Perbaikan: Mengubah teks pada hasil instansiasi, bukan template aslinya
                npcList.GetChild(1).GetComponent<TMP_Text>().text = npc.npcName;

                Button btnDeskripsi = npcList.GetComponent<Button>();

                btnDeskripsi.onClick.RemoveAllListeners();
                btnDeskripsi.onClick.AddListener(() =>
                {
                    npcData = npc;
                    NPCDeskripsi();
                });
            }
        }
        else
        {
            Debug.LogError("NPCManager belum terhubung!");
        }
    }

    private void ClearChildrenExceptTemplate(Transform parent, Transform template)
    {
        foreach (Transform child in parent)
        {
            if (child != template)
                Destroy(child.gameObject);
        }
    }

    private void NPCDeskripsi()
    {
        Debug.Log("memanggil fungsi npc deskripsi");
        npcDeskripsi.gameObject.SetActive(true);
        // Set jumlah item in inventory
        Transform textTransform = npcDeskripsi.transform.Find("NamaLengkap");
        if (textTransform != null)
        {
            textTransform.gameObject.SetActive(true);
            TMP_Text targetText = textTransform.GetComponent<TMP_Text>();
            targetText.text = npcData.fullName;
        }
        else
        {
            Debug.LogWarning("Text untuk item tidak ditemukan di dalam slot!");
        }
    }
}
