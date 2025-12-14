using System;
using System.Collections;
using UnityEngine;

public enum Lokasi
{
    None,
    Danau,
    Kota,
    Kebun,
    HutanAjaib,
    RumahJaka
}
public class LocationManager : MonoBehaviour
{
    [Serializable]
    public class GameObjectLocation
    {
        public string nameLocation;
        public GameObject Location;
        public Lokasi lokasiSaatIni = Lokasi.None; // Assign default ke None
    }

    public GameObjectLocation[] LocationArray;

    [Header("Daftar Hubungan")]
    [SerializeField] Player_Health player_Health;
    [SerializeField] DialogueSystem dialogueSystem;
    [SerializeField] QuestManager questManager;
    private Coroutine healingCoroutine;
    public float delayHealing = 2f; // Delay tiap heal dalam detik

    [Header("Daftar nilai bool")]
    public bool inLokasi;
    public bool mainQuestDanau;
    public bool mainQuestMisiYangTerlupakan;
    public bool hasDisplayedDanauDialogue = false;
    public bool exitDanauScene = false;
    public bool inDanau;
    public bool inRumahJaka;



    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (inDanau && healingCoroutine == null)
        {
            healingCoroutine = StartCoroutine(AreaHealingCoroutine());
        }

        // Kalau player keluar area
        if (!inDanau && healingCoroutine != null)
        {
            StopCoroutine(healingCoroutine);
            healingCoroutine = null;
        }
    }

    public void HandleLocationEnter(GameObjectLocation gol)
    {
        // Menampilkan nama lokasi atau melakukan logika tambahan lainnya
        Debug.Log($"Player memasuki lokasi: {gol.Location.name}, yang berjenis {gol.lokasiSaatIni}");

        // Bisa tambahkan logika lain, misalnya memunculkan UI nama lokasi, memberi dialog, dll.
        switch (gol.lokasiSaatIni)
        {
            case Lokasi.Danau:
                // Memastikan dialog hanya dipanggil sekali
                if (!hasDisplayedDanauDialogue)
                {
                    if (mainQuestDanau)
                    {
                        //dialogueSystem.theDialogues = questManager.currentMainQuest.dialogueQuest[6];
                        dialogueSystem.StartDialogue();
                        StartCoroutine(dialogueSystem.WaitForDialogueToEnd());
                    }
                    hasDisplayedDanauDialogue = true; // Set flag supaya tidak dipanggil lagi
                }
                inDanau = true;
                break;

            case Lokasi.RumahJaka:
                // Lakukan sesuatu di Rumah Jaka

                break;

            case Lokasi.HutanAjaib:
                // Bisa buat logika khusus untuk Hutan Ajaib
                break;

                // Tambahkan case lain sesuai lokasi yang ada
        }
    }


    public void HandleLocationExit(GameObjectLocation gol)
    {
        // Menampilkan nama lokasi atau melakukan logika tambahan lainnya
        Debug.Log($"Player keluar lokasi: {gol.Location.name}, yang berjenis {gol.lokasiSaatIni}");

        // Bisa tambahkan logika lain, misalnya memunculkan UI nama lokasi, memberi dialog, dll.
        switch (gol.lokasiSaatIni)
        {
            case Lokasi.Danau:
                // Misalnya, tampilkan dialog atau trigger event di sini
                //dialogueSystem.theDialogues = questManager.currentMainQuest.dialogueQuest[6];
                //dialogueSystem.StartDialogue();
                if (mainQuestDanau)
                {
                    inDanau = false;
                    //questManager.currentMainQuest.currentQuestState = MainQuest1State.Pulang;
                    //questManager.NextQuestState();
                    mainQuestDanau = false;
                    exitDanauScene = true;
                }
                inDanau = false;
                break;

            case Lokasi.RumahJaka:
                // Lakukan sesuatu di Rumah Jaka
                if (mainQuestMisiYangTerlupakan)
                {
                    //questManager.currentMainQuest.currentQuestState = MainQuest1State.MisiYangBelumSelesai;
                    //questManager.NextQuestState();
                    mainQuestMisiYangTerlupakan = false;
                }

                break;

            case Lokasi.HutanAjaib:
                // Bisa buat logika khusus untuk Hutan Ajaib
                break;

                // Tambahkan case lain sesuai lokasi yang ada
        }
    }


    private IEnumerator AreaHealingCoroutine()
    {
        while (inDanau)
        {
            if (PlayerController.Instance.health < PlayerController.Instance.playerData.maxHealth) // Cek biar nggak over-heal
            {
                PlayerController.Instance.health += 3;
                Debug.Log("Healing... HP sekarang: " + PlayerController.Instance.health);
            }

            yield return new WaitForSeconds(delayHealing); // Tunggu sebelum heal lagi
        }


    }
}
