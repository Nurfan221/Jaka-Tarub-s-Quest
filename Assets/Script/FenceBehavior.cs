using UnityEngine;
using System.Collections;

public class FenceBehavior : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] fenceSprites;
    public LayerMask fenceLayer;
    public BoxCollider2D boxCollider2D;

    public bool isTop, isBottom, isLeft, isRight;

    private bool isUpdating = false; // Tambahkan flag ini
    [Header("Gate Settings")]
    public bool isGate;
    public bool isGateOpen;
    public Sprite[] gateSprite;
    public float frameRate = 0.3f; // Waktu per frame (kecepatan animasi)

    //private SpriteRenderer spriteRenderer; // Komponen SpriteRenderer
    private int currentFrame = 0; // Indeks frame saat ini

    private void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
    }
    public void UpdateFenceSprite()
    {
        // Cegah perulangan jika sudah sedang dalam proses update
        if (isUpdating) return;
        if (isGate) return; // Jika ini adalah gerbang, tidak perlu update sprite pagar

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

    public void ToggleGate(FenceGateInteractable gate) // <<< TAMBAH PARAMETER
    {
        // Hentikan animasi sebelumnya jika ada untuk mencegah konflik
        StopAllCoroutines();

        if (isGateOpen)
        {
            // Jika gerbang terbuka, jalankan animasi menutup
            StartCoroutine(CloseGateAnimation(gate)); // <<< BERIKAN CALLBACK
        }
        else
        {
            // Jika gerbang tertutup, jalankan animasi membuka
            StartCoroutine(OpenGateAnimation(gate)); // <<< BERIKAN CALLBACK
        }
    }


    private IEnumerator OpenGateAnimation(FenceGateInteractable gate)
    {
        // Pengecekan keamanan di awal coroutine
        if (gateSprite.Length == 0)
        {
            yield break;
        }

        // Loop dari frame pertama ke frame terakhir
        for (int i = 0; i < gateSprite.Length; i++)
        {
            spriteRenderer.sprite = gateSprite[i];
            yield return new WaitForSeconds(frameRate);
        }

        // Tandai gerbang sebagai terbuka setelah animasi selesai
        isGateOpen = true;
        
        Debug.Log("Gerbang terbuka!");
        if (boxCollider2D != null)
        {
            boxCollider2D.isTrigger = true; // Gerbang terbuka, jadi jadikan trigger
        }

        gate.OnAnimationComplete();
    }

    private IEnumerator CloseGateAnimation(FenceGateInteractable gate)
    {
        // Pengecekan keamanan di awal coroutine
        if (gateSprite.Length == 0)
        {
            yield break;
        }

        // Loop dari frame terakhir ke frame pertama
        for (int i = gateSprite.Length - 1; i >= 0; i--)
        {
            spriteRenderer.sprite = gateSprite[i];
            yield return new WaitForSeconds(frameRate);
        }

        // Tandai gerbang sebagai tertutup setelah animasi selesai
        isGateOpen = false;
        Debug.Log("Gerbang tertutup!");
        if (boxCollider2D != null)
        {
            boxCollider2D.isTrigger = false; // Gerbang terbuka, jadi jadikan trigger
        }

        gate.OnAnimationComplete();
    }
}






