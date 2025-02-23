using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{

    public Enemy_Health enemyHealth; // Referensi ke skrip Enemy_Health
    public Transform barFill; // Referensi ke BarFill
    private Vector3 originalScale; // Ukuran awal BarFill

    void Start()
    {
        if (barFill != null)
            originalScale = barFill.localScale; // Simpan ukuran awal
    }

    void Update()
    {
        if (enemyHealth != null && barFill != null)
        {
            // Hitung persentase health
            float healthPercent = (float)enemyHealth.health / enemyHealth.maxHealth;
            // Ubah scale berdasarkan health
            barFill.localScale = new Vector3(originalScale.x * healthPercent, originalScale.y, originalScale.z);
        }
    }
}
