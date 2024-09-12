using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardFlip : MonoBehaviour
{
    public GameObject frontSide; // Drag the front side GameObject here
    public GameObject backSide;  // Drag the back side GameObject here
    public Button flipButton;    // Drag the flip button here

    private bool isFrontShowing = true; // Tracks which side is currently showing

    private void Start()
    {
        if (flipButton != null)
        {
            flipButton.onClick.AddListener(FlipCard);
            Debug.Log("Flip button listener added.");
        }
        else
        {
            Debug.LogError("Flip Button is not assigned.");
        }

        if (frontSide == null || backSide == null)
        {
            Debug.LogError("Front Side or Back Side GameObject is not assigned.");
        }

        // Initialize with front side showing
        if (frontSide != null && backSide != null)
        {
            frontSide.SetActive(true);
            backSide.SetActive(false);
            Debug.Log("Initialized: Front side showing, back side hidden.");
        }
    }

    private void FlipCard()
    {
        Debug.Log("FlipCard method called.");
        StartCoroutine(FlipAnimation());
    }

    private IEnumerator FlipAnimation()
    {
        Debug.Log("FlipAnimation started.");
        float elapsedTime = 0f;
        float duration = 0.5f; // Duration of the flip animation

        Vector3 frontStartRotation = frontSide.transform.rotation.eulerAngles;
        Vector3 frontEndRotation = new Vector3(frontStartRotation.x, frontStartRotation.y + 180, frontStartRotation.z);

        Vector3 backStartRotation = backSide.transform.rotation.eulerAngles;
        Vector3 backEndRotation = new Vector3(backStartRotation.x, backStartRotation.y + 180, backStartRotation.z);

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            frontSide.transform.rotation = Quaternion.Euler(Vector3.Lerp(frontStartRotation, frontEndRotation, t));
            backSide.transform.rotation = Quaternion.Euler(Vector3.Lerp(backStartRotation, backEndRotation, t));
            Debug.Log($"Animating... t: {t} elapsedTime: {elapsedTime} deltaTime: {Time.deltaTime}");
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Finalize the rotation
        frontSide.transform.rotation = Quaternion.Euler(frontEndRotation);
        backSide.transform.rotation = Quaternion.Euler(backEndRotation);

        // Swap visibility
        frontSide.SetActive(!isFrontShowing);
        backSide.SetActive(isFrontShowing);

        Debug.Log("Flip animation completed. Front side showing: " + !isFrontShowing);

        // Toggle the state
        isFrontShowing = !isFrontShowing;
    }
}
