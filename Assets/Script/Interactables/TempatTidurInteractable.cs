using UnityEngine;

public class TempatTidurInteractable : Interactable
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void Interact()
    {
        TimeManager.Instance.hour = 99;
    }
}
