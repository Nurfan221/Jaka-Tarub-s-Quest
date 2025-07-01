using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player_Anim : MonoBehaviour
{
    public Animator animator;
    Player_Movement pm;
    SpriteRenderer sr;
    public bool isAttacking;
    public bool isTakingDamage = false;
    public Vector2 lastDirection = Vector2.down; // Default menghadap bawah

    private void Start()
    {
        pm = GetComponent<Player_Movement>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        UpdateAnimation();
    }

    public void UpdateAnimation()
    {
        if (animator == null)
        {
            Debug.LogError("Animator belum di-assign!");
            return;
        }

        if (isTakingDamage) return; // Jangan ubah animasi saat terkena damage

        Vector2 movement = pm.movementDirection.normalized; // Normalisasi vektor
        bool isMoving = movement != Vector2.zero;

        if (isMoving)
        {
            lastDirection = movement;
            animator.SetFloat("MoveX", Mathf.Round(movement.x));
            animator.SetFloat("MoveY", Mathf.Round(movement.y));
        }
        else
        {
            animator.SetFloat("IdleX", Mathf.Round(lastDirection.x));
            animator.SetFloat("IdleY", Mathf.Round(lastDirection.y));
        }
        animator.SetFloat("Speed", isMoving ? 1f : 0f);
    }



    // Fungsi untuk menyerang
    public void PlayAttackAnimation()
    {
        if (animator == null) return;

        isAttacking = true;
        animator.SetTrigger("Attack");

        // Reset ke mode berjalan setelah animasi selesai
        StartCoroutine(ResetAttackState());
    }

    public void PlayTakeDamageAnimation()
    {
        if (animator == null) return;

        isTakingDamage = true;
        animator.SetTrigger("TakeDamage");

        // Pastikan animasi take damage berjalan, lalu kembali ke idle
        StartCoroutine(ResetTakeDamageState());
    }
    private IEnumerator ResetTakeDamageState()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isTakingDamage = false;
        UpdateAnimation(); // Kembalikan ke animasi normal
    }


    private IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isAttacking = false;
        UpdateAnimation();
    }


}
