using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player_Health : MonoBehaviour
{
    public static Player_Health Instance;

    [SerializeField] Player_Anim player_Anim;
    [SerializeField] BuffScrollController buffScrollController;

    [Header("HEALTH VALUE")]
    public int maxHealth = 100;
    public int health = 100;

    [Header("STAMINA VALUE")]
    public int maxStamina = 100;
    public float stamina = 100;
    public float staminaRegenRate = 15;

    [Header("Emotional Cap System")]
    [Range(0, 100)]
    public float initialGriefPenalty = 30f;
    public bool isInGrief = false;
    private float currentGriefPenalty;
    private int healingQuestsCompleted = 0;
    public int totalHealingQuests = 5;

    // BARU: Sistem Kelelahan (Fatigue)
    [Header("Fatigue System")]
    [Range(0, 100)]
    public float currentFatiguePenalty = 0f; // Penalti stamina dari kelelahan (dalam persen)
    [Tooltip("Batas maksimum penalti kelelahan yang bisa diakumulasi.")]
    public float maxFatiguePenalty = 50f; // Contoh: maks 50% dari stamina bisa hilang karena lelah

    // Batas health/stamina saat ini akan kita hitung secara dinamis
    private int currentHealthCap;
    private float currentStaminaCap;

    public SpriteRenderer sr;

    private void Awake()
    {
        Instance = this;
        health = maxHealth;
        stamina = maxStamina;
        UpdateCaps();
        StartGrief();
    }

    void Update()
    {
        UpdateCaps();

        health = (int)Mathf.Clamp(health, 0, currentHealthCap);
        stamina = Mathf.Clamp(stamina, 0, currentStaminaCap);

        PlayerUI.Instance.healthUI.fillAmount = (float)health / maxHealth;
        PlayerUI.Instance.staminaUI.fillAmount = stamina / maxStamina;

        float regenRate = isInGrief ? staminaRegenRate * 0.7f : staminaRegenRate;

        if (stamina < currentStaminaCap)
        {
            stamina += regenRate * Time.deltaTime;
        }
    }

    // --- FUNGSI DIPERBAIKI ---
    void UpdateCaps()
    {
        // Penalti untuk health HANYA berasal dari 'grief'.
        float healthPenalty = isInGrief ? currentGriefPenalty : 0;
        currentHealthCap = Mathf.RoundToInt(maxHealth * (1f - healthPenalty / 100f));

        // Penalti untuk stamina bisa berasal dari 'grief' DAN 'fatigue'.
        float staminaPenalty = (isInGrief ? currentGriefPenalty : 0) + currentFatiguePenalty;
        currentStaminaCap = maxStamina * (1f - staminaPenalty / 100f);

        // Pastikan cap tidak di bawah nilai minimum (misal 10) untuk mencegah bug.
        currentHealthCap = Mathf.Max(currentHealthCap, 10);
        currentStaminaCap = Mathf.Max(currentStaminaCap, 10);
    }

    // --- FUNGSI SISTEM BERTAHAP (GRIEF) ---

    [ContextMenu("Start Grief")]
    public void StartGrief()
    {
        isInGrief = true;
        currentGriefPenalty = initialGriefPenalty;
        healingQuestsCompleted = 0;
        UpdateCaps();
        Debug.Log($"Jaka mulai berduka. Batas HP/Stamina turun sebesar {currentGriefPenalty}%.");
    }

    [ContextMenu("Heal Grief Step")]
    public void HealGriefStep()
    {
        if (!isInGrief) return;
        healingQuestsCompleted++;

        // Hitung berapa persen pemulihan untuk satu quest ini
        float recoveryPercentagePerQuest = initialGriefPenalty / totalHealingQuests;

        // Hitung berapa POIN HP dan Stamina yang akan ditambahkan sebagai reward
        int healthToRestore = Mathf.RoundToInt(maxHealth * (recoveryPercentagePerQuest / 100f));
        float staminaToRestore = maxStamina * (recoveryPercentagePerQuest / 100f);

        // Kurangi penalti untuk menaikkan batas maksimum (cap)
        currentGriefPenalty -= recoveryPercentagePerQuest;
        currentGriefPenalty = Mathf.Max(0, currentGriefPenalty);

        UpdateCaps(); // Hitung ulang batas maksimum yang baru

        // Tambahkan reward ke health & stamina saat ini, lalu clamp ke batas baru
        health = Mathf.Clamp(health + healthToRestore, 0, currentHealthCap);
        stamina = Mathf.Clamp(stamina + staminaToRestore, 0, currentStaminaCap);

        Debug.Log($"Quest healed! Added {healthToRestore} HP. New HP: {health}/{currentHealthCap}");

        // Cek jika sudah pulih sepenuhnya
        if (healingQuestsCompleted >= totalHealingQuests)
        {
            isInGrief = false;
            currentGriefPenalty = 0;
            UpdateCaps(); // Final update caps to 100%
            // Pada quest terakhir, isi penuh HP dan Stamina sebagai tanda pemulihan total
            health = currentHealthCap;
            stamina = currentStaminaCap;
            Debug.Log("Jaka telah pulih sepenuhnya dari kesedihannya.");
        }
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        if (buffScrollController.isBuffProtection) damage -= buffScrollController.jumlahBuffProtection;
        damage = Mathf.Max(0, damage);
        health -= damage;
        if (player_Anim != null) player_Anim.PlayTakeDamageAnimation();
        StartCoroutine(ApplyKnockback(attackerPosition));
        StartCoroutine(TakeDamageVisual());
        if (health <= 0) Die();
    }

    public void Heal(int healthAmount, int staminaAmount)
    {
        health = Mathf.Clamp(health + healthAmount, 0, currentHealthCap);
        stamina = Mathf.Clamp(stamina + staminaAmount, 0, currentStaminaCap);
    }

    /// <summary>
    /// Mengurangi stamina pemain (untuk aksi biasa). Mengembalikan true jika berhasil.
    /// </summary>
    public bool SpendStamina(float amount)
    {
        if (stamina >= amount)
        {
            stamina -= amount;
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
        currentFatiguePenalty += fatigueAmount;
        currentFatiguePenalty = Mathf.Clamp(currentFatiguePenalty, 0, maxFatiguePenalty);
        UpdateCaps(); // Segera update batas maksimum stamina
        stamina = Mathf.Min(stamina, currentStaminaCap); // Pastikan stamina saat ini tidak melebihi batas baru
        Debug.Log($"Player lelah! Penalti stamina sekarang: {currentFatiguePenalty}%");
    }

    /// <summary>
    /// Mengembalikan HP/Stamina saat hari baru atau setelah tidur. Juga memulihkan kelelahan.
    /// </summary>
    public void ReverseHealthandStamina()
    {
        // Kelelahan pulih sepenuhnya setelah tidur
        currentFatiguePenalty = 0;

        // Panggil UpdateCaps SETELAH mereset penalti, SEBELUM mengisi ulang stamina
        UpdateCaps();

        health = currentHealthCap;
        stamina = currentStaminaCap; // Isi stamina sampai batas yang sudah pulih
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
