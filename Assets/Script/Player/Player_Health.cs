using System;
using System.Collections;
using UnityEngine;

public class Player_Health : MonoBehaviour
{


    public static event Action Sekarat;

    // Batas health/stamina saat ini akan kita hitung secara dinamis


    public SpriteRenderer sr;
    private PlayerController stats;
    private PlayerData_SO playerData;
    private bool isDead = false;

    private void Awake()
    {

        // Ambil "Papan Pengumuman" dari Otak dan simpan ke jalan pintas kita.
        if (PlayerController.Instance != null)
        {
            stats = PlayerController.Instance;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }

        if (PlayerController.Instance != null)
        {
            playerData = PlayerController.Instance.playerData;
        }
        else
        {
            Debug.LogError("PlayerData_SO.Instance tidak ditemukan saat Awake!");
        }
    }
    private void Start()
    {
        UpdateCaps();
    }

    void Update()
    {


        stats.health = (int)Mathf.Clamp(stats.health, 0, stats.currentHealthCap);
        stats.stamina = Mathf.Clamp(stats.stamina, 0, stats.currentStaminaCap);

        PlayerUI.Instance.healthUI.fillAmount = (float)stats.health / playerData.maxHealth;
        PlayerUI.Instance.staminaUI.fillAmount = stats.stamina / playerData.maxStamina;

        float regenRate = stats.isInGrief ? playerData.staminaRegenRate * 0.7f : playerData.staminaRegenRate;

        if (stats.stamina < stats.currentStaminaCap)
        {
            stats.stamina += regenRate * Time.deltaTime;
            PlayerUI.Instance.UpdateStaminaDisplay(stats.stamina, playerData.maxStamina);
        }
    }


    void UpdateCaps()
    {
        // Penalti untuk health HANYA berasal dari 'grief'.
        float healthPenalty = stats.isInGrief ? stats.currentGriefPenalty : 0;
        stats.currentHealthCap = Mathf.RoundToInt(playerData.maxHealth * (1f - healthPenalty / 100f));

        // Penalti untuk stamina bisa berasal dari 'grief' DAN 'fatigue'.
        float staminaPenalty = (stats.isInGrief ? stats.currentGriefPenalty : 0) + stats.currentFatiguePenalty;
        stats.currentStaminaCap = playerData.maxStamina * (1f - staminaPenalty / 100f);

        // Pastikan cap tidak di bawah nilai minimum (misal 10) untuk mencegah bug.
        stats.currentHealthCap = Mathf.Max(stats.currentHealthCap, 10);
        stats.currentStaminaCap = Mathf.Max(stats.currentStaminaCap, 10);
    }



    [ContextMenu("Start Grief")]
    public void StartGrief()
    {
        stats.isInGrief = true;
        stats.currentGriefPenalty = playerData.initialGriefPenalty;
        stats.healingQuestsCompleted = 0;
        UpdateCaps();
        Debug.Log($"Jaka mulai berduka. Batas HP/Stamina turun sebesar {stats.currentGriefPenalty}%.");
    }

