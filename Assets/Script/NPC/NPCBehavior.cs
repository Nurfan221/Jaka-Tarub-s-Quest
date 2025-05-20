using UnityEngine;

using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using static QuestManager;
using static UnityEditor.Progress;


public class NPCBehavior : MonoBehaviour
{
    [Header("Daftar Hubungan")]
    [SerializeField] QuestManager questManager;
    [SerializeField] protected DialogueSystem dialogueSystem;
    [SerializeField] NPCManager npcManager;
    [SerializeField] GameEconomy gameEconomy;
    [SerializeField] QuestInfoUI questInfoUI;
    private NPCManager.Schedule currentActivity; // Gunakan NPCManager.Schedule untuk mendeklarasikan tipe

    [SerializeField] NPCAnimation npcAnimation;



    public string npcName;
    public Vector3 startPosition;
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
    
    public bool CheckItemGive( ref int stackItem)
    {
        Debug.Log("check item give di jalankan ");
        bool isItemGiven = false;

        foreach (var chapter in questManager.chapters)
        {
            foreach (var quest in chapter.sideQuest)
            {
                if ( npcName == quest.NPC.name)
                {
                    foreach (var item in quest.itemQuests)
                    {
                        if (itemQuest == item.item.name && quest.questActive)
                        {
                            if (item.jumlah > 0) // Pastikan masih ada item yang diperlukan
                            {
                                int jumlahDiBerikan = Mathf.Min(stackItem, item.jumlah); // tentukan jumlah item yang di berikan
                                item.jumlah -= jumlahDiBerikan; // Kurangi jumlah item
                                stackItem -= jumlahDiBerikan;

                                // Ambil idChapter dari chapter saat ini
                                int idChapter = chapter.idChapter;

                                CheckFinishQuest(quest.questName, idChapter);


                                Debug.Log($"Quest aktif: {quest.questName}, NPC: {quest.NPC.name}, Item: {itemQuest}");
                                Debug.Log($"Sisa jumlah item quest: {item.jumlah}");
                                isItemGiven = true;
                                return true;
                                break; // Berhenti setelah menemukan quest yang sesuai
                            }
                            else
                            {
                                Debug.Log($"Item untuk quest {quest.questName} sudah habis!");

                            }

                        }
                        else
                        {
                            Debug.Log($"item atau npc tidak sama : itemquest {itemQuest} item {item.item.name} npcName {npcName} npc {quest.NPC.name}");
                            return false;
                        }
                    }
                }
                
            }

        }



        // checked jika item tidak ada di dalam quest
        // maka akan menambahkan point frendships sesuai array frendships
        // CheckItemForfrendship(); // Loop tambahan: Cek itemQuest pada array friendship NPC
        if (!isItemGiven)
        {
            //Debug.Log("UHUYyyyyyyyu");
            foreach (var npcData in npcManager.npcDataArray)
            {
                int addedValue = CheckFriendshipItem(npcData.frendship, itemQuest, ref stackItem);
                if (addedValue > 0)
                {
                    npcData.totalFrendships += addedValue;
                    //Debug.Log($"Item {itemQuest} ditemukan di kategori persahabatan NPC {npcData.prefab.name}. Total persahabatan sekarang: {npcData.totalFrendships}");
                    isItemGiven = true;
                    break;
                }
            }

            if (!isItemGiven)
            {
                Debug.Log("Tidak ada quest aktif atau item/NPC tidak sesuai.");
            }
        }

        if(questManager.chapter1IsDone)
        {
            if (questManager.currentMainQuest != null)
            {
                if (questManager.currentMainQuest.NPC.name == npcName)
                {
                    foreach (var item in questManager.currentMainQuest.itemQuests)
                    {
                        if (itemQuest == item.item.name && item.jumlah > 0)
                        {
                            int jumlahDiBerikan = Mathf.Min(stackItem, item.jumlah);
                            item.jumlah -= jumlahDiBerikan;
                            stackItem = jumlahDiBerikan;
                            Debug.Log("main Quest Selesai");
                            CheckFinishMainQuest();
                        }
                    }
                }
            }
        }

        return isItemGiven;
    }




