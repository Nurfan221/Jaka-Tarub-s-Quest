using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Health : MonoBehaviour
{
    public static Player_Health Instance; // Access this class from the Instace

    [Header("HEALTH VALUE")]
    [SerializeField] int maxHealth = 100;
    [SerializeField] int health = 100;

    [Header("STAMINA VALUE")]
    [SerializeField] float maxStamina = 100;
    public float stamina = 100;
    [SerializeField] float staminaRegenRate = 15;

    [SerializeField] SpriteRenderer sr;

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

    public void TakeDamage(int damage)
    {
        health -= damage;

        StartCoroutine(TakeDamageVisual());
        if (health <= 0)
            Die();
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 7)
        {
            TakeDamage(3);
        }
    }
}
