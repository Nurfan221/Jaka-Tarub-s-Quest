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
            UpdatePromptMessage(perangkapBehavior.IsFull);
    }

    private void OnEnable()
    {


        // Jika PerangkapBehavior punya event onStateChanged (mis. OnFullChanged), subscribe di sini:
        if (perangkapBehavior != null)
            perangkapBehavior.OnFullChanged += UpdatePromptMessage;


    }

    private void OnDisable()
    {
        if (perangkapBehavior != null)
            perangkapBehavior.OnFullChanged -= UpdatePromptMessage;
    }




    protected override void Interact()
    {
        // Safety check: pastikan komponen ada
        if (perangkapBehavior == null)
        {
            Debug.LogWarning("[PerangkapInteractable] PerangkapBehavior tidak ditemukan.");
            return;
        }

        if (perangkapBehavior.IsFull)
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
        UpdatePromptMessage(perangkapBehavior.IsFull);
    }

    private void UpdatePromptMessage(bool isFull)
    {
        if (isFull)
            promptMessage = "Perangkap penuh, ambil hasil tangkapan.";
        else
            promptMessage = "Perangkap kosong, tunggu hasil tangkapan.";
    }

}
