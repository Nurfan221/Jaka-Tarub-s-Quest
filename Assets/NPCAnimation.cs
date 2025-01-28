using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAnimation : MonoBehaviour
{
    [SerializeField] Animator NPC; // Referensi ke NPC
    [SerializeField] Animator bajuAnimation; //Referensi ke baju animator
    public SpriteRenderer baju;
    public SpriteRenderer sr;


    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();  // Ambil SpriteRenderer untuk flip sprite
    }



    public void SetWalkAnimation(bool top, bool down, bool right, bool left)
    {
        if (NPC == null)
        {
            Debug.LogError("Animator NPC belum di-assign!");
            return;
        }

        NPC.SetBool("JalanAtas", top);
        NPC.SetBool("JalanBawah", down);
        NPC.SetBool("JalanKanan", right);
        NPC.SetBool("JalanKiri", left);

        //if (top)
        //{
        //    bajuAnimation.SetTrigger("TriggerWalkUp");
        //}else if(down)
        //{
        //    bajuAnimation.SetTrigger("TriggerWalkDown");
        //}
        //else if (left)
        //{
        //    bajuAnimation.SetTrigger("TriggerWalkLeft");
        //}
        //else if (right)
        //{
        //    bajuAnimation.SetTrigger("TriggerWalkRight");
        //}
        //else
        //{
        //    bajuAnimation.SetTrigger("TriggerIdle");
        //}

        // Set PlayerIdle ketika tidak ada pergerakan
        NPC.SetBool("Idle", !(top || down || right || left));
    }


}
