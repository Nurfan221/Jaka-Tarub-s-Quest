using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class KnockbackSystem : MonoBehaviour
{
    [Header("Settings")]
    public float thrust = 10f; // Kekuatan dorongan
    public float knockbackDuration = 0.2f; // Berapa lama efeknya (sebelum bisa gerak lagi)

    [Header("References")]
    public Rigidbody2D rb;

    [Header("Events")]
    // Event ini berguna untuk memberi tahu script utama (AI/Player) untuk "MATI SURI" sebentar
    private System.Action onKnockbackFinished;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    public void PlayKnockback(Transform sender, System.Action onDone = null)
    {
        StopAllCoroutines();

        // Simpan fungsi titipan (callback) ke variable
        this.onKnockbackFinished = onDone;

        // Hitung Arah: (Posisi Saya - Posisi Penyerang) = Arah Mundur
        Vector2 direction = (transform.position - sender.position).normalized;

        //  Reset Velocity dulu agar dorongan konsisten (tidak meluncur licin)
        rb.linearVelocity = Vector2.zero;

        // Dorong! (ForceMode2D.Impulse = hentakan instan)
        rb.AddForce(direction * thrust, ForceMode2D.Impulse);

        //  Mulai timer pemulihan
        StartCoroutine(ResetKnockback());
    }

    private IEnumerator ResetKnockback()
    {
        yield return new WaitForSeconds(knockbackDuration);

        rb.linearVelocity = Vector2.zero;

        // PANGGIL FUNGSI TITIPAN TADI (Artinya: "Woi, udah selesai nih!")
        onKnockbackFinished?.Invoke();
    }
}