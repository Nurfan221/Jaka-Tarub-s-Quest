using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeraangkapInteractable : Interactable
{
    public PerangkapBehavior perangkapBehavior;

    private void Awake()
    {
        perangkapBehavior = GetComponent<PerangkapBehavior>();
    }

    void Start()
    {
        // Set prompt sesuai status awal
        if (perangkapBehavior != null)
        {
            UpdatePromptMessage(perangkapBehavior.isfull);
        }
    }

    public void UpdatePromptMessage(bool isfull)
    {
        promptMessage = isfull ? "Ambil Hewan" : "Interact";
    }

    protected override void Interact()
    {
        if (perangkapBehavior.isfull)
        {
            perangkapBehavior.TakeAnimal();
        }
        else
        {
            perangkapBehavior.TakePerangkap();
        }
    }
}
