using System.Collections.Generic;
using UnityEngine;

public class NPCAnimation : MonoBehaviour
{
    [Header("Master Animator (badan utama NPC)")]
    public Animator bodyAnimator;

    [Header("Animator bagian tubuh (otomatis terisi)")]
    public List<Animator> layerAnimators = new List<Animator>();

    public Vector2 lastDirection = Vector2.down;
    private Vector2 previousPos;

    private void Awake()
    {
        AutoFindAnimators();
    }

    void Update()
    {
        Vector2 movement = ((Vector2)transform.position - previousPos).normalized;

        UpdateAnimationParameters(movement);

        previousPos = transform.position;
    }


    public void AutoFindAnimators()
    {
        if (bodyAnimator == null)
            bodyAnimator = GetComponentInChildren<Animator>();

        layerAnimators.Clear();

        string[] partNames = { "Baju", "Celana", "Rambut", "Sepatu" };

        foreach (string name in partNames)
        {
            Transform t = transform.Find(name);
            if (t != null)
            {
                Animator ani = t.GetComponent<Animator>();
                if (ani != null) layerAnimators.Add(ani);
            }
        }
    }


    void UpdateAnimationParameters(Vector2 movement)
    {
        if (bodyAnimator == null) return;

        movement = movement.normalized;
        bool isMoving = movement != Vector2.zero;

        if (isMoving)
        {
            lastDirection = movement;

            SetAnim(bodyAnimator, movement.x, movement.y, 1f);
        }
        else
        {
            SetAnim(bodyAnimator, lastDirection.x, lastDirection.y, 0f);
        }

        // Copy parameter ke baju/celana/rambut/sepatu
        foreach (Animator anim in layerAnimators)
        {
            anim.SetFloat("MoveX", bodyAnimator.GetFloat("MoveX"));
            anim.SetFloat("MoveY", bodyAnimator.GetFloat("MoveY"));
            anim.SetFloat("IdleX", bodyAnimator.GetFloat("IdleX"));
            anim.SetFloat("IdleY", bodyAnimator.GetFloat("IdleY"));
            anim.SetFloat("Speed", bodyAnimator.GetFloat("Speed"));
        }

        SyncVisuals();
    }

    void SetAnim(Animator anim, float x, float y, float speed)
    {
        anim.SetFloat("MoveX", Mathf.Round(x));
        anim.SetFloat("MoveY", Mathf.Round(y));
        anim.SetFloat("IdleX", Mathf.Round(lastDirection.x));
        anim.SetFloat("IdleY", Mathf.Round(lastDirection.y));
        anim.SetFloat("Speed", speed);
    }


    void SyncVisuals()
    {
        if (bodyAnimator == null) return;

        AnimatorStateInfo m = bodyAnimator.GetCurrentAnimatorStateInfo(0);
        int hash = m.fullPathHash;
        float time = m.normalizedTime;

        foreach (Animator anim in layerAnimators)
        {
            AnimatorStateInfo s = anim.GetCurrentAnimatorStateInfo(0);

            if (s.fullPathHash != hash ||
                Mathf.Abs(s.normalizedTime - time) > 0.02f)
            {
                anim.Play(hash, 0, time);
            }
        }
    }


}
