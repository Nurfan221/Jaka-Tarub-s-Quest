using UnityEngine;

public class PintuInteractable : Interactable
{
    //[SerializeField] PintuManager pintuManager;
    public string pintuName;
    public IdPintu idPintu;
    public bool isPintuIn; // menyimpan variabel jika 0 berarti out jika 1 berarti in

    private void Start()
    {
        pintuName = gameObject.name;
    }
    protected override void Interact()
    {
        //StartCoroutine(PintuManager.Instance.EnterArea(pintuName));
        PintuManager.Instance.EnterArea(idPintu, isPintuIn);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

}
