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

        //  Jika sedang tergebug, JANGAN update apa-apa.
        // Biarkan animasi tergebug main sampai selesai.
        if (isTakingDamage) return;

        // Ini terjadi jika durasi Knockback lebih lama dari durasi animasi sakit.
        if (pm.ifDisturbed)
        {
            SetAnimParameters(bodyAnimator, lastDirection.x, lastDirection.y, 0f);
            foreach (Animator anim in layerAnimators)
            {
                anim.SetFloat("Speed", 0f);
                anim.SetFloat("MoveX", bodyAnimator.GetFloat("MoveX"));
                anim.SetFloat("MoveY", bodyAnimator.GetFloat("MoveY"));
                anim.SetFloat("IdleX", bodyAnimator.GetFloat("IdleX"));
                anim.SetFloat("IdleY", bodyAnimator.GetFloat("IdleY"));
            }
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

            
        }
    }

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

        if (isTakingDamage || isAttacking) return;

        AnimatorStateInfo masterState = bodyAnimator.GetCurrentAnimatorStateInfo(0);
        int currentHash = masterState.fullPathHash;
        float currentTime = masterState.normalizedTime;

        foreach (Animator anim in layerAnimators)
        {
            AnimatorStateInfo slaveState = anim.GetCurrentAnimatorStateInfo(0);

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
        if (isTakingDamage && nameAnimation != "Die") return;
        if (bodyAnimator == null) return;

        isTakingDamage = true;

        // Pastikan saat kena damage, parameter jalan dimatikan paksa
        // agar Animator tidak bingung mau lari atau sakit.
        bodyAnimator.SetFloat("Speed", 0f);
        foreach (Animator anim in layerAnimators)
        {
            if (anim != null) anim.SetFloat("Speed", 0f);
        }

        // Set Arah Damage
        bodyAnimator.SetFloat("TakeDamageX", lastDirection.x);
        bodyAnimator.SetFloat("TakeDamageY", lastDirection.y);

        foreach (Animator anim in layerAnimators)
        {
            if (anim != null)
            {
                anim.SetFloat("TakeDamageX", lastDirection.x);
                anim.SetFloat("TakeDamageY", lastDirection.y);
            }
        }

        // Trigger
        bodyAnimator.SetTrigger(nameAnimation);
        Debug.Log($"Memainkan animasi: {nameAnimation}");
        foreach (Animator anim in layerAnimators)
        {
            if (anim != null) anim.SetTrigger(nameAnimation);
        }

        if (nameAnimation != "Die")
        {
            // Hentikan coroutine lama jika ada (biar tidak tumpang tindih)
            StopAllCoroutines();
            StartCoroutine(ResetTakeDamageState());
        }
    }

    private IEnumerator ResetTakeDamageState()
    {
        // Tunggu sampai Unity benar-benar memulai transisi
        yield return null;

        float duration = 0.5f; // Default fallback

        // Jika Animator sedang transisi (Run -> TakeDamage), 
        // durasi yang benar ada di "NextState", bukan "CurrentState".
        if (bodyAnimator.IsInTransition(0))
        {
            duration = bodyAnimator.GetNextAnimatorStateInfo(0).length;
        }
        else
        {
            duration = bodyAnimator.GetCurrentAnimatorStateInfo(0).length;
        }

        // Tunggu sesuai durasi animasi
        yield return new WaitForSeconds(duration);

        isTakingDamage = false;

        // Kembalikan kontrol
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