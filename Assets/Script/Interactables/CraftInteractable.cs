using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftInteractable : Interactable
{
    [SerializeField] Craft craft;
    protected override void Interact()
    {
        craft.OpenCraft();
    }
}
