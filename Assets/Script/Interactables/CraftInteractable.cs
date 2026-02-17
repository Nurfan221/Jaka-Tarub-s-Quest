using UnityEngine;

public class CraftInteractable : Interactable
{
    public bool isCraftFood;
    protected override void Interact()
    {
        Debug.Log("cek interactable ");
        TutorialManager.Instance.TriggerTutorial("Tutorial_Craft");
        MechanicController.Instance.HandleOpenCrafting(isCraftFood);
    }
}
