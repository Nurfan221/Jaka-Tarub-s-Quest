using UnityEngine;

public class CraftInteractable : Interactable
{
    public bool isCraftFood;
    public bool useArrowVisual;
    public Transform arrowVisual;

    private void Start()
    {
        UseArrawVisualfunction();
    }

    protected override void Interact()
    {
        Debug.Log("cek interactable ");
        TutorialManager.Instance.TriggerTutorial("Tutorial_Craft");
        MechanicController.Instance.HandleOpenCrafting(isCraftFood);
        useArrowVisual = false;
        UseArrawVisualfunction();
    }

    public void UseArrawVisualfunction()
    {
        if (useArrowVisual && arrowVisual != null)
        {
            arrowVisual.gameObject.SetActive(true); // Pastikan panah awalnya tidak aktif
        }
        else
        {
            arrowVisual.gameObject.SetActive(false);
        }
    }
}
