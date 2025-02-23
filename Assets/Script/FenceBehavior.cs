using UnityEngine;

public class FenceBehavior : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] fenceSprites;
    public LayerMask fenceLayer;

    public bool isTop, isBottom, isLeft, isRight;

    private bool isUpdating = false; // Tambahkan flag ini

    public void UpdateFenceSprite()
    {
        // Cegah perulangan jika sudah sedang dalam proses update
        if (isUpdating) return;

        isUpdating = true; // Set flag agar tidak memproses ulang

        isTop = CheckFence(Vector2.up);
        isBottom = CheckFence(Vector2.down);
        isLeft = CheckFence(Vector2.left);
        isRight = CheckFence(Vector2.right);

        Debug.Log("Top: " + isTop + ", Bottom: " + isBottom + ", Left: " + isLeft + ", Right: " + isRight);

        SelectSprite();

        isUpdating = false; // Reset flag setelah selesai
    }

    void SelectSprite()
    {
        int spriteIndex = 0;

        if (isTop) spriteIndex += 8;
        if (isBottom) spriteIndex += 4;
        if (isLeft) spriteIndex += 2;
        if (isRight) spriteIndex += 1;

        if (spriteIndex >= 0 && spriteIndex < fenceSprites.Length)
        {
            spriteRenderer.sprite = fenceSprites[spriteIndex];
            Debug.Log("Sprite Index: " + spriteIndex);

        }
    }

    bool CheckFence(Vector2 direction)
    {
        float raycastDistance = 1f;
        float offset = 0.5f;

        Vector2 raycastStart = (Vector2)transform.position + direction * offset;

        Debug.DrawRay(raycastStart, direction * (raycastDistance - offset), Color.green, 1f);

        RaycastHit2D hit = Physics2D.Raycast(raycastStart, direction, raycastDistance - offset, fenceLayer);

        if (hit.collider != null && hit.collider.CompareTag("Fence"))
        {
            Debug.Log("Fence detected at: " + hit.collider.gameObject.transform.position);

            FenceBehavior fence = hit.collider.GetComponent<FenceBehavior>();

            if (fence != null && fence != this && !fence.isUpdating) // Cek jika pagar lain tidak dalam proses update
            {
                fence.UpdateFenceSprite(); // Update pagar tetangga
            }

            return true;
        }

        return false;
    }
}






