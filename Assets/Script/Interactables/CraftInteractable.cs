using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftInteractable : Interactable
{
    [SerializeField] CraftUI craftUI;
    protected override void Interact()
    {
        craftUI.OpenCraft();
    }
}
