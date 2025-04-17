using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player_Health : MonoBehaviour
{
    public static Player_Health Instance;

    [SerializeField] Player_Anim player_Anim;

    [Header("HEALTH VALUE")]
    public int maxHealth = 100;
    public int health = 100;



    [Header("STAMINA VALUE")]
    public int maxStamina = 100;
    public float stamina = 100;
    public float staminaRegenRate = 15;
    public float currentMaxStamina;

    [Header("Emotional Cap System")]
    public int emotionalHealthCap = 100;   // Batas max health saat berduka
    public int emotionalStaminaCap = 100;  // Batas max stamina saat berduka
    public bool isInGrief = false;         // Apakah sedang berduka

    public SpriteRenderer sr;


    private void Start()
    {
        currentMaxStamina = maxStamina;
        
    }
    private void Awake()
    {
        Instance = this;
        health = maxHealth;
        stamina = currentMaxStamina;
        emotionalHealthCap = maxHealth;
        emotionalStaminaCap = maxStamina;
    }

    void Update()
    {
        int currentHealthCap = isInGrief ? emotionalHealthCap : maxHealth;
        int currentStaminaCap = isInGrief ? emotionalStaminaCap : maxStamina;

        // Clamp untuk pastikan current tidak melebihi cap
        health = Mathf.Clamp(health, 0, currentHealthCap);
        stamina = Mathf.Clamp(stamina, 0, currentStaminaCap);

        // Update UI
        PlayerUI.Instance.healthUI.fillAmount = (float)health / maxHealth;
        PlayerUI.Instance.staminaUI.fillAmount = (float)stamina / maxStamina;

        // Regen stamina
        float regenRate = isInGrief ? staminaRegenRate * 0.3f : staminaRegenRate;

        if (stamina < maxStamina)
        {
            stamina = Mathf.MoveTowards(stamina, currentMaxStamina, regenRate * Time.deltaTime);
        }
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        health -= damage;

        if (player_Anim != null)
            player_Anim.PlayTakeDamageAnimation();

        Vector2 knockbackDirection = ((Vector2)transform.position - attackerPosition).normalized;
        StartCoroutine(ApplyKnockback(knockbackDirection));

        if (health <= 0)
        {
            Die();
        }
    }

    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float knockbackForce = 1f;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
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

    public void Heal(int amount)
    {
        int currentHealthCap = isInGrief ? emotionalHealthCap : maxHealth;
        health = Mathf.Clamp(health + amount, 0, currentHealthCap);
    }

    public bool SpendStamina(float exhaust)
    {
        if (exhaust > stamina) return false;

        stamina -= exhaust;
        return true;
    }

    public bool SpendMaxCurrentStamina(float inUse)
    {
        if (inUse > currentMaxStamina) return false;

        currentMaxStamina -= inUse;
        stamina = currentMaxStamina;
        return true;
    }

    

    [ContextMenu("KILL")]
    void Die()
    {
        Debug.Log("Player Died");
        GameController.Instance.PlayerDied();
    }

    /// <summary>
    /// Digunakan saat hari baru dimulai, tapi disesuaikan dengan emotional cap jika masih berduka.
    /// </summary>
    public void ReverseHealthandStamina()
    {
        int currentHealthCap = isInGrief ? emotionalHealthCap : maxHealth;
        float currentStaminaCap = isInGrief ? emotionalStaminaCap : maxStamina;

        Debug.Log("health awal " + health);
        health = currentHealthCap;
        Debug.Log("health sekarang" + health);
        Debug.Log("Stamina awal " + stamina);
        stamina = currentStaminaCap;
        Debug.Log("Stamina saat ini " + stamina);
    }

    /// <summary>
    /// Quest healing untuk menaikkan batas emosional secara bertahap.
    /// </summary>
    public void IncreaseEmotionalCap(int amount)
    {
        if (isInGrief)
        {
            emotionalHealthCap = Mathf.Min(emotionalHealthCap + amount, maxHealth);
            emotionalStaminaCap = Mathf.Min(emotionalStaminaCap + amount, (int)maxStamina);

            if (emotionalHealthCap == maxHealth && emotionalStaminaCap == (int)maxStamina)
            {
                isInGrief = false;
                Debug.Log("Jaka telah pulih sepenuhnya dari kesedihannya.");
            }
        }
    }
}
