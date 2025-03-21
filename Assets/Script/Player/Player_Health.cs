using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static NPCBehavior;

public class Player_Health : MonoBehaviour
{
    public static Player_Health Instance; // Access this class from the Instace
    [SerializeField] Player_Anim player_Anim;

    [Header("HEALTH VALUE")]
    public int maxHealth = 100;
    public int health = 100;

    [Header("STAMINA VALUE")]
    public float maxStamina = 100;
    public float stamina = 100;
    public float staminaRegenRate = 15;

    public SpriteRenderer sr;

    private void Awake()
    {
        Instance = this;
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        PlayerUI.Instance.healthUI.fillAmount = (float)health / maxHealth;
        PlayerUI.Instance.staminaUI.fillAmount = stamina / maxStamina;

        if (stamina < maxStamina)
        {
            stamina = Mathf.MoveTowards(stamina, maxStamina, staminaRegenRate * Time.deltaTime);
        }
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        health -= damage;

        if (player_Anim != null)
        {
            player_Anim.PlayTakeDamageAnimation();
        }

        // Hitung arah knockback
        Vector2 knockbackDirection = ((Vector2)transform.position - attackerPosition).normalized;

        // Terapkan knockback dan hentikan kontrol pemain sementara
        StartCoroutine(ApplyKnockback(knockbackDirection));

        // Jika HP habis, mati
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
            rb.linearVelocity = Vector2.zero; // Reset velocity
            rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        }

        // Hentikan gerakan sementara
        Player_Movement movement = GetComponent<Player_Movement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        yield return new WaitForSeconds(0.3f); // Sesuaikan dengan durasi animasi take damage

        // Aktifkan kembali gerakan setelah knockback selesai
        if (movement != null)
        {
            movement.enabled = true;
        }
    }





    IEnumerator TakeDamageVisual()
    {
        float startTime = Time.time;
        while (Time.time < startTime + .5f)
        {
            sr.color = Color.Lerp(new(1, 0, 0), new(1, 1, 1), (Time.time - startTime) / .5f);
            yield return null;
        }
    }

    public void Heal(int heal)
    {
        health += heal;
    }

    public bool SpendStamina(float exhaust)
    {
        if (exhaust > stamina)
        {
            return false;
        }
        else
        {
            stamina -= exhaust;
            return true;
        }
    }

    [ContextMenu("KILL")]
    void Die()
    {
        print("Player Died");
        GameController.Instance.PlayerDied();
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.gameObject.layer == 7)
    //    {
    //        TakeDamage(3);
    //    }
    //}
}
