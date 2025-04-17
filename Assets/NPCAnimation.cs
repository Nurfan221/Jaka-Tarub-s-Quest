using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NPCBehavior;

public class NPCAnimation : MonoBehaviour
{
    [SerializeField] Animator animator; // Referensi ke NPC
    [SerializeField] Animator bajuAnimation; //Referensi ke baju animator
    public Vector2 lastDirection = Vector2.down; // Default menghadap bawah
    private Vector2 previousPosition;
    public SpriteRenderer baju;
    public SpriteRenderer sr;


    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();  // Ambil SpriteRenderer untuk flip sprite
    }

    private void Update()
    {
        Vector2 movement = ((Vector2)transform.position - previousPosition).normalized;

        // Update animasi berdasarkan gerakan
        UpdateAnimation(movement);
        // Simpan posisi sekarang sebagai referensi untuk frame berikutnya
        previousPosition = transform.position;
    }



    public void UpdateAnimation(Vector2 movement)
    {
        if (animator == null)
        {
            Debug.LogError("Animator belum di-assign!");
            return;
        }

        movement = movement.normalized;
        bool isMoving = movement != Vector2.zero;

        if (isMoving)
        {
            // Simpan arah terakhir saat bergerak, agar animasi idle mengikuti arah ini
            lastDirection = movement;

            // Set parameter untuk Blend Tree berjalan
            animator.SetFloat("MoveX", Mathf.Round(movement.x)); // -1, 0, 1
            animator.SetFloat("MoveY", Mathf.Round(movement.y));
        }
        else
        {
            // Gunakan arah terakhir saat idle
            animator.SetFloat("IdleX", Mathf.Round(lastDirection.x));
            animator.SetFloat("IdleY", Mathf.Round(lastDirection.y));
        }

        // Atur Speed untuk blend tree berjalan
        animator.SetFloat("Speed", isMoving ? 1f : 0f);
    }



}
