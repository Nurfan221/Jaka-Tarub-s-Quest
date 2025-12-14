using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCListUI : MonoBehaviour
{
    [Header("Daftar Hubungan")]
    [SerializeField] NPCManager npcManager;

    [Header("Database Definisi NPC")]
    public List<NpcSO> allNpcDefinitions; // Seret semua aset NpcSO Anda ke sini
    public NpcSO npcData; // Data NPC yang sedang ditampilkan di deskripsi

    [Header("UI STUFF")]
    [SerializeField] Transform ContentList;
    [SerializeField] Transform SlotTemplateList;

    [Header("NPC Deskripsi")]
    public Transform npcDeskripsi;
    public Transform namaLengkap;
    public Image fotoProfil;
    public Transform[] npcLoved;
    public int barPerasaan;
    public bool apakahMemberi;
    public bool apakahMenyapa;
    public Image statusMemberi;
    public Image statusMenyapa;
    public Sprite iconMemberi;
    public Sprite iconMenyapa;
    public Transform ulangTahun;
    public Transform pekerjaan;
    public Transform hobi;


    void Start()
    {
        //RefreshNPCList(); // Tambahkan pemanggilan fungsi di Start jika ingin langsung memuat daftar NPC
    }

    public void RefreshNPCList()
    {
        Debug.Log("Memanggil fungsi RefreshNPCList");

        ClearChildrenExceptTemplate(ContentList, SlotTemplateList);

        foreach (var npc in allNpcDefinitions)
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
        TMP_Text targetNama = namaLengkap.GetComponent<TMP_Text>();
        targetNama.text = npcData.fullName;

        TMP_Text targetUltah = ulangTahun.GetComponent<TMP_Text>();
        targetUltah.text = "Ulang Tahun : " + npcData.tanggalUltah.ToString() + "/" + npcData.bulanUltah.ToString();

        TMP_Text targetperkerjaan = pekerjaan.GetComponent<TMP_Text>();
        targetperkerjaan.text = "Pekerjaan : " + npcData.pekerjaan;

        TMP_Text targetHobi = hobi.GetComponent<TMP_Text>();
        targetHobi.text = "Hobi : " + npcData.hobi;
    }
}
