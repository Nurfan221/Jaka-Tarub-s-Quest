using System;
using System.Linq;
using UnityEngine;

public class NPCBehavior : MonoBehaviour
{

    private NpcSO npcData;

    public string npcName { get; private set; }
    public bool isLockedForQuest = false;
    public bool isQuestLocation;
    public Dialogues questOverrideDialogue; // Dialog sementara dari quest
    public Dialogues normalDialogue; // Dialog normal NPC
    private bool isMoving = false;
    private int currentWaypointIndex = 0;
    private Vector3 preQuestPosition;
    public float movementSpeed = 2.0f;
    private Schedule currentActivity; // Aktivitas yang sedang dilakukan NPC


    public void Initialize(NpcSO data)
    {
        this.npcData = data;
        this.npcName = data.fullName;
        this.name = data.fullName; // Ganti nama GameObject agar mudah dicari
        transform.position = data.schedules[0].waypoints[0]; // Mulai di waypoint pertama dari jadwal pertama
    }

    private void Update()
    {
        // Jangan lakukan apa-apa jika sedang dikunci oleh quest
        if (isLockedForQuest) return;

        // Jika tidak sedang bergerak, periksa jadwal
        if (!isMoving)
        {
            //CheckSchedule();
        }
        else
        {
            // Jika sedang bergerak, lanjutkan pergerakan
            MoveToNextWaypoint();
        }
    }

    public void OverrideForQuest(Vector3 newPosition, Dialogues newDialogue)
    {
        Debug.Log($"NPC {this.name} sekarang dibajak oleh quest.");
        Debug.Log($"NPC {this.name} akan pindah ke posisi {newPosition} dengan dialog quest baru.");
        // Pastikan NPC tidak sedang bergerak
        isLockedForQuest = true; // Kunci jadwalnya!
        isQuestLocation = true; // Tandai bahwa NPC berada di lokasi quest
        preQuestPosition = transform.position; // Simpan lokasi saat ini
        transform.position = newPosition; // Pindahkan langsung ke lokasi quest
        questOverrideDialogue = newDialogue; // Simpan dialog quest sementaranya


        // Pastikan NPC terlihat jika sebelumnya tidak aktif
        gameObject.SetActive(true);
    }

    public void ReturnToPreQuestPosition()
    {
        // Jangan lakukan apa-apa jika NPC tidak sedang dalam mode quest
        if (!isLockedForQuest) return;

        Debug.Log($"NPC {this.name} kembali ke posisi semula di {preQuestPosition}");
        transform.position = preQuestPosition;
    }


    // Perintah dari luar untuk melepaskan NPC kembali ke jadwal normalnya.

    public void ReturnToNormalSchedule()
    {
        Debug.Log($"NPC {this.name} kembali ke jadwal normal.");
        isLockedForQuest = false;
        questOverrideDialogue = null;
        // NPC akan otomatis melanjutkan jadwalnya di frame Update berikutnya.
    }
    // NPC memeriksa jadwalnya sendiri berdasarkan waktu saat ini.
    private void CheckSchedule()
    {
        // Reset status 'hasStarted' setiap hari baru (misal, jam 4 pagi)
       

        foreach (var schedule in npcData.schedules)
        {
            if (TimeManager.Instance.hour >= schedule.startTime)
            {
                StartActivity(schedule);
                break; // Hanya mulai satu aktivitas
            }
        }
    }

    public void StartActivity(Schedule schedule)
    {
        currentActivity = schedule;
        currentWaypointIndex = 0;
        isMoving = true;
    }

    private void MoveToNextWaypoint()
    {
        if (currentActivity == null || currentWaypointIndex >= currentActivity.waypoints.Length)
        {
            isMoving = false;
            return;
        }

        Vector3 targetPosition = currentActivity.waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= currentActivity.waypoints.Length)
            {
                isMoving = false; // Aktivitas selesai
            }
        }
    }

    // Perintahkan NPC untuk pindah ke lokasi quest.

    public void MoveToQuestLocation(Vector3 position)
    {
        isLockedForQuest = true; // Kunci jadwal normal
        isMoving = false; // Hentikan pergerakan saat ini
        transform.position = position; // Pindahkan langsung ke lokasi quest
        Debug.Log($"NPC {npcName} dipindahkan ke lokasi quest.");
    }



    public bool CheckItemGive(ItemData inventoryItemData)
    {
        Debug.Log($"NPC {this.npcName}: Pemain mencoba memberikan item {inventoryItemData.itemName}");

        // Panggil fungsi terpusat di QuestManager untuk memproses item
        bool itemProcessedByQuest = QuestManager.Instance.ProcessItemGivenToNPC(inventoryItemData, this.npcName); // <<< PENTING

        if (itemProcessedByQuest)
        {
          

            Debug.Log($"NPC {this.npcName}: Item '{inventoryItemData.itemName}' telah diproses oleh sistem quest.");
            // Panggil fungsi untuk memperbarui UI Inventaris pemain
            // MechanicController.Instance.HandleUpdateInventory(); // Contoh
            return true; // Item berhasil diberikan dan diproses
        }
        else
        {
            Debug.Log($"NPC {this.npcName}: Item '{inventoryItemData.itemName}' tidak relevan untuk quest manapun.");
            return false; // Item tidak diproses
        }
    }


}