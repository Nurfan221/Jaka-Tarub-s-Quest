public class PintuInteractable : Interactable
{
    //[SerializeField] PintuManager pintuManager;
    public string pintuName;
    public IdPintu idPintu;
    public bool isPintuIn; // menyimpan variabel jika 0 berarti out jika 1 berarti in

    public bool rumahTerbengkalai;
    public Dialogues dialogueRumahTerbengkalai;
    private void Start()
    {
        pintuName = gameObject.name;
    }
    protected override void Interact()
    {
        //StartCoroutine(PintuManager.Instance.EnterArea(pintuName));
        if (rumahTerbengkalai)
        {
            DialogueSystem.Instance.HandlePlayDialogue(dialogueRumahTerbengkalai);
        }
        else
        {
            PintuManager.Instance.EnterArea(idPintu, isPintuIn);

        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

}
