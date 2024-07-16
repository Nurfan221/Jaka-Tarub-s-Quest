using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TamashiiAnim : MonoBehaviour
{
    TamashiiFollow tf;

    SpriteRenderer theSprite;

    [SerializeField] Sprite[] idleSprites;
    [SerializeField] Sprite[] walkSprites;

    [SerializeField] float idleAnimSpd = 3;
    [SerializeField] float walkingAnimSpd = .5f;

    enum AnimState
    {
        Idle,
        Walking
    }
    AnimState currentState = AnimState.Idle;
    AnimState prevState = AnimState.Idle;

    private void Awake()
    {
        tf = GetComponent<TamashiiFollow>();
    }
    // Start is called before the first frame update
    void Start()
    {
        theSprite = tf.sr;
    }

    // Update is called once per frame
    void Update()
    {
        if (!tf.path.reachedDestination)
            currentState = AnimState.Walking;
        else
            currentState = AnimState.Idle;


        if (prevState != currentState)
        {
            switch (currentState)
            {
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
                startTime = Time.time;
                currentFrame = (int)((Time.time - startTime) * images.Length / animSpd);
            }
            theSprite.sprite = images[currentFrame];
            yield return null;
        }
    }
}
