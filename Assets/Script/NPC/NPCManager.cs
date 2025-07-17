using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static QuestManager;


public enum JobType
{
    Petani,
    PenjagaKuburan,
    Pedagang,
    Nelayan,
    Pemburu,
    Penjahit
}


public class NPCManager : MonoBehaviour
{

    [System.Serializable]
    public class NPCData
    {
        public string npcName;
        public string fullName;
        public string pekerjaan;
        public string hobi;
        public int tanggalUltah;
        public int bulanUltah;
        public Sprite npcSprite;
        public GameObject prefab;                  // Prefab NPC
        public int totalFrendships;                // menghitung jumlah nilai pertemanan
        public int countGift;
        public Schedule[] schedules;              // Jadwal aktivitas NPC
        public Frendship frendship;              // Hubungan persahabatan NPC
    }

    [System.Serializable]
    public class Schedule
    {
        public string activityName;
        public Vector3[] waypoints;
        public float startTime;
        public bool hasStarted = false; // Tambahkan penanda
        public bool isOngoing = false; // tanda apakah schedule sudah selesai
    }

    [System.Serializable]
    public class Frendship
    {
        public int favoriteValue;
        public int likesValue;
        public int normalValue;
        public int hateValue;
        public Item[] favorites;
        public Item[] like;
        public Item[] normal;
        public Item[] hate;
    }

    public NPCData[] npcDataArray; // Array untuk menyimpan semua data NPC
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private DialogueSystem dialogueSystem;
    [SerializeField] private EnvironmentManager kuburanEnvironment;
    [SerializeField] GameEconomy gameEconomy;

    public Dialogues dialogueTerimakasih;

    public GameObject wargaDesaParent; // Objek kosong untuk menampung NPC


    private void Update()
    {
        CheckNPCActivities();
    }

    private void CheckNPCActivities()
    {
        foreach (var npcData in npcDataArray)
        {
            GameObject npcInstance = npcData.prefab.gameObject;
            NPCBehavior npcBehavior = npcInstance.GetComponent<NPCBehavior>();

            if (npcBehavior == null)
                continue;

            foreach (var schedule in npcData.schedules)
            {
                // Cek apakah waktu cocok dan aktivitas belum dimulai
                if (Mathf.Approximately(timeManager.hour, schedule.startTime) && !schedule.hasStarted)
                {
                    npcBehavior.StartActivity(schedule); // Mulai aktivitas
                    schedule.isOngoing = true;          // Tandai aktivitas sedang berlangsung
                    schedule.hasStarted = true;         // Tandai aktivitas sudah dimulai
                    break; // Keluar dari loop karena hanya satu aktivitas yang boleh dimulai
                }

                // Jika aktivitas sedang berlangsung, biarkan NPC menyelesaikan aktivitasnya
                if (schedule.isOngoing)
                {
                    // Jika NPC tidak lagi bergerak, tandai aktivitas selesai
                    if (!npcBehavior.IsMoving())
                    {
                        schedule.isOngoing = false; // Tandai aktivitas selesai
                    }
                }
            }
        }
    }






