using UnityEngine;

public class CraftInteractable : Interactable
{
    [SerializeField] private Craft craft;

    protected override void Interact()
    {
        Debug.Log("cek interactable ");
        craft.OpenCraft();
    }
}