    [ContextMenu("Heal Grief Step")]
    public void HealGriefStep()
    {
        if (!stats.isInGrief) return;
        stats.healingQuestsCompleted++;

        // Hitung berapa persen pemulihan untuk satu quest ini
        float recoveryPercentagePerQuest = playerData.initialGriefPenalty / playerData.totalHealingQuests;

        // Hitung berapa POIN HP dan Stamina yang akan ditambahkan sebagai reward
        int healthToRestore = Mathf.RoundToInt(playerData.maxHealth * (recoveryPercentagePerQuest / 100f));
        float staminaToRestore = playerData.maxStamina * (recoveryPercentagePerQuest / 100f);

        // Kurangi penalti untuk menaikkan batas maksimum (cap)
        stats.currentGriefPenalty -= recoveryPercentagePerQuest;
        stats.currentGriefPenalty = Mathf.Max(0, stats.currentGriefPenalty);

        UpdateCaps(); // Hitung ulang batas maksimum yang baru

        // Tambahkan reward ke health & stamina saat ini, lalu clamp ke batas baru
        stats.health = Mathf.Clamp(stats.health + healthToRestore, 0, stats.currentHealthCap);
        stats.stamina = Mathf.Clamp(stats.stamina + staminaToRestore, 0, stats.currentStaminaCap);

        Debug.Log($"Quest healed! Added {healthToRestore} HP. New HP: {stats.health}/{stats.currentHealthCap}");

        // Cek jika sudah pulih sepenuhnya
        if (stats.healingQuestsCompleted >= playerData.totalHealingQuests)
        {
            stats.isInGrief = false;
            stats.currentGriefPenalty = 0;
            UpdateCaps(); // Final update caps to 100%
            // Pada quest terakhir, isi penuh HP dan Stamina sebagai tanda pemulihan total
            stats.health = stats.currentHealthCap;
            stats.stamina = stats.currentStaminaCap;
            Debug.Log("Jaka telah pulih sepenuhnya dari kesedihannya.");
        }
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        // Jika pemain sudah ditandai mati (atau sedang proses mati), tolak damage baru.
        // Ini mencegah fungsi Die() dipanggil berulang kali oleh musuh yang menyerang bersamaan.
        if (isDead) return;

        //  Logika Hitung Damage 
        if (BuffScrollController.Instance.isBuffProtection)
            damage -= BuffScrollController.Instance.jumlahBuffProtection;

        damage = Mathf.Max(0, damage);
        stats.health -= damage;

        // Update UI
        PlayerUI.Instance.UpdateHealthDisplay(stats.health, playerData.maxHealth);

        // Animasi & Visual (Tetap jalankan ini meskipun akan mati, agar terlihat impact-nya)
        PlayerController.Instance.HandlePlayAnimation("TakeDamage");
        StartCoroutine(ApplyKnockback(attackerPosition));
        StartCoroutine(TakeDamageVisual());

        // CEK KEMATIAN:
        if (stats.health <= 0)
        {
            // Serangan musuh ke-2 yang masuk setelah baris ini akan ditolak oleh "if (isDead) return;" di atas.
            isDead = true;

            // Panggil proses kematian dengan jeda
            StartCoroutine(ProcessDeathSequence());
        }
    }

    private IEnumerator ProcessDeathSequence()
    {
        // Berikan jeda sedikit (misal 0.1 atau 0.2 detik)
        // Ini memberi waktu agar animasi TakeDamage atau Knockback sempat terlihat sedikit
        // dan memastikan semua logika frame saat ini selesai sebelum objek dimatikan.
        yield return new WaitForSeconds(0.1f);

        // Jalankan logika kematian yang sebenarnya
        Die();
    }

    public void Heal(int healthAmount, int staminaAmount)
    {
        // Simpan nilai lama untuk perbandingan
        int oldHealth = stats.health;
        float oldStamina = stats.currentStaminaCap;

        // Lakukan proses penyembuhan
        stats.health = Mathf.Clamp(stats.health + healthAmount, 0, stats.currentHealthCap);
        stats.currentStaminaCap = Mathf.Clamp(stats.currentStaminaCap + staminaAmount, 0, playerData.maxStamina);



    }


