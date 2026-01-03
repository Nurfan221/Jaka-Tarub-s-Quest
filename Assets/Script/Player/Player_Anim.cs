using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Anim : MonoBehaviour
{
    [Header("Master Animator (Badan Utama)")]
    public Animator bodyAnimator;

    [Header("Assign semua bagian tubuh (Body, Armor, dll) ke sini")]
    public List<Animator> layerAnimators = new List<Animator>();


    [Header("Sprite Renderers")]
    public SpriteRenderer bodySR;
    public SpriteRenderer pantsSR;
    public SpriteRenderer clothSR; // Armor/Baju
    public SpriteRenderer hairSR;
    public SpriteRenderer shoesSR;

    Player_Movement pm;

    public bool isAttacking;
    public bool isTakingDamage = false;
    public Vector2 lastDirection = Vector2.down;

    private void Start()
    {
        pm = GetComponent<Player_Movement>();
    }

    void Update()
    {
        UpdateAnimationParameters();
    }

    void LateUpdate()
    {
        SyncVisuals();
        UpdateLayerSorting();
    }

    public void UpdateAnimationParameters()
    {
        if (bodyAnimator == null) return;

        if (isTakingDamage) return;
        if (pm.ifDisturbed)
        {
            // Set parameter Master ke Idle (Speed 0)
            SetAnimParameters(bodyAnimator, lastDirection.x, lastDirection.y, 0f);

            // Wajib Sync ke Slave (Baju, Celana, dll) juga!
            foreach (Animator anim in layerAnimators)
            {
                anim.SetFloat("Speed", 0f);
                anim.SetFloat("MoveX", bodyAnimator.GetFloat("MoveX"));
                anim.SetFloat("MoveY", bodyAnimator.GetFloat("MoveY"));
                anim.SetFloat("IdleX", bodyAnimator.GetFloat("IdleX"));
                anim.SetFloat("IdleY", bodyAnimator.GetFloat("IdleY"));
            }

            // Baru setelah parameternya di-nol-kan, kita return agar tidak baca input gerakan
            return;
        }

        Vector2 movement = pm.movementDirection.normalized;
        bool isMoving = movement != Vector2.zero;

        // Atur Parameter untuk Body 
        if (isMoving)
        {
            lastDirection = movement;
            SetAnimParameters(bodyAnimator, movement.x, movement.y, 1f);
        }
        else
        {
            SetAnimParameters(bodyAnimator, lastDirection.x, lastDirection.y, 0f);
        }

        // Atur Parameter untuk Layer Lain (Slave) agar state machine-nya merespons
        foreach (Animator anim in layerAnimators)
        {
            // Kita copy parameter yang sama persis ke baju/celana
            anim.SetFloat("MoveX", bodyAnimator.GetFloat("MoveX"));
            anim.SetFloat("MoveY", bodyAnimator.GetFloat("MoveY"));
            anim.SetFloat("IdleX", bodyAnimator.GetFloat("IdleX"));
            anim.SetFloat("IdleY", bodyAnimator.GetFloat("IdleY"));
            anim.SetFloat("Speed", bodyAnimator.GetFloat("Speed"));

            // Trigger attack/damage juga harus di-pass jika ada logic-nya di sini
            // (Biasanya trigger dilakukan via method terpisah di bawah)
        }
    }

    // Helper function biar kodenya rapi
    void SetAnimParameters(Animator anim, float x, float y, float speed)
    {
        anim.SetFloat("MoveX", Mathf.Round(x));
        anim.SetFloat("MoveY", Mathf.Round(y));
        anim.SetFloat("IdleX", Mathf.Round(lastDirection.x)); // Pastikan idle direction juga terupdate
        anim.SetFloat("IdleY", Mathf.Round(lastDirection.y));
        anim.SetFloat("Speed", speed);
    }

    void SyncVisuals()
    {
        if (bodyAnimator == null || layerAnimators.Count == 0) return;

        // Ambil info state animasi dari Badan (Master) saat ini
        AnimatorStateInfo masterState = bodyAnimator.GetCurrentAnimatorStateInfo(0);
        int currentHash = masterState.fullPathHash;
        float currentTime = masterState.normalizedTime; // Waktu putar (0.0 sampai 1.0)

        foreach (Animator anim in layerAnimators)
        {
            AnimatorStateInfo slaveState = anim.GetCurrentAnimatorStateInfo(0);

            // Kita paksa baju untuk 'Play' state yang sama di waktu yang sama persis.
            if (slaveState.fullPathHash != currentHash || Mathf.Abs(slaveState.normalizedTime - currentTime) > 0.02f)
            {
                anim.Play(currentHash, 0, currentTime);
            }
        }
    }

    // Fungsi Trigger (Serang/Hit)
    public void PlayTriggerAnimation(string triggerName)
    {
        if (bodyAnimator == null) return;

        // Trigger Master
        bodyAnimator.SetTrigger(triggerName);

        // Trigger Slaves
        foreach (Animator anim in layerAnimators)
        {
            anim.SetTrigger(triggerName);
        }

        // Logic reset state bisa disesuaikan lagi jika perlu
    }

    public void PlayAnimation(string nameAnimation)
    {
        if (bodyAnimator == null) return;

        isTakingDamage = true;

        // Trigger Master (Badan)
        bodyAnimator.SetTrigger(nameAnimation);

        // Trigger Slaves (Baju, dll)
        foreach (Animator anim in layerAnimators)
        {
            if (anim != null) anim.SetTrigger(nameAnimation);
        }

        StartCoroutine(ResetTakeDamageState());
    }

    private IEnumerator ResetTakeDamageState()
    {
        // Tunggu 1 frame agar Animator sempat transisi ke state baru
        // Jika tidak, kita mungkin mengambil durasi animasi 'Idle' bukannya 'Hurt'
        yield return null;

        // Gunakan bodyAnimator sebagai satu-satunya acuan waktu
        float duration = bodyAnimator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(duration);

        isTakingDamage = false;

        // Panggil fungsi update parameter agar kembali ke Idle/Run yang benar
        UpdateAnimationParameters();
    }

    private IEnumerator ResetAttackState()
    {
        // Tunggu 1 frame agar state berpindah
        yield return null;

        // Ambil durasi dari Master
        float duration = bodyAnimator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(duration);

        isAttacking = false;

        UpdateAnimationParameters();
    }

    // Fungsi Play & Wait (Hard Sync Play)
    public IEnumerator PlayAndWaitForAnimation(string stateName, int layer = 0)
    {
        if (bodyAnimator == null) yield break;

        // Play di Master
        bodyAnimator.Play(stateName, layer, 0f);

        // Play di semua Slave (Force waktu ke 0 agar sinkron)
        foreach (Animator anim in layerAnimators)
        {
            if (anim != null) anim.Play(stateName, layer, 0f);
        }

        yield return null; // Tunggu frame update

        // Kita tidak perlu mengecek layerAnimators karena mereka pasti durasinya sama
        while (bodyAnimator.GetCurrentAnimatorStateInfo(layer).IsName(stateName) &&
               bodyAnimator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1.0f)
        {
            // Opsional: Paksa sinkronisasi terus menerus jika animasi sangat panjang
            // SyncVisuals(); 

            yield return null;
        }

        Debug.Log($"Animasi '{stateName}' telah selesai.");

        // Kembalikan kontrol ke update normal
        UpdateAnimationParameters();
    }

    void UpdateLayerSorting()
    {
        // Cek arah terakhir (lastDirection)
        // Jika Y lebih besar dari 0, berarti menghadap ATAS (Belakang)
        if (lastDirection.y > 0.1f)
        {



            bodySR.sortingOrder = 10;   // Dasar
            pantsSR.sortingOrder = 11;  // Celana
            clothSR.sortingOrder = 11;  // Baju di atas celana
            hairSR.sortingOrder = 13;   // Rambut paling atas (menutupi punggung baju)
            shoesSR.sortingOrder = 12;  // Sepatu paling atas



        }
        else
        {


            bodySR.sortingOrder = 6;
            pantsSR.sortingOrder = 7;
            clothSR.sortingOrder = 7;
            hairSR.sortingOrder = 7;
            shoesSR.sortingOrder = 8;
        }
    }
}