using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

// Perhatikan, kelas ini mewarisi semua yang ada di MainQuestController
public class MainQuest1_Controller : MainQuestController
{
    // Enum milik maiknqueststate 1 
    public enum MainQuest1State
    {
        None,
        Play,
        MenemukanDanau,
        PergiKeLokasiQuest,
        CariRusa,
        BunuhRusa,
        MunculBandit,
        Sekarat,
        SceneDanauIndah,
        Pulang,
        KabarKesedihan,
        MisiYangBelumSelesai,
        PermintaanMamat,
        Selesai,
    }

    [Header("Detail Spesifik untuk Main Quest 1")]
    public MainQuest1State currentState = MainQuest1State.None;

    // Semua data lain yang Anda butuhkan untuk quest ini ada di sini.
    public Dialogues[] dialogueQuest;
    public Dialogues dialoguePengingat;
    public Sprite[] spriteQuest;
    public GameObject NPC;
    public int indexLocation;
    public locationMainQuest[] arrayLocationMainQuest;

    // Class spesifik untuk quest ini bisa didefinisikan di sini jika perlu.
    [System.Serializable]
    public class locationMainQuest
    {
        public string infoQuest;
        public GameObject locationQuest;
        public bool isSpawner;
        public GameObject spawner;
    }



    // Ini adalah implementasi dari "kontrak" abstract.
    public override void UpdateQuest()
    {

        switch (currentState)
        {
            case MainQuest1State.None:


                currentState = MainQuest1State.Play;
                UpdateQuest();
                break;
            case MainQuest1State.Play:
                Debug.Log($"Memulai currentState Quest: {currentState}");
                ShowDialogueAndSprite(0, 0, true);

                GameObject npcMainQuest = NPC;
                Vector3 locationNpcMainQuest = locateNpcQuest;
                Dialogues dialoguesMainQuest = dialogueQuest[1];

                questManager.npcManager.CheckNPCMainQuest(npcMainQuest, locationNpcMainQuest, dialoguesMainQuest);
                currentState = MainQuest1State.PergiKeLokasiQuest;
                


                break;
            case MainQuest1State.PergiKeLokasiQuest:
                questManager.mainQuestInfo = arrayLocationMainQuest[indexLocation].infoQuest;
                currentState = MainQuest1State.PergiKeLokasiQuest;
                UpdateLocationMainQuest();
                break;
            case MainQuest1State.CariRusa:
                //playerQuest.CariRusa();
                break;
            case MainQuest1State.BunuhRusa:
                //MunculkanSpawnerBandit();
                break;
            case MainQuest1State.Sekarat:
                //indexLocation++;
                //UpdateLocationMainQuest();
                break;
            case MainQuest1State.SceneDanauIndah:
                //ShowDialogueAndSprite(5, 1, true);
                //HapusSparnerBandit();
                break;
            case MainQuest1State.Pulang:
                //indexLocation++;
                //UpdateLocationMainQuest();
                break;
            case MainQuest1State.KabarKesedihan:
                //ShowDialogueAndSprite(8, 2, true);
                //locationManager.mainQuestMisiYangTerlupakan = true;
                break;
            case MainQuest1State.MisiYangBelumSelesai:
                //indexLocation++;
                //UpdateLocationMainQuest();
                break;
            case MainQuest1State.PermintaanMamat:
                //mainQuestInfo = "Cari daging rusa untuk Mamat";
                //mainQuestInfo = locationMainQuest[indexLocation].infoQuest;
                //childTemplateContentGo = childContentGo.GetComponentInChildren<TextMeshProUGUI>();
                //childContentGo.name = mainQuestInfo;
                //childTemplateContentGo.text = mainQuestInfo;questManager.questManager.questUI

                //chapter1IsDone = true;
                break;

        }

    }

