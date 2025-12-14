using System.Collections;
using UnityEngine;

public class SwordAnim : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;
    [SerializeField] SpriteRenderer sr;
    [SerializeField] float animSpd = .5f;

    public void StartFX()
    {
        StopAllCoroutines();
        StartCoroutine(Slashing());
    }

    IEnumerator Slashing()
    {
        sr.enabled = true;
        //while (currentFrame < sprites.Length)
        //{
        //    sr.sprite = sprites[currentFrame];
        //    currentFrame++;
        //    yield return new WaitForSeconds(.1f);
        //}
        int currentFrame = 0;
        float startTime = Time.time;
        while (true)
        {
            currentFrame = (int)((Time.time - startTime) * sprites.Length / animSpd);
            if (currentFrame >= sprites.Length)
            {
                break;
            }
            sr.sprite = sprites[currentFrame];
            yield return null;
        }
        sr.enabled = false;
    }
}
