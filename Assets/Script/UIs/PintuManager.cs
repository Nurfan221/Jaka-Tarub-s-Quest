using UnityEngine;
using System.Collections;
public class PintuManager : MonoBehaviour
{
    public static PintuManager Instance { get; private set; }
    public DatabaseManager databaseManager;

    [Header("Daftar Hubungan")]
    public GameObject player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.gameObject;
        if (player == null)
        {
            Debug.LogError("Player not found in the scene!");
            return;
        }
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        databaseManager = DatabaseManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void EnterArea(IdPintu idPintu, bool isPintuIn)
    {
        StartCoroutine(EnterAreaRoutine(idPintu, isPintuIn));
    }

    private IEnumerator EnterAreaRoutine(IdPintu idPintu, bool isPintuIn)
    {
      

        //  buat variabel penampung sementara
        var dataPintu = databaseManager.listPintu.Find(x => x.idPintu == idPintu);

        // Cek keamanan: Jika data tidak ditemukan, batalkan biar tidak error
        if (dataPintu == null)
        {
            Debug.LogError("Data Pintu tidak ditemukan!");
            yield break;
        }

       

        string namaLokasiYgDituju = "";

        if (isPintuIn)
        {
            // Jika Masuk: Nama lokasi sesuai data pintu (Misal: "Rumah Jaka")
            namaLokasiYgDituju = $"Masuk ke {dataPintu.lokasiName}";
        }
        else
        {
           
            namaLokasiYgDituju = $"keluar dari {dataPintu.lokasiName}";
        }

       
        Debug.Log($"Menuju ke: {namaLokasiYgDituju}");

       

        // Munculkan layar hitam loading
        StartCoroutine(LoadingScreenUI.Instance.SetLoadingandTimer(true, namaLokasiYgDituju));

        // Tunggu 1 detik agar layar benar-benar gelap SEBELUM player pindah
        yield return new WaitForSeconds(1.0f);

        

        //  pakai variabel 'dataPintu' yang sudah kita temukan di Tahap 1
        if (isPintuIn) // MASUK RUMAH
        {
            ClockManager.Instance.isIndoors = true;
            player.transform.position = dataPintu.pintuOut;
            SmoothCameraFollow.Instance.EnterHouse(true);
        }
        else // KELUAR RUMAH
        {
            ClockManager.Instance.isIndoors = false;
            player.transform.position = dataPintu.pintuIn;
            SmoothCameraFollow.Instance.EnterHouse(false);
        }

     

        SoundManager.Instance.CheckGameplayMusic(ClockManager.Instance.isIndoors, 0.1f);
        SmoothCameraFollow.Instance.SnapToTarget();
        ClockManager.Instance.UpdateDateTime();

        // Tunggu 1 detik agar layar benar-benar gelap SEBELUM player pindah
        yield return new WaitForSeconds(1.0f);

        // Trigger Tutorial (Gunakan dataPintu.lokasiName)
        if (dataPintu.lokasiName == "RumahJaka")
        {
            TutorialManager.Instance.TriggerTutorial("Tutorial_MasukHutan");
        }

    }

    public void NPCEnterArea(IdPintu idPintu, bool isPintuIn, GameObject npcObject)
    {
        Debug.Log("Nama pintu: " + idPintu);

        foreach (var pintuTujuan in databaseManager.listPintu)
        {
            if (pintuTujuan.idPintu == idPintu)
            {
                Vector3 targetPosition;

                // Tentukan posisi tujuan
                if (isPintuIn)
                {
                    targetPosition = pintuTujuan.pintuOut;
                }
                else
                {
                    targetPosition = pintuTujuan.pintuIn;
                }

                // Pindahkan NPC
                npcObject.transform.position = targetPosition;
                Debug.Log("Posisi NPC dipindahkan ke : " + targetPosition);

                // Pastikan Anda mengakses script NPC yang benar (misal NPCBehavior)
                var npcScript = npcObject.GetComponent<NPCBehavior>();
                if (npcScript != null)
                {
                    npcScript.StartTeleportCooldown();
                }
            }
        }
    }

}
