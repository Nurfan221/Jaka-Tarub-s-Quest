using UnityEngine;

public class PintuInteractable : Interactable
{
    //[SerializeField] PintuManager pintuManager;
    public string pintuName;

    private void Start()
    {
        pintuName = gameObject.name;
    }
    protected override void Interact()
    {
        PintuManager.Instance.EnterArea(pintuName);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
}
