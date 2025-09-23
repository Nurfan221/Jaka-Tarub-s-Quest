using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerInteractable : Interactable
{
    public EnvironmentBehavior envBehavior; // Changed to public to be accessible

    private void Start()
    {
        promptMessage = "Ambil Bunga";
    }

    private void Update()
    {
        // if (plantSeed.isReadyToHarvest)
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
        envBehavior.GetItemDrop();
    }
}
