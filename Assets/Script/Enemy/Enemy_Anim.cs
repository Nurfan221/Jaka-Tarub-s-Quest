using System.Collections;
using Pathfinding;
using UnityEngine;

public class Enemy_Anim : MonoBehaviour
{
    AIPath path;

    [SerializeField] SpriteRenderer theSprite;

    [SerializeField] Sprite[] idleSprites;
    [SerializeField] Sprite[] walkSprites;
    [SerializeField] Sprite[] attackSprites;

    [SerializeField] float idleAnimSpd = 3;
    [SerializeField] float walkingAnimSpd = .5f;
    [SerializeField] float attackAnimSpd = .5f;

    bool doneAttacking = false;

    enum AnimState
    {
        Idle,
        Walking,
        Attack
    }
    AnimState currentState = AnimState.Idle;
    AnimState prevState = AnimState.Idle;

    private void Awake()
    {
        path = GetComponent<AIPath>();
    }


    // Update is called once per frame
    void Update()
    {
        if (doneAttacking && path.reachedDestination && attackSprites.Length != 0)
        {
            currentState = AnimState.Attack;
        }
        else if (!doneAttacking)
        {
            if (!path.reachedDestination)
                currentState = AnimState.Walking;
            else
                currentState = AnimState.Idle;
        }




        if (prevState != currentState)
        {
            switch (currentState)
            {
                case AnimState.Attack:
                    LoopSprite(attackSprites, attackAnimSpd);
                    break;
                case AnimState.Idle:
                    LoopSprite(idleSprites, idleAnimSpd);
                    break;
                case AnimState.Walking:
                    LoopSprite(walkSprites, walkingAnimSpd);
                    break;
                default: break;
            }
        }

        prevState = currentState;
    }

    void LoopSprite(Sprite[] images, float animSpd)
    {
        StopAllCoroutines();
        StartCoroutine(Looping(images, animSpd));
    }
    IEnumerator Looping(Sprite[] images, float animSpd)
    {
        int currentFrame = 0;
        float startTime = Time.time;
        while (true)
        {
            currentFrame = (int)((Time.time - startTime) * images.Length / animSpd);
            if (currentFrame >= images.Length)
            {
                if (attackSprites.Length != 0)
                    StartCoroutine(countingAttack());
                doneAttacking = false;
                startTime = Time.time;
                currentFrame = (int)((Time.time - startTime) * images.Length / animSpd);
            }
            theSprite.sprite = images[currentFrame];
            yield return null;
        }
    }

    IEnumerator countingAttack()
    {
        yield return new WaitForSeconds(.5f);
        doneAttacking = true;

    }
}
