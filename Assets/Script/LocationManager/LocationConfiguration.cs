using UnityEngine;
using System.Collections;

public class LocationConfiguration : MonoBehaviour
{
    [Header("Daftar Hubungan")]
    [SerializeField] Player_Health player_Health;
    [SerializeField] QuestManager questManager;
    public Lokasi lokasiSaatIni = Lokasi.None; // Assign default ke Danau
    public bool mainQuestDanau;
    public bool inDanau;
    public bool inRumahJaka;
    private Coroutine healingCoroutine;
    public float delayHealing = 2f; // Delay tiap heal dalam detik



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (inDanau && healingCoroutine == null)
        {
            healingCoroutine = StartCoroutine(AreaHealingCoroutine());
        }

        // Kalau player keluar area
        if (!inDanau && healingCoroutine != null)
        {
            StopCoroutine(healingCoroutine);
            healingCoroutine = null;
        }
    }


    private IEnumerator AreaHealingCoroutine()
    {
        while (inDanau)
        {
            if (player_Health.health < player_Health.maxHealth) // Cek biar nggak over-heal
            {
                player_Health.health += 3;
                Debug.Log("Healing... HP sekarang: " + player_Health.health);
            }

            yield return new WaitForSeconds(delayHealing); // Tunggu sebelum heal lagi
        }


    }




}
