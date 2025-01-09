using UnityEngine;

public class VisibilityController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private PopUpAnimation popUpAnimation;
    private bool isVisible;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        popUpAnimation = GetComponent<PopUpAnimation>();
    }

    void Update()
    {
        CheckVisibility();
    }

    void CheckVisibility()
    {
        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);
        bool isInView = viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1;

        if (isInView && !isVisible)
        {
            spriteRenderer.enabled = true;
            popUpAnimation.TriggerPopUp();
            isVisible = true;
        }
        else if (!isInView && isVisible)
        {
            spriteRenderer.enabled = false;
            isVisible = false;
        }
    }
}