    // Mengurangi stamina pemain (untuk aksi biasa). Mengembalikan true jika berhasil.
    public bool SpendStamina(float amount)
    {
        if (stats.stamina >= amount)
        {
            stats.stamina -= amount;
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool DrainStamina(float drainPerSecond)
    {
        if (stats.currentStaminaCap > 0)
        {
            // Kurangi BATAS STAMINA berdasarkan waktu.
            stats.currentStaminaCap -= drainPerSecond;
            PlayerUI.Instance.UpdateStaminaDisplay(stats.currentStaminaCap, playerData.maxStamina);


            //    Gunakan 'stats.currentStaminaCap' lagi di dalam Mathf.Max.
            stats.currentStaminaCap = Mathf.Max(stats.currentStaminaCap, 0);

            // Setelah batasnya turun, pastikan stamina saat ini tidak melebihi batas baru tersebut.
            stats.stamina = Mathf.Min(stats.stamina, stats.currentStaminaCap);

            Debug.Log($"Stamina cap dikuras sebanyak : " + drainPerSecond);
            Debug.Log($"Stamina cap dikuras. Batas baru: {stats.currentStaminaCap}");


            return true;
        }
        else
        {
            Debug.Log("Batas stamina sudah habis, tidak bisa dikuras lagi.");
            return false;
        }
        //Debug.Log("mengurangi stamina sejumlah : " + drainPerSecond);
    }

    public void ApplyFatigue(float fatigueAmount)
    {
        stats.currentFatiguePenalty += fatigueAmount;
        stats.currentFatiguePenalty = Mathf.Clamp(stats.currentFatiguePenalty, 0, playerData.maxFatiguePenalty);
        UpdateCaps(); // Segera update batas maksimum stamina
        stats.stamina = Mathf.Min(stats.stamina, stats.currentStaminaCap); // Pastikan stamina saat ini tidak melebihi batas baru
        Debug.Log($"Player lelah! Penalti stamina sekarang: {stats.currentFatiguePenalty}%");
    }


    public void ReverseHealthandStamina()
    {
        // Kelelahan pulih sepenuhnya setelah tidur
        stats.currentFatiguePenalty = 0;

        // Panggil UpdateCaps SETELAH mereset penalti, SEBELUM mengisi ulang stamina
        UpdateCaps();

        stats.health = stats.currentHealthCap;
        stats.stamina = stats.currentStaminaCap; // Isi stamina sampai batas yang sudah pulih
    }

    private IEnumerator ApplyKnockback(Vector2 attackerPosition)
    {
        Vector2 knockbackDirection = ((Vector2)transform.position - attackerPosition).normalized;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float knockbackForce = 1f;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
        Player_Movement movement = GetComponent<Player_Movement>();
        if (movement != null) movement.enabled = false;
        yield return new WaitForSeconds(0.3f);
        if (movement != null) movement.enabled = true;
    }

    IEnumerator TakeDamageVisual()
    {
        float startTime = Time.time;
        while (Time.time < startTime + 0.5f)
        {
            sr.color = Color.Lerp(new Color(1, 0, 0), Color.white, (Time.time - startTime) / 0.5f);
            yield return null;
        }
    }

    [ContextMenu("KILL")]
    void Die()
    {
        Debug.Log("Player Died");
        //GameController.Instance.PlayerDied();
        GameController.Instance.StartPassOutSequence(false);
        isDead = false; // Reset flag kematian untuk memungkinkan respawn
    }

    public void CheckSekarat()
    {
        Debug.Log("Mengecek kondisi sekarat..."); // Tambahkan ini untuk tahu fungsi dipanggil

        // Pastikan perbandingannya benar. Gunakan (float) untuk pembagian yang akurat.
        if ((float)stats.health / playerData.maxHealth <= 0.3f)
        {
            // Tambahkan ini untuk tahu kondisi terpenuhi
            Debug.Log("KONDISI SEKARAT TERPENUHI! MENYIARKAN EVENT...");
            Sekarat?.Invoke();
        }
    }

    public void PlayerPingsan()
    {
        int healthPenalty = Mathf.CeilToInt(stats.health * 0.8f);
        int staminaPenalty = Mathf.FloorToInt(stats.currentStaminaCap * 0.8f);
        Debug.Log("PlayerPingsan dipanggil. nilai health di kurangi menjadi : " + healthPenalty + "nilai stamina di kurangi menjadi " + staminaPenalty);

        stats.currentStaminaCap -= staminaPenalty;
        stats.health -= healthPenalty;
    }




}
