using System.Collections;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public Animator animator;




    public IEnumerator PlayAndWaitForAnimation(string stateName, int layer = 0)
    {
        animator.Play(stateName);

        yield return null;

        // Loop ini akan terus berjalan selama kondisi di dalamnya terpenuhi
        while (animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName) &&
               animator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1.0f)
        {
            // Tunggu frame berikutnya sebelum mengecek lagi
            yield return null;
        }

        Debug.Log($"Animasi '{stateName}' telah selesai.");
    }

}