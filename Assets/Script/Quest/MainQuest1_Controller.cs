using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MainQuest1_Controller : MainQuestController
{
    // Enum state HANYA untuk quest ini.
    public enum MainQuest1State
    {
        BelumMulai,
        AdeganMimpi,
        PergiKeHutan,
        CariRusa,
        MunculkanHarimau,
        // ... state lainnya
        Selesai
    }

    [Header("Detail Spesifik untuk Main Quest 1")]
    public MainQuest1State currentState = MainQuest1State.BelumMulai;
    public Dialogues[] dialogueQuest;
    public Dialogues dialoguePengingat;
    public Sprite[] spriteQuest;
    public string targetNpcName;
    public Vector3 locateNpcQuest;
    public locationMainQuest[] arrayLocationMainQuest;
    private bool isDialoguePlaying;

    private string lokasiYangDitunggu;

    // Class data lokasi spesifik untuk quest ini.
    [System.Serializable]
    public class locationMainQuest
    {
        public string infoQuest;
        public GameObject locationQuest;
        public GameObject[] animalObject;
        public Vector2 playerLocation;
        public Vector2 otherLocation;
    }

    // Daftar untuk melacak semua rusa yang aktif untuk quest ini
    private List<GameObject> rusaQuestAktif = new List<GameObject>();

    [Header("Data Object")]
    public GameObject harimauPrefab; // Slot untuk prefab harimau
    public Transform lokasiMunculHarimau; // Transform untuk posisi munculnya harimau
    public GameObject playerObject;
    
    // Manajemen Event
    private void OnEnable()
    {
        // Mulai mendengarkan event saat quest ini aktif.
        PlayerQuest.OnPlayerEnteredLocation += HandleLocationEnter;

        // Mulai mendengarkan sinyal kematian dari AnimalBehavior
        AnimalBehavior.OnAnimalDied += HandleRusaDikalahkan;
    }
    private void OnDisable()
    {
        // Berhenti mendengarkan saat quest ini selesai/hancur.
        PlayerQuest.OnPlayerEnteredLocation -= HandleLocationEnter;

        // Berhenti mendengarkan agar tidak ada error saat scene berganti
        AnimalBehavior.OnAnimalDied -= HandleRusaDikalahkan;
    }

    // Logika Inti Quest

    public override void StartQuest(QuestManager manager)
    {
        base.StartQuest(manager);
        // Saat quest dimulai, langsung jalankan state pertama.
        ChangeState(MainQuest1State.AdeganMimpi);
    }

    // Fungsi ini wajib ada, tapi kita biarkan kosong karena kita pakai sistem event.
    public override void UpdateQuest() { }

    // Fungsi pusat untuk mengubah state dan menjalankan aksi awal.
    private void ChangeState(MainQuest1State newState)
    {
        currentState = newState;
        Debug.Log("State Main Quest 1 berubah menjadi: " + currentState);

        switch (currentState)
        {
            case MainQuest1State.AdeganMimpi:
                // Jalankan scene/dialog pembuka SATU KALI.
                ShowDialogueAndSprite(0, 0, true);


                Vector3 locationNpcMainQuest = locateNpcQuest;

                //questManager.npcManager.PindahkanNPCToQuestLocation(targetNpcName, locationNpcMainQuest);
                break;

            case MainQuest1State.PergiKeHutan:
                // Beri tugas baru ke pemain.
                //questManager.mainQuestInfo = "Pergilah ke Hutan Ajaib";
                //questManager.UpdateDisplayQuest(questName);
                //questManager.npcManager.KembalikanNPKeJadwalNormal(targetNpcName);
                // Set lokasi yang kita tunggu.
                lokasiYangDitunggu = arrayLocationMainQuest[0].locationQuest.name;
                break;

            case MainQuest1State.CariRusa:
                //questManager.mainQuestInfo = "Cari jejak Rusa di sekitar hutan";
                //questManager.UpdateDisplayQuest("Pergilah ke Hutan Ajaib");
                ShowDialogueAndSprite(2, 0, false);
                // Logika untuk spawn rusa atau jejaknya bisa dimulai di sini.
                rusaQuestAktif.Clear();
                for (int i = 0; i < arrayLocationMainQuest[0].animalObject.Length; i++)
                {
                    // Spawn hewan di posisi yang valid
                    GameObject rusaBaru = Instantiate(arrayLocationMainQuest[0].animalObject[i]);
                    Transform locationSpawn = arrayLocationMainQuest[0].locationQuest.GetComponent<Transform>();
                    rusaBaru.transform.position = locationSpawn.position;
                    rusaBaru.transform.localScale = Vector3.one;  // Pastikan skala 1,1,1
                    rusaBaru.transform.parent = locationSpawn;        // Atur parent setelahnya

                    // Atur skala prefab ke 1 untuk memastikan ukurannya sesuai dengan prefab aslinya
                    rusaBaru.transform.localScale = Vector3.one;
                    rusaQuestAktif.Add(rusaBaru);
                }
                break;
             case MainQuest1State.MunculkanHarimau:
                MunculkanHarimau();
                break;

            case MainQuest1State.Selesai:
                isQuestComplete = true; // Tandai quest selesai untuk QuestManager.
                break;
        }
    }

    // Fungsi Pemicu (Dipanggil oleh Sistem Lain)

    // Contoh: Fungsi ini bisa dipanggil oleh DialogueSystem setelah dialog selesai.
    public void OnDialogueFinished()
    {
        if (currentState == MainQuest1State.AdeganMimpi)
        {
            ChangeState(MainQuest1State.PergiKeHutan);
        }
    }

    // Fungsi ini berjalan OTOMATIS saat pemain masuk ke sebuah lokasi.
    private void HandleLocationEnter(string locationName)
    {
        // Cek apakah lokasi yang dimasuki pemain adalah yang kita tunggu.
        if (currentState == MainQuest1State.PergiKeHutan && locationName == lokasiYangDitunggu)
        {
            Debug.Log("sudah sampai hutan ajaib");
            
           
            
            // Lanjutkan quest!
            ChangeState(MainQuest1State.CariRusa);
            lokasiYangDitunggu = null; // Reset agar tidak terpicu lagi.
        }
    }

    // Fungsi Bantuan
    public void ShowDialogueAndSprite(int indexDialogue, int indexImage, bool pakaiImage)
    {
        Debug.Log("Mulai Scene Cerita");
        if (pakaiImage)
        {
            //questManager.questUI.gameObject.SetActive(true);
            ////tentukan image yang ingin di tampilkan
            //Image questImageUI = questManager.questUI.GetChild(0).GetComponent<Image>();
            //questImageUI.sprite = spriteQuest[indexImage];

            // Pastikan index tidak melebihi batas array
            //questManager.dialogueSystem.theDialogues = dialogueQuest[indexDialogue];
            //questManager.dialogueSystem.StartDialogue();
            //StartCoroutine(questManager.dialogueSystem.WaitForDialogueToEnd());
        }
        else
        {
            // Pastikan index tidak melebihi batas array
            //questManager.dialogueSystem.theDialogues = dialogueQuest[indexDialogue];
            //questManager.dialogueSystem.StartDialogue();
            //StartCoroutine(questManager.dialogueSystem.WaitForDialogueToEnd());
        }



    }

    public void OnNPCInteracted(string name)
    {
        Debug.Log($"NPC Interacted: {name}. Expecting: {this.targetNpcName}. Current State: {currentState}");


        // Jika dialog lain sedang berjalan, jangan lakukan apa-apa
        if (isDialoguePlaying) return;

       

        if (name == this.targetNpcName)
        {
            // Jika namanya cocok, baru kita lanjutkan ke logika state
            switch (currentState)
            {
                case MainQuest1State.AdeganMimpi:
                    Debug.Log($"Kondisi terpenuhi! Memulai dialog dengan NPC: {targetNpcName}");
                    //StartCoroutine(PlayDialogueSequence(dialogueQuest[1], MainQuest1State.PergiKeHutan));
                    
                    break;

                    // ... case lainnya ...
            }
        }
        else
        {
            // Jika nama NPC tidak cocok dengan yang ditunggu quest,
            // Anda bisa memilih untuk tidak melakukan apa-apa atau memainkan dialog basa-basi.
            Debug.Log("Kontol namanya beda");
        }
    }

    // Ini adalah fungsi bantuan yang sangat penting!
    // Ia akan memainkan dialog, MENUNGGU sampai selesai, lalu mengubah state.
    //private IEnumerator PlayDialogueSequence(Dialogues dialogueToPlay, MainQuest1State nextState)
    //{
    //    // Beri tahu sistem bahwa dialog sedang berjalan
    //    isDialoguePlaying = true;

    //    // Set dialog yang benar di DialogueSystem
    //    //questManager.dialogueSystem.theDialogues = dialogueToPlay;

    //    //Mulai dialognya
    //    //questManager.dialogueSystem.StartDialogue();

    //    //Mulai coroutine di DialogueSystem dan TUNGGU di sini sampai coroutine itu selesai.
    //    //yield return StartCoroutine(questManager.dialogueSystem.WaitForDialogueToEnd());

    //    // 5. Kode di bawah ini HANYA akan berjalan SETELAH WaitForDialogueToEnd() selesai.
    //    Debug.Log("Selesai menunggu dialog. Melanjutkan ke state berikutnya.");
    //    ChangeState(nextState);

    //    GameController.Instance.PindahKeScene("Village");
    //    // Set kembali status dialog
    //    isDialoguePlaying = false;
    //}

    //logika mendeskripsikan kematian rusa dan memunculkan harimau 
    // Di dalam MainQuest1_Controller.cs
    private void HandleRusaDikalahkan(AnimalBehavior rusaYangMati)
    {
        // Cek apakah rusa yang mati itu ada di dalam daftar pelacakan kita
        if (rusaQuestAktif.Contains(rusaYangMati.gameObject))
        {
            Debug.Log($"Seekor rusa quest ({rusaYangMati.name}) telah dikalahkan.");

            // Hapus dari daftar
            rusaQuestAktif.Remove(rusaYangMati.gameObject);

            // Cek apakah daftar sudah kosong
            if (rusaQuestAktif.Count == 0)
            {
                Debug.Log("Semua rusa telah dikalahkan! Saatnya harimau muncul!");

                ChangeState(MainQuest1State.MunculkanHarimau); // Anda bisa ganti namanya jadi MunculHarimau
            }
            else
            {
                Debug.Log($"Sisa rusa: {rusaQuestAktif.Count}");
            }
        }
    }

    // Fungsi yang akan dipanggil saat semua rusa sudah kalah
    private void MunculkanHarimau()
    {
        // Ganti state quest jika perlu

        GameController.Instance.PindahKeScene("Village");
        Transform playerObjectTransform = playerObject.GetComponent<Transform>();
        playerObjectTransform.position = arrayLocationMainQuest[0].playerLocation;
        if (harimauPrefab != null && lokasiMunculHarimau != null)
        {
            // Munculkan harimau di lokasi yang ditentukan
            Instantiate(harimauPrefab, arrayLocationMainQuest[0].otherLocation, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Prefab Harimau atau Lokasi Munculnya belum di-set di Inspector!");
        }
    }
}