using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player_Health : MonoBehaviour
{
    public static Player_Health Instance;

    [SerializeField] Player_Anim player_Anim;
    [SerializeField] BuffScrollController buffScrollController;


    // Batas health/stamina saat ini akan kita hitung secara dinamis


    public SpriteRenderer sr;
    private PlayerData_SO stats;

    private void Awake()
    {
        //Instance = this;
        //PlayerController.Instance.health = PlayerController.Instance.maxHealth;
        //PlayerController.Instance.stamina = PlayerController.Instance.maxStamina;
        //UpdateCaps();
        //StartGrief();
        // Ambil "Papan Pengumuman" dari Otak dan simpan ke jalan pintas kita.
        if (PlayerController.Instance != null)
        {
            stats = PlayerController.Instance.playerData;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }
    }

    void Update()
    {
        UpdateCaps();

         stats.health = (int)Mathf.Clamp(stats.health, 0, stats.currentHealthCap);
         stats.stamina = Mathf.Clamp(stats.stamina, 0, stats.currentStaminaCap);

        PlayerUI.Instance.healthUI.fillAmount = (float)stats.health /  stats.maxHealth;
        PlayerUI.Instance.staminaUI.fillAmount =  stats.stamina / stats.maxStamina;

        float regenRate =  stats.isInGrief ? stats.staminaRegenRate * 0.7f : stats.staminaRegenRate;

        if (stats.stamina <  stats.currentStaminaCap)
        {
             stats.stamina += regenRate * Time.deltaTime;
        }
    }

    // --- FUNGSI DIPERBAIKI ---
    void UpdateCaps()
    {
        // Penalti untuk health HANYA berasal dari 'grief'.
        float healthPenalty = stats.isInGrief ?  stats.currentGriefPenalty : 0;
         stats.currentHealthCap = Mathf.RoundToInt(stats.maxHealth * (1f - healthPenalty / 100f));

        // Penalti untuk stamina bisa berasal dari 'grief' DAN 'fatigue'.
        float staminaPenalty = (stats.isInGrief ?  stats.currentGriefPenalty : 0) + stats.currentFatiguePenalty;
        stats.currentStaminaCap =  stats.maxStamina * (1f - staminaPenalty / 100f);

        // Pastikan cap tidak di bawah nilai minimum (misal 10) untuk mencegah bug.
         stats.currentHealthCap = Mathf.Max(stats.currentHealthCap, 10);
        stats.currentStaminaCap = Mathf.Max( stats.currentStaminaCap, 10);
    }

    // --- FUNGSI SISTEM BERTAHAP (GRIEF) ---

    [ContextMenu("Start Grief")]
    public void StartGrief()
    {
        stats.isInGrief = true;
        stats.currentGriefPenalty = stats.initialGriefPenalty;
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
        float recoveryPercentagePerQuest = stats.initialGriefPenalty / stats.totalHealingQuests;

        // Hitung berapa POIN HP dan Stamina yang akan ditambahkan sebagai reward
        int healthToRestore = Mathf.RoundToInt(stats.maxHealth * (recoveryPercentagePerQuest / 100f));
        float staminaToRestore = stats.maxStamina * (recoveryPercentagePerQuest / 100f);

        // Kurangi penalti untuk menaikkan batas maksimum (cap)
        stats.currentGriefPenalty -= recoveryPercentagePerQuest;
        stats.currentGriefPenalty = Mathf.Max(0, stats.currentGriefPenalty);

        UpdateCaps(); // Hitung ulang batas maksimum yang baru

        // Tambahkan reward ke health & stamina saat ini, lalu clamp ke batas baru
        stats.health = Mathf.Clamp(stats.health + healthToRestore, 0, stats.currentHealthCap);
        stats.stamina = Mathf.Clamp(stats.stamina + staminaToRestore, 0, stats.currentStaminaCap);

        Debug.Log($"Quest healed! Added {healthToRestore} HP. New HP: {stats.health}/{stats.currentHealthCap}");

        // Cek jika sudah pulih sepenuhnya
        if (stats.healingQuestsCompleted >= stats.totalHealingQuests)
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
        if (buffScrollController.isBuffProtection) damage -= buffScrollController.jumlahBuffProtection;
        damage = Mathf.Max(0, damage);
        stats.health -= damage;
        if (player_Anim != null) player_Anim.PlayTakeDamageAnimation();
        StartCoroutine(ApplyKnockback(attackerPosition));
        StartCoroutine(TakeDamageVisual());
        if (stats.health <= 0) Die();
    }

    public void Heal(int healthAmount, int staminaAmount)
    {
        stats.health = Mathf.Clamp(stats.health + healthAmount, 0, stats.currentHealthCap);
        stats.stamina = Mathf.Clamp(stats.stamina + staminaAmount, 0, stats.currentStaminaCap);
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

    /// <summary>
    /// BARU: Menerapkan efek kelelahan yang mengurangi batas maksimum stamina sementara.
    /// </summary>
    /// <param name="fatigueAmount">Jumlah persen kelelahan yang ditambahkan (misal: 5 untuk 5%).</param>
    public void ApplyFatigue(float fatigueAmount)
    {
        stats.currentFatiguePenalty += fatigueAmount;
        stats.currentFatiguePenalty = Mathf.Clamp(stats.currentFatiguePenalty, 0, stats.maxFatiguePenalty);
        UpdateCaps(); // Segera update batas maksimum stamina
        stats.stamina = Mathf.Min(stats.stamina, stats.currentStaminaCap); // Pastikan stamina saat ini tidak melebihi batas baru
        Debug.Log($"Player lelah! Penalti stamina sekarang: {stats.currentFatiguePenalty}%");
    }

    /// <summary>
    /// Mengembalikan HP/Stamina saat hari baru atau setelah tidur. Juga memulihkan kelelahan.
    /// </summary>
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
        GameController.Instance.PlayerDied();
    }
}
