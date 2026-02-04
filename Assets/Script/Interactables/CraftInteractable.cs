using UnityEngine;

public class CraftInteractable : Interactable
{
    public bool isCraftFood;
    protected override void Interact()
    {
        Debug.Log("cek interactable ");
        MechanicController.Instance.HandleOpenCrafting(isCraftFood);
    }
}
