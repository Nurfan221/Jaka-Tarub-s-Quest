using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Anim : MonoBehaviour
{
    [SerializeField] Animator animator; // Referensi ke Animator
    Player_Movement pm;
    SpriteRenderer sr;


    private void Start()
    {
        pm = GetComponent<Player_Movement>();
        sr = GetComponent<SpriteRenderer>();  // Ambil SpriteRenderer untuk flip sprite
    }

    public void Update()
    {
        bool isMoving = pm.movement != Vector2.zero;

        if (isMoving)
        {
            // Jika bergerak ke atas
            if (pm.movement.y > 0.1f)
            {
                SetWalkAnimation(true, false, false, false); // Set WalkTop
            }
            // Jika bergerak ke bawah
            else if (pm.movement.y < -0.1f)
            {
                SetWalkAnimation(false, true, false, false); // Set WalkDown
            }
            // Jika bergerak ke kanan
            else if (pm.movement.x > 0.01f)
            {
                SetWalkAnimation(false, false, true, false); // Set WalkRight
                sr.flipX = false; // Tidak perlu membalikkan sprite, biarkan default
            }
            // Jika bergerak ke kiri
            else if (pm.movement.x < -0.1f)
            {
                SetWalkAnimation(false, false, false, true); // Set WalkLeft
                sr.flipX = true; // Membalikkan sprite untuk kiri
            }
        }
        else
        {
            // Jika tidak bergerak, set ke idle
            SetWalkAnimation(false, false, false, false);
        }
    }

    private void SetWalkAnimation(bool top, bool down, bool right, bool left)
    {
        animator.SetBool("WalkTop", top);
        animator.SetBool("WalkDown", down);
        animator.SetBool("WalkRight", right);
        animator.SetBool("WalkLeft", left);

        // Set PlayerIdle ketika tidak ada pergerakan
        animator.SetBool("PlayerIdle", !(top || down || right || left));
    }


}