    public void ShowDialogueAndSprite(int indexDialogue, int indexImage, bool pakaiImage)
    {
        Debug.Log("Mulai Scene Cerita");
        if (pakaiImage)
        {
            questManager.questUI.gameObject.SetActive(true);
            //tentukan image yang ingin di tampilkan
            Image questImageUI = questManager.questUI.GetChild(0).GetComponent<Image>();
            questImageUI.sprite = spriteQuest[indexImage];

            // Pastikan index tidak melebihi batas array
            questManager.dialogueSystem.theDialogues = dialogueQuest[indexDialogue];
            questManager.dialogueSystem.StartDialogue();
            StartCoroutine(questManager.dialogueSystem.WaitForDialogueToEnd());
        }
        else
        {
            // Pastikan index tidak melebihi batas array
            questManager.dialogueSystem.theDialogues = dialogueQuest[indexDialogue];
            questManager.dialogueSystem.StartDialogue();
            StartCoroutine(questManager.dialogueSystem.WaitForDialogueToEnd());
        }



    }

    public void UpdateLocationMainQuest()
    {
        switch (indexLocation)
        {
            case 0:
                questManager.childContentGo = questManager.ContentGO.transform.Find(questName);
                questManager.childTemplateContentGo = questManager.childContentGo.GetComponentInChildren<TextMeshProUGUI>();
                questManager.childContentGo.name = questManager.mainQuestInfo;
                questManager.childTemplateContentGo.text = questManager.mainQuestInfo;
                questManager.playerQuest.locationMainQuest = arrayLocationMainQuest[indexLocation].locationQuest;
                questManager.playerQuest.dialogueInLocation = dialogueQuest[2];
                break;
            //case 1:
            //    Debug.Log("memanggil fungsi sekarat");
            //    childContentGo = ContentGO.transform.Find(mainQuestInfo);

            //    mainQuestInfo = locationMainQuest[indexLocation].infoQuest;
            //    childTemplateContentGo = childContentGo.GetComponentInChildren<TextMeshProUGUI>();
            //    childContentGo.name = mainQuestInfo;
            //    childTemplateContentGo.text = mainQuestInfo;

            //    dialogueSystem.theDialogues = dialogueQuest[3];
            //    dialogueSystem.StartDialogue();

            //    playerQuest.locationMainQuest = locationMainQuest[indexLocation].locationQuest;
            //    playerQuest.dialogueInLocation = dialogueQuest[4];
            //    playerQuest.mainQuestInLocation = false;
            //    break;
            //case 2:
            //    Debug.Log("memanggil fungsi pulang");
            //    childContentGo = ContentGO.transform.Find(mainQuestInfo);

            //    mainQuestInfo = locationMainQuest[indexLocation].infoQuest;
            //    childTemplateContentGo = childContentGo.GetComponentInChildren<TextMeshProUGUI>();
            //    childContentGo.name = mainQuestInfo;
            //    childTemplateContentGo.text = mainQuestInfo;

            //    dialogueSystem.theDialogues = dialogueQuest[7];
            //    dialogueSystem.StartDialogue();

            //    playerQuest.locationMainQuest = locationMainQuest[indexLocation].locationQuest;
            //    playerQuest.dialogueInLocation = dialogueQuest[8];
            //    playerQuest.mainQuestInLocation = false;
            //    break;
            //case 3:
            //    Debug.Log("memanggil fungsi lupa sesuatu");
            //    childContentGo = ContentGO.transform.Find(mainQuestInfo);
            //    mainQuestInfo = locationMainQuest[indexLocation].infoQuest;
            //    childTemplateContentGo = childContentGo.GetComponentInChildren<TextMeshProUGUI>();
            //    childContentGo.name = mainQuestInfo;
            //    childTemplateContentGo.text = mainQuestInfo;

            //    playerQuest.locationMainQuest = locationMainQuest[indexLocation].locationQuest;
            //    playerQuest.dialogueInLocation = dialogueQuest[9];
            //    playerQuest.mainQuestInLocation = false;
            //    Debug.Log("main quest in location di set ke false");
            //    Debug.Log("index location : " + indexLocation);
            //    Debug.Log(playerQuest.mainQuestInLocation);
            //    break;

        }
    }
}