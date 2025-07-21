using UnityEngine;
using System.Linq;

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
        // Cek apakah quest valid
        if (questStatus == null || questStatus.Progress != QuestProgress.Accepted)
        {
            return;
        }

        bool allRequirementsMet = true;
        foreach (var requirement in questStatus.Quest.itemRequirements)
        {
            if (questStatus.itemProgress[requirement.itemName] < requirement.count)
            {
                allRequirementsMet = false;
                break;
            }
        }

        // JIKA SEMUA SYARAT TERPENUHI...
        if (allRequirementsMet)
        {
            // Biarkan QuestManager yang mengurus sisanya (dialog, hadiah, save).
            QuestManager.Instance.CompleteQuest(questStatus);
        }
        else
        {
            // Jika belum selesai, Anda bisa memicu dialog "pengingat" di sini
            Debug.Log("Quest belum selesai, masih ada item yang dibutuhkan.");
        }
    }

    // HAPUS SEMUA FUNGSI LAMA: CheckFinishQuest, CheckFinishMainQuest, CheckFinishMiniQuest.
}