using UnityEngine;

public class CraftInteractable : Interactable
{

    protected override void Interact()
    {
        Debug.Log("cek interactable ");
        MechanicController.Instance.HandleOpenCrafting();
    }
}