    public void CheckFinishQuest(string nameQuest, int idChapter)
    {
        foreach (var chapter in questManager.chapters)
        {
            foreach (var quest in chapter.sideQuest)
            {
                if (quest.questName == nameQuest)
                {

                    bool allItemsComplete = true; // Asumsi semua item selesai

                    foreach (var item in quest.itemQuests)
                    {
                        if (item.jumlah > 0) // Jika ada item yang belum selesai
                        {
                            allItemsComplete = false; // Tandai bahwa quest belum selesai
                            break; // Tidak perlu melanjutkan pengecekan
                        }
                    }

                    if (allItemsComplete)
                    {
                        // Tandai quest selesai dan jalankan dialog selesai
                        quest.finish.TheDialogues[0].name = npcName;
                        quest.finish.mainSpeaker = npcName;
                        dialogueSystem.theDialogues = quest.finish;
                        dialogueSystem.StartDialogue();
                        gameEconomy.money += quest.reward;

                        if (quest.rewards.Length > 0)
                        {
                            dialogueSystem.theDialogues = quest.rewardItemQuest;
                            dialogueSystem.StartDialogue();
                            for (int i = 0; i < quest.rewards.Length; i++)
                            {
                                // Pastikan itemReward tidak null untuk menghindari NullReferenceException
                                if (quest.rewards[i].itemReward != null)
                                {
                                    // Mengambil nama item dari itemReward
                                    string itemRewardName = quest.rewards[i].itemReward.name;
                                    GameObject itemReward = quest.rewards[i].itemReward;

                                    // Debug untuk memastikan nama itemReward terambil dengan benar
                                    Debug.Log($"Item Reward Name: {itemRewardName}");

                                    // Men-drop item menggunakan ItemPool
                                    ItemPool.Instance.DropItem(itemRewardName, transform.position + new Vector3(0, 0.5f, 0), itemReward);
                                }
                            }
                        }


                        Debug.Log("selesai: " + quest.questName);
                        quest.questComplete = true;
                        if (idChapter == chapter.idChapter)
                        {
                            chapter.currentSideQuest++;
                            questManager.InputAntrianMainQuest();

                        }
                        quest.questActive = false;
                        questManager.CheckQuest();
                        questInfoUI.SetQuestInActive(quest.questName);

                    }
                    else
                    {
                        // Dialog "belum selesai"
                        questManager.notFinished.TheDialogues[0].name = npcName;
                        questManager.notFinished.mainSpeaker = npcName;
                        dialogueSystem.theDialogues = questManager.notFinished;
                        dialogueSystem.StartDialogue();
                    }
                }
            }
        }
    }

    public void CheckFinishMainQuest()
    {
        bool allItemMainQuest = true;
       foreach(var item in questManager.currentMainQuest.itemQuests)
        {
            if (item.jumlah >0)
            {
                allItemMainQuest = false;
                break;
            }

           
        }

        if (allItemMainQuest)
        {
            // Tandai quest selesai dan jalankan dialog selesai
            questManager.currentMainQuest.finish.TheDialogues[0].name = npcName;
            questManager.currentMainQuest.finish.mainSpeaker = npcName;
            dialogueSystem.theDialogues = questManager.currentMainQuest.finish;
            dialogueSystem.StartDialogue();
            gameEconomy.money += questManager.currentMainQuest.reward;

            if (questManager.currentMainQuest.rewards.Length > 0)
            {
                dialogueSystem.theDialogues = questManager.currentMainQuest.rewardItemQuest;
                dialogueSystem.StartDialogue();
                for (int i = 0; i < questManager.currentMainQuest.rewards.Length; i++)
                {
                    // Pastikan itemReward tidak null untuk menghindari NullReferenceException
                    if (questManager.currentMainQuest.rewards[i].itemReward != null)
                    {
                        // Mengambil nama item dari itemReward
                        string itemRewardName = questManager.currentMainQuest.rewards[i].itemReward.name;
                        GameObject itemReward = questManager.currentMainQuest.rewards[i].itemReward;

                        // Debug untuk memastikan nama itemReward terambil dengan benar
                        Debug.Log($"Item Reward Name: {itemRewardName}");

                        // Men-drop item menggunakan ItemPool
                        ItemPool.Instance.DropItem(itemRewardName, transform.position + new Vector3(0, 0.5f, 0), itemReward);
                    }
                }
            }


            Debug.Log("selesai: " + questManager.currentMainQuest.questName);
            questManager.currentMainQuest.questComplete = true;
            
            questManager.currentMainQuest.questActive = false;
            questManager.MainQuestSelesai();
            //questInfoUI.SetQuestInActive(questManager.currentMainQuest.questName);

        }
        else
        {
            // Dialog "belum selesai"
            questManager.notFinished.TheDialogues[0].name = npcName;
            questManager.notFinished.mainSpeaker = npcName;
            dialogueSystem.theDialogues = questManager.notFinished;
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




}
