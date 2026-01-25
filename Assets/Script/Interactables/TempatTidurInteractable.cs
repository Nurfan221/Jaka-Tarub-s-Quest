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
        Debug.Log("Pemain memilih untuk tidur.");


        TimeManager.Instance.AdvanceToNextDay();
        SaveDataManager.Instance.SaveGame();

        StartCoroutine(LoadingScreenUI.Instance.SetLoadingandTimer(false));


    }
}
