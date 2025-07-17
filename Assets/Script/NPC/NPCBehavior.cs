using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Tambahkan ini
using Unity.VisualScripting;
using static QuestManager;
using static UnityEditor.Progress;
using NUnit.Framework.Interfaces;



public class NPCBehavior : MonoBehaviour
{
    [Header("Daftar Hubungan")]
    //[SerializeField] QuestManager questManager;
    [SerializeField] protected DialogueSystem dialogueSystem;
    [SerializeField] NPCManager npcManager;
    [SerializeField] GameEconomy gameEconomy;
    [SerializeField] QuestInfoUI questInfoUI;
    [SerializeField] EnvironmentManager kuburanInteractable;
    private NPCManager.Schedule currentActivity; // Gunakan NPCManager.Schedule untuk mendeklarasikan tipe

    [SerializeField] NPCAnimation npcAnimation;



    public string npcName;
    public JobType pekerjaanNPC;
    public Vector3 startPosition;
    public Vector2 currentPosition;
    public float movementSpeed = 2.0f;


    public bool isMoving = false;
    public int currentWaypointIndex = 0; //index saat ini
    private Renderer npcRenderer;
    private bool hasStartedActivity = false;

    public string itemQuest;
    public int jumlahItem;

    //Animasi objek lain contoh pakaian 
    [System.Serializable]
    public class Animation
    {
        public string name;
        public Sprite[] sprites;
        public SpriteRenderer spriteRenderers;
    }

    public Animation[] animations;
    public float frameRate = 0.1f; // Waktu per frame (kecepatan animasi)
    private int currentFrame = 0; // Indeks frame saat ini



    private void Start()
    {
        npcRenderer = GetComponent<Renderer>();
        if (npcRenderer == null)
        {
            //Debug.LogError("Renderer tidak ditemukan pada NPC!");
        }
        
        npcName = gameObject.name;
    }


    private void Update()
    {
        if (isMoving)
        {
            MoveToNextWaypoint();
        }
    }

    public void StartActivity(NPCManager.Schedule schedule)
    {
        if (schedule == null)
        {
            return;
        }

        currentActivity = schedule;

        if (currentActivity.waypoints.Length > 0)
        {
            currentWaypointIndex = 0; // Reset indeks waypoint
            isMoving = true;          // Mulai pergerakan
        }
        else
        {
            Debug.Log("Tidak ada aktivitas.");
        }

        StartCoroutine(playAnimationClothes());
    }

    




