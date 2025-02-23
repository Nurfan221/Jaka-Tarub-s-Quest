using UnityEngine;

public class Enemy_Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float health;
    [SerializeField] Enemy_Bandit enemy_Bandit;

    void Start()
    {
        health = maxHealth; // Set health ke nilai maksimum saat enemy muncul
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth); // Pastikan tidak kurang dari 0 atau lebih dari maxHealth

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        enemy_Bandit.DropItem();
        Debug.Log(gameObject.name + " has died.");
        Destroy(gameObject);
    }
}
