using UnityEngine;
using System.Collections;
public class Enemy_Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float health;
    public Enemy_Bandit enemy_Bandit;

    void Start()
    {
        health = maxHealth; // Set health ke nilai maksimum saat enemy muncul
    }

    public void TakeDamage(float damage, Transform attackerTransform)
    {
        enemy_Bandit.SetKnockbackStatus(true, attackerTransform);
        //enemy_Bandit.PlayAnimation("TakeDamage");
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth); // Pastikan tidak kurang dari 0 atau lebih dari maxHealth
        //enemy_Bandit.Run();
        if (health <= 0)
        {
            enemy_Bandit.Die();
        }
    }

   

}