    private void MoveToNextWaypoint()
    {
        if (currentWaypointIndex < currentActivity.waypoints.Length)
        {
            // Ambil waypoint target berdasarkan indeks
            Vector3 targetPosition = currentActivity.waypoints[currentWaypointIndex];

            //ambil posisi awal target sebelum bergerak
            if (currentWaypointIndex == 1 )
            {
                startPosition = transform.position;
            }else if (currentWaypointIndex > 1)
            {
                startPosition = currentActivity.waypoints[currentWaypointIndex - 1];
            }




            // Gerakkan NPC ke target
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

            // Jika mencapai waypoint target, lanjut ke waypoint berikutnya
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                currentWaypointIndex++;
            }
        }
        else
        {
            // Selesai mencapai semua waypoint
            isMoving = false; // Hentikan pergerakan
            currentActivity.isOngoing = false; // Tandai aktivitas selesai

            // Set animasi idle saat semua waypoint selesai
            //npcAnimation.SetWalkAnimation(false, false, false, false);
        }
    }


    public bool IsMoving()
    {
        return isMoving;
    }



    private void OnDrawGizmos()
    {
        if (npcManager.npcDataArray != null)
        {
            foreach (var npc in npcManager.npcDataArray)
            {
                if (npc.schedules != null)
                {
                    foreach (var schedule in npc.schedules)
                    {
                        Gizmos.color = Color.blue;

                        // Gambar titik waypoints dalam posisi global
                        for (int i = 0; i < schedule.waypoints.Length; i++)
                        {
                            Gizmos.DrawSphere(schedule.waypoints[i], 0.2f); // Waypoint sudah dalam posisi global
                        }

                        // Gambar garis antar-waypoints
                        for (int i = 0; i < schedule.waypoints.Length - 1; i++)
                        {
                            Gizmos.DrawLine(schedule.waypoints[i], schedule.waypoints[i + 1]);
                        }
                    }
                }
            }
        }
    }




    private IEnumerator playAnimationClothes()
    {
        while (true) // Loop tanpa batas (animasi berulang)
        {
            foreach (var sprite in animations)
            {
                if (sprite.sprites.Length > 0)
                {
                    sprite.spriteRenderers.sprite = sprite.sprites[currentFrame]; // Setel sprite saat ini
                    currentFrame = (currentFrame + 1) % sprite.sprites.Length; // Pindah ke frame berikutnya (loop)
                }
                yield return new WaitForSeconds(frameRate); // Tunggu sebelum beralih ke frame berikutnya
            }
        }
    }



    private IEnumerator FadeOutAndDestroy()
    {
        // Debug.Log("menjalankan fungsi menghapus npc");
        float fadeDuration = 2f;
        float elapsedTime = 0f;
        Color initialColor = npcRenderer.material.color;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(initialColor.a, 0f, elapsedTime / fadeDuration);
            npcRenderer.material.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }

    public void StartFirstActivity()
    {
        // Pastikan NPC diaktifkan terlebih dahulu
        gameObject.SetActive(true);
        // Debug.Log("npc di aktifkan");
        // Periksa apakah NPC aktif sebelum memulai fade-in
        if (npcRenderer != null)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            Debug.LogError("Renderer NPC tidak ditemukan!");
        }
    }


    private IEnumerator FadeIn()
    {
        // Debug.Log("Menjalankan fade-in untuk NPC");

        float fadeDuration = 2f;
        float elapsedTime = 0f;
        Color initialColor = npcRenderer.material.color;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(initialColor.a, 1f, elapsedTime / fadeDuration);
            npcRenderer.material.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Debug.Log("NPC muncul kembali setelah fade-in");
    }

    //public void ReceiveItem(Item item)
    //{
    //    Debug.Log("NPC menerima item: " + item.name);
    //    // Tambahkan logika untuk menerima item
    //}

    public bool CheckItemGive(ItemData inventoryItemData)
    {
        Debug.Log($"Memeriksa apakah NPC {this.npcName} membutuhkan item {inventoryItemData.itemName}");
        PlayerQuestStatus activeQuestStatus = QuestManager.Instance.GetActiveQuestForNPC(this.npcName);

        if (activeQuestStatus == null)
        {
             Debug.Log($"Tidak ada quest aktif untuk NPC {this.npcName}");
            return false;
            //Debug.Log($"seharusnya nama npc adalah {activeQuestStatus.Quest.npcName}");
        }

        //Cari apakah item yang diberikan pemain dibutuhkan oleh quest aktif tersebut.
        ItemData requiredItem = activeQuestStatus.Quest.itemRequirements
            .FirstOrDefault(req => req.itemName.Equals(inventoryItemData.itemName, System.StringComparison.OrdinalIgnoreCase));

        // Jika item tidak dibutuhkan oleh quest ini, hentikan fungsi.
        if (requiredItem == null)
        {
            Debug.Log($"Item {inventoryItemData.itemName} tidak dibutuhkan untuk quest '{activeQuestStatus.Quest.questName}'.");
            return false;
        }

        //Hitung progres dan jumlah yang dibutuhkan
        int currentProgress = activeQuestStatus.itemProgress[inventoryItemData.itemName];
        int needed = requiredItem.count - currentProgress;

        if (needed <= 0)
        {
            Debug.Log($"Kebutuhan untuk item {inventoryItemData.itemName} sudah terpenuhi.");
            return false; // Kebutuhan item ini sudah selesai
        }

        //Tentukan berapa banyak item yang akan diberikan
        int amountToGive = Mathf.Min(inventoryItemData.count, needed);

        //Lakukan proses serah terima
        if (amountToGive > 0)
        {
            // Tambahkan progres di data status quest
            activeQuestStatus.itemProgress[inventoryItemData.itemName] += amountToGive;

            // Kurangi item dari inventaris pemain
            inventoryItemData.count -= amountToGive;

            Debug.Log($"Diberikan {amountToGive} {inventoryItemData.itemName} untuk quest '{activeQuestStatus.Quest.questName}'.");

            // Setelah memberikan item, periksa apakah quest tersebut sekarang sudah selesai
            CheckIfQuestIsComplete(activeQuestStatus);

            return true; // Berhasil memberikan item
        }

        return false; // Gagal memberikan item
    }

    // Buat fungsi helper baru untuk memeriksa penyelesaian quest
    private void CheckIfQuestIsComplete(PlayerQuestStatus questStatus)
    {
        bool allRequirementsMet = true;

        // Periksa setiap item yang dibutuhkan
        foreach (var requirement in questStatus.Quest.itemRequirements)
        {
            // Jika progres item saat ini masih kurang dari yang dibutuhkan
            if (questStatus.itemProgress[requirement.itemName] < requirement.count)
            {
                allRequirementsMet = false; // Tandai quest belum selesai
                break; // Tidak perlu cek lagi, keluar dari loop
            }
        }

        // Jika semua persyaratan sudah terpenuhi
        if (allRequirementsMet)
        {
            Debug.Log($"SELURUH ITEM UNTUK QUEST '{questStatus.Quest.questName}' TELAH TERKUMPUL!");

            // Panggil fungsi di QuestManager untuk menyelesaikan quest secara resmi
            QuestManager.Instance.CompleteQuest(questStatus);

            // Di sini Anda bisa memicu dialog "selesai"
            dialogueSystem.theDialogues = questStatus.Quest.finishDialogue;
            dialogueSystem.StartDialogue();
        }
        else
        {
            // Jika belum selesai, Anda bisa memicu dialog "pengingat"
            // dialogueSystem.theDialogues = questStatus.Quest.reminderDialogue;
            // dialogueSystem.StartDialogue();
            Debug.Log("Quest belum selesai, masih ada item yang dibutuhkan.");
        }
    }







    public void CheckFinishQuest(string nameQuest, int idChapter)
    {
        //foreach (var chapter in QuestManager.Instance.chapters)
        //{
        //    if (chapter.idChapter == idChapter)
        //    {
        //        foreach (var quest in chapter.sideQuest )
        //        {
        //            if (quest.questName == nameQuest)
        //            {
        //                bool allItemsComplete = true;

        //                foreach (var item in quest.itemQuests)
        //                {
        //                    if (item.count > 0) // Jika ada item yang belum selesai
        //                    {
        //                        Debug.Log("jumlah item : " + item.count);
        //                        allItemsComplete = false;
        //                        break;
        //                    }
        //                }

        //                if (allItemsComplete)
        //                {
                            
        //                    dialogueSystem.theDialogues = quest.finish;
        //                    dialogueSystem.StartDialogue();
        //                    gameEconomy.money += quest.reward;

        //                    if (quest.rewards != null && quest.rewards.Length > 0)
        //                    {
        //                        dialogueSystem.theDialogues = quest.rewardItemQuest;
        //                        dialogueSystem.StartDialogue();
                               
        //                        for (int i = 0; i < quest.rewards.Length; i++)
        //                        {
        //                            // Membuat salinan baru dari item yang ada untuk menghindari modifikasi referensi langsung
        //                            ItemPool.Instance.AddItem(quest.rewards[i]);
        //                        }


        //                    }

        //                    Debug.Log("selesai: " + quest.questName);
        //                    if (quest.isInGrief)
        //                    {
        //                        Player_Health.Instance.HealGriefStep();
        //                    }
        //                    quest.questComplete = true;
        //                    if (idChapter == chapter.idChapter)
        //                    {
        //                        chapter.currentSideQuest++;
        //                        if (chapter.currentSideQuest == chapter.sideQuest.Length)
        //                        {
        //                            QuestManager.Instance.ScheduleNextMainQuest(chapter.idChapter);

        //                        }
        //                        QuestManager.Instance.UpdateDateSideQuest();
        //                    }
        //                    quest.questActive = false;
        //                    QuestManager.Instance.CheckQuest();
        //                    questInfoUI.SetQuestInActive(quest.questName);
        //                }
        //                else
        //                {
        //                    QuestManager.Instance.notFinished.TheDialogues[0].name = npcName;
        //                    QuestManager.Instance.notFinished.mainSpeaker = npcName;
        //                    dialogueSystem.theDialogues = QuestManager.Instance.notFinished;
        //                                dialogueSystem.StartDialogue();
        //                }
        //            }
        //        }
        //    }
        //}
    }


    public void CheckFinishMainQuest()
    {
        bool allItemMainQuest = true;
       //foreach(var item in QuestManager.Instance.currentMainQuest.itemQuests)
       // {
       //     if (item.stackCount >0)
       //     {
       //         allItemMainQuest = false;
       //         break;
       //     }

           
       // }

        if (allItemMainQuest)
        {
            // Tandai quest selesai dan jalankan dialog selesai
            //QuestManager.Instance.currentMainQuest.finish.TheDialogues[0].name = npcName;
            //QuestManager.Instance.currentMainQuest.finish.mainSpeaker = npcName;
            //dialogueSystem.theDialogues = QuestManager.Instance.currentMainQuest.finish;
            //dialogueSystem.StartDialogue();
            //gameEconomy.money += QuestManager.Instance.currentMainQuest.reward;

            //if (QuestManager.Instance.currentMainQuest.rewards.Length > 0)
            //{
            //    dialogueSystem.theDialogues = QuestManager.Instance.currentMainQuest.rewardItemQuest;
            //    dialogueSystem.StartDialogue();
            //    for (int i = 0; i < QuestManager.Instance.currentMainQuest.rewards.Length; i++)
            //    {
            //        // Pastikan itemReward tidak null untuk menghindari NullReferenceException
            //        if (QuestManager.Instance.currentMainQuest.rewards[i].itemReward != null)
            //        {
            //            // Mengambil nama item dari itemReward
            //            string itemRewardName = QuestManager.Instance.currentMainQuest.rewards[i].itemReward.name;
            //            Item itemReward = QuestManager.Instance.currentMainQuest.rewards[i].itemReward;

            //            // Debug untuk memastikan nama itemReward terambil dengan benar
            //            Debug.Log($"Item Reward Name: {itemRewardName}");

            //            // Men-drop item menggunakan ItemPool
            //            ItemPool.Instance.DropItem(itemRewardName, transform.position + new Vector3(0, 0.5f, 0), itemReward.prefabItem);
            //        }
            //    }
            //}


            //Debug.Log("selesai: " + QuestManager.Instance.currentMainQuest.questName);
            //QuestManager.Instance.currentMainQuest.questComplete = true;
            
            //QuestManager.Instance.currentMainQuest.questActive = false;
            //QuestManager.Instance.MainQuestSelesai();
            //questInfoUI.SetQuestInActive(QuestManager.Instance.currentMainQuest.questName);

        }
        else
        {
            // Dialog "belum selesai"
            //QuestManager.Instance.notFinished.TheDialogues[0].name = npcName;
            //QuestManager.Instance.notFinished.mainSpeaker = npcName;
            //dialogueSystem.theDialogues = QuestManager.Instance.notFinished;
            dialogueSystem.StartDialogue();
        }
    }

    private int CheckFriendshipItem(NPCManager.Frendship friendship, string itemName, ref int stackItem)
    {
        if (friendship == null) return 0;
        if (stackItem <= 0) return 0;

        if (IsItemInArray(friendship.favorites, itemName))
        {
            stackItem -= 1; // Kurangi stackItem sebelum return
            return friendship.favoriteValue;
        }
        else if (IsItemInArray(friendship.like, itemName))
        {
            stackItem -= 1; // Kurangi stackItem sebelum return
            return friendship.likesValue;
        }
        else if (IsItemInArray(friendship.normal, itemName))
        {
            stackItem -= 1; // Kurangi stackItem sebelum return
            return friendship.normalValue;
        }
        else if (IsItemInArray(friendship.hate, itemName))
        {
            stackItem -= 1; // Kurangi stackItem sebelum return
            return friendship.hateValue;
        }

        return 0;
    }

   

    //public void CheckFinishMiniQuest(string nameQuest)
    //{
    //    var miniQuest = QuestManager.Instance.currentMiniQuest; // Menyimpan reference mini quest aktif

    //    // Cek apakah miniQuest valid dan judulnya sesuai dengan quest yang dimaksud
    //    if (miniQuest == null || miniQuest.judulQuest != nameQuest)
    //        return;

    //    bool allItemsComplete = true;

    //    // Cek apakah semua item sudah selesai
    //    foreach (var item in miniQuest.itemsQuest)
    //    {
    //        //if (item.stackCount > 0) // Jika masih ada item yang belum selesai
    //        //{
    //        //    Debug.Log("Jumlah item belum selesai: " + item.stackCount);
    //        //    allItemsComplete = false;
    //        //    break;
    //        //}
    //    }

    //    if (allItemsComplete)
    //    {
    //        // Semua item selesai, mulai dialog selesai quest
    //        dialogueSystem.theDialogues = miniQuest.finishDialogue;
    //                    dialogueSystem.StartDialogue();

    //        // Menambah reward uang ke ekonomi
    //        gameEconomy.money += miniQuest.rewardQuest;

    //        // Cek apakah ada item reward
    //        if (miniQuest.rewardItemQuest != null)
    //        {
    //            dialogueSystem.theDialogues = miniQuest.rewardDialogueQuest;
    //                        dialogueSystem.StartDialogue();

    //            // Membuat salinan item dan menambahkannya ke inventory
    //            Item itemCopy = miniQuest.rewardItemQuest;
    //            //itemCopy.stackCount = miniQuest.rewardItemQuest.stackCount;

    //            //// Ambil item dari pool dan tambahkan ke inventory
    //            //Item itemFromPool = ItemPool.Instance.GetItem(itemCopy.itemName, itemCopy.stackCount);
    //            //if (itemFromPool != null)
    //            //{
    //            //    //Player_Inventory.Instance.AddItem(itemFromPool);
    //            //    Debug.Log($"Item: {itemFromPool.itemName}, Jumlah: {itemFromPool.stackCount}");
    //            //}
    //        }

    //        Debug.Log("Quest selesai: " + miniQuest.judulQuest);

    //        // Tandai quest sebagai selesai dan nonaktifkan
    //        miniQuest.questComplete = true;
    //        miniQuest.questActive = false;

    //        // Update quest side jika perlu
    //        //QuestManager.Instance.UpdateDateSideQuest();
    //        QuestManager.Instance.CheckQuest();

    //        // Update UI quest yang sudah tidak aktif
    //        questInfoUI.SetQuestInActive(miniQuest.judulQuest);

    //        // Reset quest
    //        QuestManager.Instance.currentMiniQuest = null;
    //    }
    //    else
    //    {
    //        // Jika masih ada item yang belum selesai, beri dialog pengingat
    //        QuestManager.Instance.notFinished.TheDialogues[0].name = npcName;
    //        QuestManager.Instance.notFinished.mainSpeaker = npcName;

    //        dialogueSystem.theDialogues = QuestManager.Instance.notFinished;
    //        dialogueSystem.StartDialogue();
    //    }
    //}




    private bool IsItemInArray(Item[] items, string itemName)
    {
        if (items == null) return false;

        foreach (var item in items)
        {
            if (item.name == itemName)
            {
                return true;
            }
        }

        return false;
    }

    public void JobNPC()
    {

    }


}
