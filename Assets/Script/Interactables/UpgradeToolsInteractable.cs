using NUnit.Framework.Interfaces;
using UnityEngine;

public class UpgradeToolsInteractable : Interactable
{
    [Header("Upgraade Settings")]
    public UpgradeToolsDatabase upgradeToolsDatabase;
    public ItemData itemToUpgrade;
    public ItemData itemRequired;
    public ItemData resultItemUpgrade;
    public int upgradeCostAmount;
    public int upgradeTime;
    public bool canUpgrade = false;
    public bool startedUpgrade = false;
    public SpriteRenderer resultItemSprite;
    public float frameRate = 0.1f; // Waktu per frame (kecepatan animasi)
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        TimeManager.OnDayChanged += CheckForNewDays;
    }
    private void OnDisable()
    {
        TimeManager.OnDayChanged -= CheckForNewDays;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void Interact()
    {
        if (!startedUpgrade)
        {
            MechanicController.Instance.HandleOpenUpgradeTools(this);
        }else
        {
                       Debug.Log("Upgrade sedang berlangsung, tunggu hingga selesai!");
        }
    }
    public void UpdateSpriteHasil()
    {
        if (upgradeToolsDatabase != null)
        {
            resultItemSprite.gameObject.SetActive(true);
            resultItemSprite.sprite = upgradeToolsDatabase.itemToolResult.sprite;
        }
        else
        {
            resultItemSprite.gameObject.SetActive(false);
            resultItemSprite.sprite = null;
        }
    }

    public void CheckForNewDays()
    {
        if (startedUpgrade && TimeManager.Instance.date >= upgradeTime)
        {
            UpdateSpriteHasil();
        }else
        {
             Debug.Log("Upgrade masih dalam proses, harap tunggu hingga selesai.");
        }    
    }
}
