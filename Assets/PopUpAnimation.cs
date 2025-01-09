using UnityEngine;

public class PopUpAnimation : MonoBehaviour
{
    public float startYOffset = -2f;  // Seberapa jauh objek muncul dari bawah
    public float animationDuration = 0.5f;  // Durasi animasi muncul
    public AnimationCurve popUpCurve;  // Kurva animasi untuk efek lebih halus


    private Vector3 originalPosition;
    private bool isAnimating = false;
    private float timer = 0f;

    void Start()
    {
        originalPosition = transform.position;
        transform.position += new Vector3(0, startYOffset, 0);  // Pindah objek ke bawah
    }

    public void TriggerPopUp()
    {
        if (!isAnimating)
        {
            timer = 0f;
            isAnimating = true;
        }
    }

    void Update()
    {
        if (isAnimating)
        {
            timer += Time.deltaTime / animationDuration;
            float newY = Mathf.Lerp(originalPosition.y + startYOffset, originalPosition.y, popUpCurve.Evaluate(timer));
            transform.position = new Vector3(originalPosition.x, newY, originalPosition.z);

            if (timer >= 1f)
            {
                isAnimating = false;
                transform.position = originalPosition;
            }
        }
    }
}