    public void CheckNPCQuest()
    {
        Debug.Log("check npc quest di jalankan");
       //foreach (var chapter in questManager.chapters)
       // {
       //     foreach(var quest in chapter.sideQuest)
       //     {
       //         if (quest.questActive)
       //         {
       //             string nameNpc = quest.NPC.name;
       //             Debug.Log("nama npc dari struck quest adalah" + nameNpc);
       //             foreach (var NPC in npcDataArray)
       //             {
       //                 if (NPC.prefab.name == nameNpc)
       //                 {
       //                     QuestInteractable interactable = NPC.prefab.GetComponent<QuestInteractable>();
       //                     if (interactable != null)
       //                     {
       //                         interactable.currentDialogue = quest.dialogueQuest;
       //                         interactable.promptMessage = "Quest " + quest.questName;
       //                         interactable.isQuestNPC = true;
       //                     }
       //                 }
       //                 else
       //                 {
       //                     Debug.Log("nama npc berbeda");
       //                 }
       //             }
       //         }
       //     }
       // }

        //if (questManager.CurrentActiveQuest != null )
        //{
        //    // maka otomatis buatkan variabel baru bernama "mq1" dengan tipe tersebut.
        //    if (questManager.CurrentActiveQuest is MainQuest1_Controller mq1)
        //    {
        //        // Tidak perlu casting manual, kita bisa langsung pakai mq1

               
                
        //        string nameNpc = mq1.targetNpcName;
        //        foreach (var npc in npcDataArray)
        //        {
        //            if (npc.prefab != null && npc.prefab.name == nameNpc)
        //            {
        //                QuestInteractable interactable = npc.prefab.GetComponent<QuestInteractable>();
        //                if (interactable != null)
        //                {
        //                    interactable.currentDialogue = mq1.dialoguePengingat;
        //                    //interactable.promptMessage = "Quest " + questManager.CurrentActiveQuest.questName;
        //                }
        //            }
        //        }
        //        Debug.Log($"NPC untuk Main Quest 1 adalah: {nameNpc}");
        //    }
            
        //}




    }

    // Mari kita sebut PindahkanNPCToQuestLocation untuk kejelasan.
    public void PindahkanNPCToQuestLocation(string targetNpcName, Vector3 locationQuest)
    {
        Debug.Log("fungsik memindahkan npc di panggil");
        // Cek apakah parent untuk warga desa sudah di-set
        if (wargaDesaParent == null)
        {
            Debug.LogError("Variabel 'wargaDesaParent' belum di-set di Inspector NPCManager!");
            return;
        }

        bool npcDitemukan = false;

        // Loop melalui setiap anak dari objek wargaDesaParent
        foreach (Transform npcTransform in wargaDesaParent.transform)
        {
            // Ambil skrip behavior dari setiap NPC
            NPCBehavior npcBehavior = npcTransform.GetComponent<NPCBehavior>();

            // Jika skrip ada dan namanya cocok
            if (npcBehavior != null && npcBehavior.npcName == targetNpcName)
            {
                npcDitemukan = true; // Tandai NPC ditemukan

                //Aktifkan GameObject NPC jika sebelumnya tidak aktif
                npcTransform.gameObject.SetActive(true);

                //Nonaktifkan sementara jadwal normalnya agar ia tidak mencoba berjalan pergi
                npcBehavior.isMoving = false; // Atau buat variabel baru: npcBehavior.isLockedForQuest = true;
                npcBehavior.currentPosition = npcTransform.position;
                //Pindahkan NPC ke lokasi quest yang ditentukan
                npcTransform.position = locationQuest;

                

                // Ambil skrip interaksinya untuk disiapkan jika perlu
                QuestInteractable questScript = npcTransform.GetComponent<QuestInteractable>();
                if (questScript != null)
                {
                    questScript.isQuestNPC = true;
                    // Anda bisa set dialog spesifik di sini jika mau,
                    // tapi lebih baik diatur oleh MainQuestController-nya langsung.
                }

                Debug.Log($"SUKSES: NPC '{targetNpcName}' telah dipindahkan ke lokasi quest.");

                // Hentikan loop karena NPC sudah ditemukan
                break;
            }
        }

        // Jika setelah loop selesai dan NPC tidak ditemukan, beri peringatan
        if (!npcDitemukan)
        {
            Debug.LogWarning($"PERINGATAN: Tidak ada anak dari 'wargaDesaParent' yang memiliki NPCBehavior dengan npcName '{targetNpcName}'!");
        }
    }

