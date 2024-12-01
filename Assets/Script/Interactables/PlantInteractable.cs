using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantInteractable : Interactable
{
    public PlantSeed plantSeed; // Changed to public to be accessible

    private void Start()
    {
        plantSeed = GetComponent<PlantSeed>();
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
        if (plantSeed.isReadyToHarvest)
        {
            plantSeed.Harvest();
        }
    }
}
