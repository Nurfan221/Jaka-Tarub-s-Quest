using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Animations;

public class AnimatorUIController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static AnimatorUIController Instance { get; private set; }
    public Animator panelAnimator;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);

        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AnimateSaveButton()
    {
        Debug.Log("AnimateSaveButton method called.");
        panelAnimator.SetTrigger("TriggerSaveButton");
    }
}
