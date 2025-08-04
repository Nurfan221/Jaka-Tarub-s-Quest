using System;
using System.Linq;
using UnityEngine;
using System.Collections;

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
    private Schedule currentActivity;
    private Coroutine movementCoroutine; // Tambahkan ini untuk mengelola coroutine



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
            CheckSchedule();
        }
        // Jika sedang bergerak, biarkan coroutine yang mengurusnya, tidak perlu di sini
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
        // Temukan jadwal terbaru yang seharusnya sudah dimulai
        Schedule newSchedule = npcData.schedules
            .Where(s => TimeManager.Instance.hour >= s.startTime)
            .OrderByDescending(s => s.startTime) // Urutkan dari yang paling lambat
            .FirstOrDefault(); // Ambil yang paling lambat (terbaru)

        // Jika tidak ada jadwal yang cocok atau jadwalnya sama dengan yang sedang berjalan, keluar
        if (newSchedule == null || newSchedule == currentActivity)
        {
            return;
        }

        // Mulai aktivitas baru
        StartActivity(newSchedule);
    }

    public void StartActivity(Schedule newSchedule)
    {
        // Hentikan coroutine pergerakan lama jika ada
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        currentActivity = newSchedule;
        currentWaypointIndex = 0;
        isMoving = true;

        Debug.Log($"NPC '{npcName}' memulai aktivitas: {currentActivity.activityName}");

        // Mulai coroutine pergerakan baru
        movementCoroutine = StartCoroutine(FollowWaypoints(currentActivity.waypoints));
    }

    private IEnumerator FollowWaypoints(Vector3[] waypoints)
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            isMoving = false;
            yield break;
        }

        foreach (Vector3 targetPosition in waypoints)
        {
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
                yield return null; // Tunggu satu frame
            }
        }

        // Setelah selesai, hentikan pergerakan
        isMoving = false;
        Debug.Log($"NPC '{npcName}' telah sampai di tujuan terakhir.");
    }
    public void OverrideForQuest(Vector3 newPosition, Dialogues newDialogue)
    {
        isLockedForQuest = true;
        isQuestLocation = true;
        preQuestPosition = transform.position;
        questOverrideDialogue = newDialogue;

        // Pindah ke posisi quest dengan pergerakan halus (Coroutines)
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(MoveToTargetPosition(newPosition, true));

        // Pastikan NPC terlihat
        gameObject.SetActive(true);
    }

    public void ReturnToPreQuestPosition()
    {
        if (!isLockedForQuest) return;

        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(MoveToTargetPosition(preQuestPosition, false));
    }

    // --- FUNGSI BANTUAN UNTUK PERGERAKAN HALUS (COROUTINE) ---
    private IEnumerator MoveToTargetPosition(Vector3 targetPosition, bool isQuestMove)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
            yield return null;
        }

        // Setelah sampai, atur status
        if (!isQuestMove)
        {
            ReturnToNormalSchedule(); // Kembalikan ke jadwal normal
        }
        Debug.Log($"NPC '{npcName}' telah sampai di posisi target.");
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