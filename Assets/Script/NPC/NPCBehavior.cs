using UnityEngine;

using System.Collections;
using System.Linq;


public class NPCBehavior : MonoBehaviour
{
    [Header("Daftar Hubungan")]
    [SerializeField] QuestManager questManager;
    [SerializeField] protected DialogueSystem dialogueSystem;
    [SerializeField] NPCManager npcManager;
    [SerializeField] GameEconomy gameEconomy;
    private NPCManager.Schedule currentActivity; // Gunakan NPCManager.Schedule untuk mendeklarasikan tipe
    public NPCManager.Schedule[] dailySchedule; // Jadwal harian menggunakan tipe global
    public NPCManager.Frendship[] frendships;

    public string npcName;
    public Vector3 StartPosition;
    public float movementSpeed = 2.0f;


    private bool isMoving = false;
    private int currentWaypointIndex = 0;
    private Renderer npcRenderer;
    private bool hasStartedActivity = false;

    public string itemQuest;
    public int jumlahItem;

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
            //Debug.LogError("Schedule tidak ditemukan!");
            return;
        }

        //Debug.Log($"Memulai aktivitas: {schedule.activityName} dengan {schedule.waypoints.Length} waypoints");
        currentActivity = schedule;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (currentActivity.waypoints.Length > 0)
        {
            currentWaypointIndex = 0;
            isMoving = true;
        }
        else
        {
            //Debug.LogError("Waypoints kosong pada aktivitas ini!");
        }
        // OnDrawGizmos();
    }



   private void MoveToNextWaypoint()
    {
        if (currentWaypointIndex < currentActivity.waypoints.Length)
        {
            Vector3 targetPosition = currentActivity.waypoints[currentWaypointIndex];
            //Debug.Log($"NPC bergerak ke waypoint {currentWaypointIndex}: {targetPosition}");

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                currentWaypointIndex++;
                //Debug.Log($"NPC mencapai waypoint {currentWaypointIndex - 1}");

                if (currentWaypointIndex >= currentActivity.waypoints.Length)
                {
                    isMoving = false;
                    //Debug.Log($"NPC menyelesaikan aktivitas: {currentActivity.activityName}");

                    if (currentActivity.activityName == "MulaiSchedule")
                    {
                        StartFirstActivity();
                    }
                    else if (currentActivity.activityName == "Istirahat")
                    {
                        StartCoroutine(FadeOutAndDestroy());
                    }
                }
            }
        }
        else
        {
            //Debug.LogError("Tidak ada waypoints untuk aktivitas ini.");
            isMoving = false;
        }
    }


   private void OnDrawGizmos()
    {
        if (dailySchedule != null)
        {
            foreach (var schedule in dailySchedule)
            {
                if (schedule.waypoints != null && schedule.waypoints.Length > 0)
                {
                    Gizmos.color = Color.cyan; // Warna garis antar waypoint
                    for (int i = 0; i < schedule.waypoints.Length - 1; i++)
                    {
                        Gizmos.DrawLine(schedule.waypoints[i], schedule.waypoints[i + 1]);
                    }

                    Gizmos.color = Color.green; // Warna waypoint (bola kecil)
                    foreach (var waypoint in schedule.waypoints)
                    {
                        Gizmos.DrawSphere(waypoint, 0.2f); // Gambar bola kecil di setiap waypoint
                    }
                }
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

     public void ReceiveItem(Item item)
    {
        Debug.Log("NPC menerima item: " + item.name);
        // Tambahkan logika untuk menerima item
    }

    public bool CheckItemGive( ref int stackItem)
    {
        bool isItemGiven = false;

        foreach (var quest in questManager.activeQuests)
        {
            if (quest.questActive && npcName == quest.nameNPC.name && itemQuest == quest.itemQuest.name)
            {
                if (quest.jumlahItem > 0) // Pastikan masih ada item yang diperlukan
                {
                    int jumlahDiBerikan = Mathf.Min(stackItem,quest.jumlahItem); // tentukan jumlah item yang di berikan
                    quest.jumlahItem -= jumlahDiBerikan; // Kurangi jumlah item
                    stackItem -= jumlahDiBerikan; //
                    if (quest.jumlahItem <= 0)
                    {
                        quest.finish.TheDialogues[0].name = npcName;
                        quest.finish.mainSpeaker = npcName;
                        dialogueSystem.theDialogues = quest.finish;
                        dialogueSystem.StartDialogue();
                        gameEconomy.Money += quest.reward;

                        
                        // Debug.Log("quest selesai : " + quest.finish);
                        quest.questActive = false;
                        questManager.activeQuests.Remove(quest);
                        questManager.CheckQuest();


                        
                    }else
                    {   
                        questManager.notFinished.TheDialogues[0].name = npcName;
                        questManager.notFinished.mainSpeaker = npcName;
                        dialogueSystem.theDialogues = questManager.notFinished;
                        dialogueSystem.StartDialogue();
                    }
                    //Debug.Log($"Quest aktif: {quest.questName}, NPC: {quest.nameNPC.name}, Item: {itemQuest}");
                    //Debug.Log($"Sisa jumlah item quest: {quest.jumlahItem}");
                    isItemGiven = true;
                    break; // Berhenti setelah menemukan quest yang sesuai
                }
                else
                {
                    //Debug.Log($"Item untuk quest {quest.questName} sudah habis!");
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
                //Debug.Log("Tidak ada quest aktif atau item/NPC tidak sesuai.");
            }
        }

        return isItemGiven;
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
