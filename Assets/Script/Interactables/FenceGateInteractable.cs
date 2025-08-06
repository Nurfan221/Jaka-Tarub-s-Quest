using UnityEngine;

public class FenceGateInteractable : Interactable
{
    private FenceBehavior fenceBehavior;
    // Simpan status gerbang di sini juga untuk referensi cepat
    private bool isGateCurrentlyOpen;
    private void Start()
    {
        fenceBehavior = GetComponent<FenceBehavior>();
        promptMessage = "Buka Gerbang"; // Set default prompt message

    }

    protected override void Interact()
    {
        Debug.Log("Interact Gate");
        if (fenceBehavior != null)
        {
            // Panggil ToggleGate tanpa parameter return, dan berikan referensi ke objek ini
            fenceBehavior.ToggleGate(this);
        }
    }

    // Fungsi ini akan dipanggil oleh FenceBehavior setelah animasi selesai
    public void OnAnimationComplete()
    {
        // Ambil status terbaru dari FenceBehavior
        isGateCurrentlyOpen = fenceBehavior.isGateOpen;

        // Atur promptMessage berdasarkan status terbaru
        if (isGateCurrentlyOpen)
        {
            promptMessage = "Tutup Gerbang";
            PlayerUI.Instance.SetPromptText(promptMessage);
        }
        else
        {
            promptMessage = "Buka Gerbang";
            PlayerUI.Instance.SetPromptText(promptMessage);
        }
    }
}
