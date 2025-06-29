using UnityEngine;
using System.Collections;

public class AttackTrigger : MonoBehaviour
{
    // Komponen yang didapatkan secara otomatis
    private Animator myAnimator;
    private SpriteRenderer mySpriteRenderer;
    private AnimalBehavior parentBehavior;

    // Variabel publik untuk di-set di Inspector
    public Sprite idleSprite; // Mengganti nama agar lebih jelas

    // Variabel status internal
    private bool isAttackSequenceRunning = false;

    void Awake()
    {
        // Dapatkan semua komponen yang dibutuhkan saat awal
        myAnimator = GetComponent<Animator>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        parentBehavior = GetComponentInParent<AnimalBehavior>();

        // Pengecekan keamanan
        if (myAnimator == null) Debug.LogError("AttackTrigger: Komponen Animator tidak ditemukan!");
        if (mySpriteRenderer == null) Debug.LogError("AttackTrigger: Komponen SpriteRenderer tidak ditemukan!");
        if (parentBehavior == null) Debug.LogError("AttackTrigger: Komponen AnimalBehavior di induk tidak ditemukan!");
    }

    void OnEnable()
    {
        // Selalu reset ke idle saat objek ini diaktifkan
        BackToIdle();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isAttackSequenceRunning) return;

        if (parentBehavior != null && parentBehavior.currentTarget != null && other.transform == parentBehavior.currentTarget)
        {
            StartCoroutine(AttackDamageSequence());
        }
    }

    // Fungsi ini sekarang tidak lagi dibutuhkan karena AttackSequence
    // akan menangani siklusnya sendiri hingga selesai.
    // Menghapusnya akan mencegah bug di mana animasi direset di tengah jalan.
    // private void OnTriggerExit2D(Collider2D other) { ... }


    // Mengganti nama agar lebih jelas tujuannya
    private void BackToIdle()
    {
        if (myAnimator == null || mySpriteRenderer == null) return;

        myAnimator.ResetTrigger("Attack");
        myAnimator.Play("Idle"); // Paksa kembali ke state Idle
        mySpriteRenderer.sprite = idleSprite;
    }


    private IEnumerator AttackDamageSequence()
    {
        isAttackSequenceRunning = true;
        Debug.Log("Memulai AttackSequence...");

        myAnimator.SetTrigger("Attack");
        Debug.Log("Animator Trigger 'Attack' dipicu.");


        //    Ganti 0.4f dengan waktu 'impact' dari animasi Anda.
        float impactDelay = 0.4f;
        yield return new WaitForSeconds(impactDelay);


        //    Kita cek sekali lagi apakah target masih ada untuk menghindari error
        if (parentBehavior != null && parentBehavior.currentTarget != null)
        {
            Debug.Log("Waktu impact tercapai! Menjalankan logika damage.");
            parentBehavior.JalankanLogikaSerangan();
        }


        float sisaDurasiAnimasi = 1.0f - impactDelay;
        yield return new WaitForSeconds(sisaDurasiAnimasi);

        Debug.Log("Animasi Attack selesai sepenuhnya.");
        BackToIdle();
        isAttackSequenceRunning = false;
        Debug.Log("AttackSequence selesai.");
    }
}