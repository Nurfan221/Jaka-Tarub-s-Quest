using UnityEngine;

[RequireComponent(typeof(PerangkapBehavior))]
public class PerangkapInteractable : Interactable
{
    public PerangkapBehavior perangkapBehavior;

    private void Awake()
    {
        // Ambil referensi, aman even jika GetComponent gagal
        perangkapBehavior = GetComponent<PerangkapBehavior>();
        // Set prompt sekali ketika aktif
        if (perangkapBehavior != null)
            UpdatePromptMessage(perangkapBehavior._isFull);
    }






    protected override void Interact()
    {
        // Safety check: pastikan komponen ada
        if (perangkapBehavior == null)
        {
            Debug.LogWarning("[PerangkapInteractable] PerangkapBehavior tidak ditemukan.");
            return;
        }

        if (perangkapBehavior._isFull)
        {
            // Ambil hewan dari perangkap
            perangkapBehavior.TakeAnimal();
        }
        else
        {
            // Pasang / ambil perangkap kosong
            perangkapBehavior.TakePerangkap();
        }

        // Setelah interaksi, update prompt agar selalu sinkron
        UpdatePromptMessage(perangkapBehavior._isFull);
    }

    public void UpdatePromptMessage(bool isFull)
    {
        if (isFull)
            promptMessage = "mengambil hasil perangkap";
        else
            promptMessage = "mengambil perangkap";
    }

}
