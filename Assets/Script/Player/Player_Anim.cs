using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Anim : MonoBehaviour
{
    [SerializeField] SpriteRenderer theSprite;
    Player_Movement pm;

    enum AnimState
    {
        Idle,
        WalkingUp,
        WalkingDown,
        WalkingLeft,
        WalkingRight
    }
    AnimState currentState = AnimState.Idle;
    AnimState prevState = AnimState.Idle;

    [SerializeField] float idleAnimSpd = 3;
    [SerializeField] float walkingAnimSpd = 0.5f;

    [SerializeField] Sprite[] idleAnim;
    [SerializeField] Sprite[] upAnim;
    [SerializeField] Sprite[] downAnim;
    [SerializeField] Sprite[] leftAnim;
    [SerializeField] Sprite[] rightAnim;

    [SerializeField] float upAnimSpd = 0.5f;
    [SerializeField] float downAnimSpd = 0.5f;
    [SerializeField] float leftAnimSpd = 0.5f;
    [SerializeField] float rightAnimSpd = 0.5f;

    void Start()
    {
        pm = GetComponent<Player_Movement>();
    }

    void Update()
    {
        Vector2 moveDir = pm.moveDir;

        if (pm.isMoving)
        {
            if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y))
            {
                currentState = moveDir.x > 0 ? AnimState.WalkingRight : AnimState.WalkingLeft;
            }
            else
            {
                currentState = moveDir.y > 0 ? AnimState.WalkingUp : AnimState.WalkingDown;
            }
        }
        else
        {
            currentState = AnimState.Idle;
        }

        if (prevState != currentState)
        {
            switch (currentState)
            {
                case AnimState.Idle:
                    LoopSprite(idleAnim, idleAnimSpd);
                    break;
                case AnimState.WalkingUp:
                    LoopSprite(upAnim, upAnimSpd);
                    break;
                case AnimState.WalkingDown:
                    LoopSprite(downAnim, downAnimSpd);
                    break;
                case AnimState.WalkingLeft:
                    LoopSprite(leftAnim, leftAnimSpd);
                    break;
                case AnimState.WalkingRight:
                    LoopSprite(rightAnim, rightAnimSpd);
                    break;
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
                startTime = Time.time;
                currentFrame = 0;
            }
            theSprite.sprite = images[currentFrame];
            yield return null;
        }
    }
}
