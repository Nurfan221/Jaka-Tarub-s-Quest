using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Enemy_Health : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField] SpriteRenderer sr;
    [HideInInspector] public Transform player;

    public Enemy_Spawner theSpawner;

    [Header("KNOCKBACK VALUE")]
    [SerializeField] float knockbackDistance = 2;
    [SerializeField] float knockbackForce = 2;
    [SerializeField] float knockbackCD = 2;
    bool justKnockbacked = false;
    float knockbackTimer;

    [Header("HEALTH VALUE")]
    [SerializeField] int maxHealth = 100;
    [SerializeField] int health = 100;
    public bool justGotHit;
    public bool CanDrop = false;
    [SerializeField] string ItemDrop;
    [SerializeField] float DropChance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);

        // Making sure only knockbacked a certain times
        if (justKnockbacked)
        {
            knockbackTimer += Time.deltaTime;
            if (knockbackTimer > knockbackCD)
            {
                knockbackTimer = 0;
                justKnockbacked = false;
            }
        }
    }


    public void TakeDamage(int damage)
    {
        health -= damage;
        justGotHit = true;

        StartCoroutine(TakeDamageVisual());

        if (!justKnockbacked)
            StartCoroutine(Knockback());

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

    // Knockbacked until set distance
    IEnumerator Knockback()
    {
        justKnockbacked = true;
        Vector2 startPos = transform.position;
        Vector2 knockbackDir = transform.position - player.position;

        while (Vector2.Distance(startPos, transform.position) < knockbackDistance)
        {
            rb.AddForce(knockbackForce * 100f * Time.deltaTime * knockbackDir, ForceMode2D.Impulse);
            yield return null;
        }
        rb.velocity = Vector2.zero;

    }

    [ContextMenu("KILL")]
    void Die()
    {
        print("Enemy Died");
        if (theSpawner != null)
        {
            theSpawner.enemies.Remove(gameObject);
            theSpawner.enemies.TrimExcess();
        }
        GameEconomy.Instance.GainMoney(10);
        Destroy(gameObject);
        if (CanDrop)
        {
            if (Random.Range(0, 100) <= DropChance)
                ItemPool.Instance.DropItem(ItemDrop, transform.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Take damage if collided with enemyHitArea layer
        if (collision.gameObject.layer == 6)
        {
            TakeDamage(10);
        }
    }
}
