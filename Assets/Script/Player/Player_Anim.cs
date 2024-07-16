using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Anim : MonoBehaviour
{
    [SerializeField] SpriteRenderer theSprite;
    Player_Movement pm;

    enum AnimState
    {
        Idle,
        Sword,
        Bow,
        Walking
    }
    AnimState currentState = AnimState.Idle;
    AnimState prevState = AnimState.Idle;

    [SerializeField] float idleAnimSpd = 3;
    [SerializeField] float walkingAnimSpd = .5f;
    [SerializeField] float swordAnimSpd = 3;
    [SerializeField] float bowAnimSpd = .5f;

    [SerializeField] Sprite[] idleAnim;
    [SerializeField] Sprite[] walkingAnim;
    [SerializeField] Sprite[] swordAnim;
    [SerializeField] Sprite[] bowAnim;

    // Start is called before the first frame update
    void Start()
    {
        pm = GetComponent<Player_Movement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pm.isMoving)
            currentState = AnimState.Walking;
        else
            currentState = AnimState.Idle;


        if (prevState != currentState)
        {
            switch (currentState)
            {
                case AnimState.Idle:
                    LoopSprite(idleAnim, idleAnimSpd);
                    break;
                case AnimState.Walking:
                    LoopSprite(walkingAnim, walkingAnimSpd);
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
                startTime = Time.time;
                currentFrame = (int)((Time.time - startTime) * images.Length / animSpd);
            }
            theSprite.sprite = images[currentFrame];
            yield return null;
        }

        //foreach (Sprite image in images)
        //{
        //    theSprite.sprite = image;
        //    yield return null;
        //}
    }
}
