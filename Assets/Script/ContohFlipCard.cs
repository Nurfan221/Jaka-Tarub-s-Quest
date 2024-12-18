using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ContohFlipCard : MonoBehaviour
{

    public GameObject frontSide; // Drag the front side GameObject here
    public GameObject backSide;  // Drag the back side GameObject here
    public Button flipButton;    // Drag the flip button here

    public bool Description = false; 
    // Start is called before the first frame update
    void Start()
    {
         if (flipButton != null)
        {
            flipButton.onClick.AddListener(FlipCard);
            Debug.Log("Flip button listener added.");
        }
        else
        {
            Debug.LogError("Flip Button is not assigned.");
        }

        if (frontSide == null || backSide == null)
        {
            Debug.LogError("Front Side or Back Side GameObject is not assigned.");
        }

        // Initialize with front side showing
        if (frontSide != null && backSide != null)
        {
            frontSide.SetActive(true);
            backSide.SetActive(false);
            Debug.Log("Initialized: Front side showing, back side hidden.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FlipCard()
    {
        Debug.Log("FlipCard method called.");
        ShowDescription();
    }

    public void ShowDescription(){
        Debug.Log("ShowDescription method called.");

        if(Description == false){
        frontSide.SetActive(false);
        backSide.SetActive(true);
        Description = true;

        }else{
            frontSide.SetActive(true);
            backSide.SetActive(false);
            Description = false;
        }
           
    }
    public void IfClose()
    {
        frontSide.SetActive(true);
        backSide.SetActive(false);
        Description = false;
    }
}