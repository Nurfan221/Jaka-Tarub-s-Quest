using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantInteractable : Interactable
{
    public SeedManager seedManager; // Changed to public to be accessible

    private void Start()
    {
        seedManager = GetComponent<SeedManager>();
    }

    private void Update()
    {
        // if (SeedManager.isReadyToHarvest)
        // {
        //     promptMessage = "Panen Tanaman";
        // }
        // else
        // {
        //     promptMessage = "";
        // }
    }

    protected override void Interact()
    {
        if (seedManager.isReadyToHarvest)
        {
            seedManager.Harvest();
        }
    }
}