    public void KembalikanNPKeJadwalNormal(string targetNpcName)
    {
        // Loop melalui setiap anak dari objek wargaDesaParent
        foreach (Transform npcTransform in wargaDesaParent.transform)
        {
            NPCBehavior npcBehavior = npcTransform.GetComponent<NPCBehavior>();

            if (npcBehavior != null && npcBehavior.npcName == targetNpcName)
            {
                // Reset statusnya agar ia bisa kembali mengikuti jadwal harian
                npcTransform.position = npcBehavior.currentPosition;
                npcBehavior.isMoving = true; // Atau isLockedForQuest = false;
                // Beritahu QuestInteractable bahwa ia bukan lagi NPC quest utama untuk saat ini
                QuestInteractable questScript = npcTransform.GetComponent<QuestInteractable>();
                if (questScript != null)
                {
                    questScript.isQuestNPC = false;
                }

                Debug.Log($"NPC {targetNpcName} telah dilepaskan dan akan kembali ke jadwalnya.");
                break;
            }
        }
    }



    public bool GiveReward(JobType jobType)
    {
        int randomReward = 0; // Default nilai reward

        switch (jobType)
        {
            case JobType.PenjagaKuburan:
                int jumlahKuburanKotor = kuburanEnvironment.countKuburanKotor;
                int jumlahKuburanDibersihkan = kuburanEnvironment.jumlahdiBersihkan;

                // Pastikan ada kuburan yang kotor dan sudah dibersihkan
                if (jumlahKuburanKotor != 0 && jumlahKuburanDibersihkan != jumlahKuburanKotor)
                {
                    // Menentukan hadiah berdasarkan jumlah kuburan yang dibersihkan
                    if (jumlahKuburanDibersihkan == 1)
                    {
                        // Hadiah untuk 1 kuburan dibersihkan
                        Debug.Log("Hadiah Kategori 1: Pemberian kecil.");
                        randomReward = 20;// Hadiah kecil
                    }
                    else if (jumlahKuburanDibersihkan >= 2 && jumlahKuburanDibersihkan < 5)
                    {
                        // Hadiah untuk lebih dari 1 hingga kurang dari 10 kuburan dibersihkan
                        Debug.Log("Hadiah Kategori 2: Pemberian sedang.");
                        randomReward = UnityEngine.Random.Range(30, 80); // Hadiah menengah
                    }
                    else if (jumlahKuburanDibersihkan >= 5 && jumlahKuburanDibersihkan < 10)
                    {
                        // Hadiah untuk lebih dari 10 hingga kurang dari 20 kuburan dibersihkan
                        Debug.Log("Hadiah Kategori 3: Pemberian besar.");
                        randomReward = UnityEngine.Random.Range(100, 200); // Hadiah besar
                    }
                    else if (jumlahKuburanDibersihkan >= 10 && jumlahKuburanDibersihkan <20)
                    {
                        // Hadiah untuk 20 atau lebih kuburan dibersihkan
                        Debug.Log("Hadiah Kategori 4: Pemberian ekstra besar!");
                        randomReward = UnityEngine.Random.Range(200, 400); // Hadiah ekstra besar
                    }

                    // Kondisi khusus untuk 100% kuburan yang dibersihkan
                    if (jumlahKuburanDibersihkan == jumlahKuburanKotor)
                    {
                        Debug.Log("Semua kuburan dibersihkan! Pemberian hadiah penuh.");
                        randomReward = UnityEngine.Random.Range(400, 600); // Hadiah penuh

                        // Menambahkan uang ke ekonomi
                        gameEconomy.money += randomReward;

                        // Menampilkan dialog terima kasih
                        dialogueSystem.theDialogues = dialogueTerimakasih;
                        dialogueSystem.StartDialogue();

                        return true; // Hadiah berhasil diberikan
                    }

                    // Menambahkan uang ke ekonomi jika belum 100%
                    gameEconomy.money += randomReward;

                    // Menampilkan dialog terima kasih
                    dialogueSystem.theDialogues = dialogueTerimakasih;
                    dialogueSystem.StartDialogue();

                    return true; // Hadiah berhasil diberikan
                }
                break;
        }

        return false; // Tidak ada hadiah jika tidak memenuhi kondisi
    }






}
