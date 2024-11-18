using UnityEngine;

using System.Collections;


public class NPCBehavior : MonoBehaviour
{
    [Header("Daftar Hubungan")]
    [SerializeField] QuestManager questManager;
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
            Debug.LogError("Renderer tidak ditemukan pada NPC!");
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
            Debug.LogError("Schedule tidak ditemukan!");
            return;
        }

        Debug.Log($"Memulai aktivitas: {schedule.activityName} dengan {schedule.waypoints.Length} waypoints");
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
            Debug.LogError("Waypoints kosong pada aktivitas ini!");
        }
        // OnDrawGizmos();
    }



   private void MoveToNextWaypoint()
    {
        if (currentWaypointIndex < currentActivity.waypoints.Length)
        {
            Vector3 targetPosition = currentActivity.waypoints[currentWaypointIndex];
            Debug.Log($"NPC bergerak ke waypoint {currentWaypointIndex}: {targetPosition}");

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                currentWaypointIndex++;
                Debug.Log($"NPC mencapai waypoint {currentWaypointIndex - 1}");

                if (currentWaypointIndex >= currentActivity.waypoints.Length)
                {
                    isMoving = false;
                    Debug.Log($"NPC menyelesaikan aktivitas: {currentActivity.activityName}");

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
            Debug.LogError("Tidak ada waypoints untuk aktivitas ini.");
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

    public void CheckItemGive()
    {
        foreach (var quest in questManager.dailyQuest)
        {
            if (quest.questActive && npcName == quest.nameNPC.name && itemQuest == quest.itemQuest.name)
            {
                
                Debug.Log("quest active dan name npc adalah : " + quest.questName);
                Debug.Log("quest active dan name npc adalah : " + quest.nameNPC.name);
                // quest.jumlahItem--;

                quest.jumlahItem = quest.jumlahItem - 1;



                Debug.Log("item quest yang diterima : " + itemQuest);
                Debug.Log("jumlah item quest di kurangi : " + quest.jumlahItem);
                
            }else
            {
                Debug.Log("quest tidak active dan name npc adalah : " + quest.questName);
                Debug.Log("quest tidak active dan name npc adalah : " + quest.nameNPC.name);
                Debug.Log("quest tidak active dan name item adalah : " + npcName);
                Debug.Log("quest tidak active dan name npc adalah : " + quest.itemQuest.name);
                Debug.Log("quest tidak active dan name npc adalah : " + itemQuest);
                Debug.Log("quest tidak aktif atau nama npc tidak sesuai");
                // Debug.Log("nama npc : " + npcName + " nama npc di dalam array quest : " + quest.) 
                // Debug.Log("ada quest active dan nama quest sama dengan yang di drag");
            }
        }
    }
}
