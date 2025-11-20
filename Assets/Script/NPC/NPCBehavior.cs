using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class NPCBehavior : MonoBehaviour
{

    private NpcSO npcData;

    public string npcName { get; private set; }
    public bool isLockedForQuest = false;
    public bool islocked; // Apakah NPC sedang dikunci oleh quest dan tidak mengikuti jadwal normal
    public bool isQuestLocation;
    public Dialogues questOverrideDialogue; // Dialog sementara dari quest
    public Dialogues normalDialogue; // Dialog normal NPC
    private bool isMoving = false;
    private int currentWaypointIndex = 0;
    private Vector3 preQuestPosition;
    public float movementSpeed = 2.0f;
    private Schedule currentActivity;
    private Coroutine movementCoroutine; // Tambahkan ini untuk mengelola coroutine
    public bool isTeleporting = false; // Flag penanda


    [Tooltip("Jarak hitbox dari pusat NPC")]
    public float hitboxRadius = 1.0f;
    public Transform hitboxTransform;
    private Vector3 lastPosition;
    private Vector2 lastMovedDirection = Vector2.right; // Default menghadap kanan (bisa diubah)
    [Tooltip("Geser titik pusat lingkaran ke atas (agar sejajar badan, bukan kaki)")]
    public float verticalOffset = -2f; // Coba ubah angka ini di inspector (misal 0.5 atau 0.8)

    [Header("variabel for quest")]
    public ItemData itemQuestToGive; // Item yang dimiliki NPC untuk quest ini
    public bool isGivenItemForQuest = false; // Apakah NPC sudah memberikan item quest
    Vector2[] questWaypoint;

    [Tooltip("Seberapa jauh emoticon akan miring (dalam derajat).")]
    public float rotationAngle;
    public Transform emoticonTransform; // Transform dari emoticon

    [Tooltip("Seberapa cepat emoticon akan berganti arah (dalam detik).")]
    public float wiggleSpeed;

    private void OnEnable()
    {
        // Mulai Coroutine saat emoticon ditampilkan.
        StartCoroutine(WiggleRoutine());
    }

    // OnDisable dipanggil saat GameObject dinonaktifkan.
    private void OnDisable()
    {
        // Hentikan Coroutine saat emoticon disembunyikan untuk mencegah error.
        StopAllCoroutines();
    }

    private void Awake()
    {
        emoticonTransform = transform.Find("Emoticon");

        // Lakukan pengecekan SETELAH mencoba mencari.
        if (emoticonTransform != null)
        {
            // Jika berhasil ditemukan...
            //Debug.Log("Objek Emoticon berhasil ditemukan!", this.gameObject);
            // Sembunyikan emoticon pada awalnya.
            emoticonTransform.gameObject.SetActive(false);
        }
        else
        {
            // Jika tidak ditemukan, beri peringatan agar mudah di-debug.
            Debug.LogWarning("Tidak bisa menemukan GameObject anak dengan nama 'Emoticon' pada NPC ini!", this.gameObject);
        }
    }


    public void Initialize(NpcSO data)
    {
        this.npcData = data;
        this.npcName = data.fullName;
        this.name = data.fullName; // Ganti nama GameObject agar mudah dicari
        transform.position = data.schedules[0].waypoints[0]; // Mulai di waypoint pertama dari jadwal pertama
    }
    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        // Jangan lakukan apa-apa jika sedang dikunci oleh quest

        // Jika tidak sedang bergerak, periksa jadwal
        if (!isMoving && !islocked)
        {
            CheckSchedule();
        }

        UpdateHitboxPosition();
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

        gameObject.transform.position = newSchedule.waypoints[0];

        currentActivity = newSchedule;
        currentWaypointIndex = 0;
        isMoving = true;

        //Debug.Log($"NPC '{npcName}' memulai aktivitas: {currentActivity.activityName}");

        // Mulai coroutine pergerakan baru
        movementCoroutine = StartCoroutine(FollowWaypoints(currentActivity.waypoints));
    }

    private IEnumerator FollowWaypoints(Vector2[] waypoints)
    {
        Debug.Log($"{gameObject.name} [DEBUG] NPCMemulai FollowWaypoints. Jumlah titik: {waypoints.Length}");

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("{gameObject.name} [DEBUG] NPCWaypoints kosong! Berhenti.");
            isMoving = false;
            yield break;
        }

        for (int i = 0; i < waypoints.Length; i++)
        {
            Vector2 targetPosition = waypoints[i];
            Debug.Log($"{gameObject.name} [DEBUG] NPCMenuju Waypoint [{i}]: {targetPosition}. Posisi Saat Ini: {transform.position}");

            // Jika NPC masih dalam status cooldown teleport (isTeleporting = true),
            // tahan dulu di sini. Jangan lanjut jalan dulu sampai cooldown selesai.
            while (isTeleporting)
            {
                Debug.Log($"{gameObject.name} [DEBUG] NPCMenunggu cooldown teleport selesai... (Posisi: {transform.position})");
                yield return null; // Tunggu frame berikutnya
            }

            // Loop pergerakan menuju satu titik
            while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
            {
                // Jika tiba-tiba teleport aktif SAAT sedang berjalan
                if (isTeleporting)
                {
                    Debug.Log("{gameObject.name} [DEBUG] NPCTeleport terdeteksi saat bergerak! Membatalkan waypoint ini.");
                    // waypoint berikutnya tidak akan langsung di-skip.
                    break;
                }

                transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
                yield return null; // Tunggu satu frame
            }

            Debug.Log($"{gameObject.name} [DEBUG] NPCSelesai dengan Waypoint [{i}] atau di-skip karena teleport.");
        }

        // Setelah selesai semua titik
        isMoving = false;
        Debug.Log("{gameObject.name} [DEBUG] NPCSemua Waypoints selesai dilalui. isMoving = false.");
    }
    public void OverrideForQuest(Vector2 startPosition, Vector2 finishLocation, Dialogues newDialogue, string nameEmoticon)
    {
        // Kunci NPC
        islocked = true;
        isLockedForQuest = true;
        isQuestLocation = true;
        preQuestPosition = transform.position;
        questOverrideDialogue = newDialogue;

        questWaypoint = new Vector2[] { startPosition, finishLocation };
        ShowEmoticon(nameEmoticon);

        if (movementCoroutine != null) StopCoroutine(movementCoroutine);

        // Ini mencegah Update() memanggil CheckSchedule() secara tidak sengaja
        isMoving = true;

        movementCoroutine = StartCoroutine(FollowWaypoints(questWaypoint));

        gameObject.SetActive(true);
    }

    public void ReturnToPreQuestPosition()
    {
        Debug.Log($"NPC {this.npcName} kembali ke posisi sebelum quest.");
        if (!isLockedForQuest) return;
        isLockedForQuest = false;
        islocked = false;
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        Vector2 questWaypointZero = questWaypoint[0];
        questWaypoint[0] = questWaypoint[1];
        questWaypoint[1] = questWaypointZero;
        movementCoroutine = StartCoroutine(FollowWaypoints(questWaypoint));

        CheckSchedule();

    }

    // FUNGSI BANTUAN UNTUK PERGERAKAN HALUS (COROUTINE) 
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

    //public void MoveToQuestLocation(Vector3 position)
    //{
    //    isLockedForQuest = true; // Kunci jadwal normal
    //    isMoving = false; // Hentikan pergerakan saat ini
    //    transform.position = position; // Pindahkan langsung ke lokasi quest
    //    Debug.Log($"NPC {npcName} dipindahkan ke lokasi quest.");
    //}





    private IEnumerator WiggleRoutine()
    {
        // Loop ini akan berjalan selamanya selama objek aktif.
        while (true)
        {
            // Miringkan ke satu sisi (misal, 1 derajat pada sumbu Z).
            // Kita menggunakan 'localRotation' karena ini adalah objek anak (child).
            emoticonTransform.localRotation = Quaternion.Euler(0, 0, rotationAngle);

            // Jeda sejenak.
            yield return new WaitForSeconds(wiggleSpeed);

            // Miringkan ke sisi berlawanan (misal, -1 derajat pada sumbu Z).
            emoticonTransform.localRotation = Quaternion.Euler(0, 0, -rotationAngle);

            //  Jeda sejenak lagi, lalu loop akan kembali ke awal.
            yield return new WaitForSeconds(wiggleSpeed);
        }
    }

    public void ShowEmoticon(string nameEmote)
    {
        Debug.Log ($"NPC {this.npcName}: Menampilkan emoticon '{nameEmote}'");
        if (emoticonTransform != null)
        {
            emoticonTransform.gameObject.SetActive(true);
            SpriteRenderer sr = emoticonTransform.GetComponent<SpriteRenderer>();
            sr.sprite = DatabaseManager.Instance.emoticonDatabase.emoticonDatabase.Find(e => e.emoticonName == nameEmote)?.emoticonSprite;
            StartCoroutine(WiggleRoutine()); // Mulai gerakan wiggle
        }
    }
    public void OnHitboxTriggerEnter(Collider2D collision)
    {
        // Jika sedang cooldown, ABAIKAN segalanya.
        if (isTeleporting) return;

        if (collision.CompareTag("Pintu"))
        {
            PintuInteractable pintu = collision.GetComponent<PintuInteractable>();
            if (pintu != null)
            {
                Debug.Log($"[SENSOR] Hitbox mendeteksi pintu: {collision.name}. Memulai Teleport...");

                // Langsung kunci NPC SEKARANG JUGA sebelum memanggil Manager.
                // Ini menjamin saat NPC mendarat di tujuan, statusnya sudah isTeleporting = true.
                StartTeleportCooldown();

                // Baru panggil Manager untuk memindahkan posisi
                PintuManager.Instance.NPCEnterArea(pintu.idPintu, pintu.isPintuIn, this.gameObject);
            }
        }
        if (collision.CompareTag("Tree"))
        {
            Debug.Log($"[SENSOR] Hitbox mendeteksi pohon: {collision.name}.");
            TreeBehavior tree = collision.GetComponent<TreeBehavior>();
            if (tree != null) tree.InstantlyDestroy();
        }
        else if (collision.CompareTag("AkarPohon"))
        {
            Debug.Log($"[SENSOR] Hitbox mendeteksi akar: {collision.name}.");
            AkarPohon akarPohon = collision.GetComponent<AkarPohon>();
            if (akarPohon != null) akarPohon.InstantlyDestroy();
        }else if (collision.CompareTag("Stone"))
        {
            Debug.Log($"[SENSOR] Hitbox mendeteksi batu: {collision.name}.");
            StoneBehavior rock = collision.GetComponent<StoneBehavior>();
            if (rock != null) rock.InstantlyDestroy();
        }
    }
   

    // Fungsi ini akan dipanggil oleh PintuManager setelah memindahkan posisi
    public void StartTeleportCooldown()
    {
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        isTeleporting = true; // Kunci pintu agar tidak bisa dipicu
        yield return new WaitForSeconds(1f); // Tunggu 1.5 detik (sampai NPC keluar dari area pintu tujuan)
        isTeleporting = false; // Buka kunci
    }

    void UpdateHitboxPosition()
    {
        // Hitung pergerakan
        Vector3 movementDelta = transform.position - lastPosition;
        if (movementDelta.sqrMagnitude > 0.001f)
        {
            lastMovedDirection = movementDelta.normalized;
        }

        if (hitboxTransform != null)
        {
            // Hitung posisi melingkar
            Vector3 finalPosition = lastMovedDirection * hitboxRadius;

            // TERAPKAN OFFSET (Ditambah nilai negatif = Turun)
            finalPosition.y += verticalOffset;

            // Terapkan ke Transform
            hitboxTransform.localPosition = finalPosition;

            // Rotasi
            float angle = Mathf.Atan2(lastMovedDirection.y, lastMovedDirection.x) * Mathf.Rad2Deg;
            hitboxTransform.rotation = Quaternion.Euler(0, 0, angle);
        }

        lastPosition = transform.position;
    }
    private void OnDrawGizmosSelected()
    {
        // Titik pusat lingkaran (Posisi NPC + Offset vertikal)
        // Jika verticalOffset negatif, titik ini akan turun.
        Vector3 centerPoint = transform.position + new Vector3(0, verticalOffset, 0);

        // Gambar Lingkaran Lintasan (Kuning)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerPoint, hitboxRadius);

        // Gambar Hitbox Aktual (Merah)
        if (hitboxTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(centerPoint, hitboxTransform.position);
            Gizmos.DrawWireSphere(hitboxTransform.position, 0.2f);
        }
    }
}