public class EnvironmentInteractable : Interactable
{
    public EnvironmentBehavior envBehavior; // Changed to public to be accessible
    public TypeObject typeObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        typeObject = gameObject.GetComponent<EnvironmentBehavior>().typeObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    protected override void Interact()
    {
        envBehavior.GetItemDrop();
    }
}
