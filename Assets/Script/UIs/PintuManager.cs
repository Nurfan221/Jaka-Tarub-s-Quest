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
      

        StartCoroutine(LoadingScreenUI.Instance.SetLoadingandTimer(false));

        yield return new WaitForSeconds(1.0f);



        foreach (var pintuTujuan in databaseManager.listPintu)
        {
            if (pintuTujuan.idPintu == idPintu)
            {
                if (isPintuIn) // MASUK RUMAH
                {
                    ClockManager.Instance.isIndoors = true;
                    player.transform.position = pintuTujuan.pintuOut;
                    SmoothCameraFollow.Instance.EnterHouse(true);
                }
                else // KELUAR RUMAH
                {
                    ClockManager.Instance.isIndoors = false;
                    player.transform.position = pintuTujuan.pintuIn;
                    SmoothCameraFollow.Instance.EnterHouse(false);
                }
                SoundManager.Instance.CheckGameplayMusic(ClockManager.Instance.isIndoors, 0.1f);

                // Snap Kamera agar tidak terlihat "terbang" ke posisi baru
                SmoothCameraFollow.Instance.SnapToTarget();

                // Update Waktu/Cahaya
                ClockManager.Instance.UpdateDateTime();

             
            }
        }

        // Tunggu sebentar lagi untuk memastikan semua logic selesai (0.1 detik)
        yield return new WaitForSeconds(0.1f);


      
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
