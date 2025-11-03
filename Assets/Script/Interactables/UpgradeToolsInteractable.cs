using NUnit.Framework.Interfaces;
using UnityEngine;

public class UpgradeToolsInteractable : Interactable, ISaveable
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
    public bool finishUpgrade = false;
    public SpriteRenderer resultItemSprite;
    public float frameRate = 0.1f; // Waktu per frame (kecepatan animasi)
    public Dialogues finishUpgradeDialogue;
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

    public object CaptureState()
    {
        Debug.Log("[SAVE-CAPTURE] Menangkap data tungku (Furnance)...");
        UpgradeToolsSaveData data = new UpgradeToolsSaveData();
        data.upgradeToolsDatabase = upgradeToolsDatabase;
        data.startedUpgrade = startedUpgrade;
        data.resultItemUpgrade = resultItemUpgrade;

        return data;
    }


    public void RestoreState(object state)
    {
        
        // Coba cast 'state' yang datang kembali ke tipe aslinya.
        var loadedData = state as UpgradeToolsSaveData;

        if (loadedData != null)
        {
            upgradeToolsDatabase = loadedData.upgradeToolsDatabase;
            startedUpgrade = loadedData.startedUpgrade;
            resultItemUpgrade = loadedData.resultItemUpgrade;

            CheckForNewDays();
        }
        else
        {
            Debug.LogWarning("Gagal merestorasi data quest: data tidak valid atau corrupt.");
        }
    }

    protected override void Interact()
    {
        if (!startedUpgrade)
        {
           
            MechanicController.Instance.HandleOpenUpgradeTools(this);
        }else
        {
          Debug.Log("Upgrade sedang berlangsung, tunggu hingga selesai!");
            if (finishUpgrade)
            {
                DialogueSystem.Instance.HandlePlayDialogue(finishUpgradeDialogue);
                ItemPool.Instance.AddItem(resultItemUpgrade);
                upgradeToolsDatabase = null;
                itemToUpgrade = null;
                itemRequired = null;
                resultItemUpgrade = null;
                finishUpgrade = false;
                startedUpgrade = false;
                UpdateSpriteHasil();
            }
        }
    }

    private bool IsUpgradeToolsDatabaseEmpty()
    {
        return upgradeToolsDatabase == null || string.IsNullOrEmpty(upgradeToolsDatabase.toolsName);
    }
    public void UpdateSpriteHasil()
    {
        if (IsUpgradeToolsDatabaseEmpty())
        {

            resultItemSprite.gameObject.SetActive(false);
            resultItemSprite.sprite = null;
        }
        else
        {

            resultItemSprite.gameObject.SetActive(true);
            resultItemSprite.sprite = upgradeToolsDatabase.itemToolResult.sprite;
        }
    }

    public void CheckForNewDays()
    {
        if (startedUpgrade && TimeManager.Instance.date >= upgradeTime)
        {
            UpdateSpriteHasil();
            finishUpgrade = true;
        }else
        {
             Debug.Log("Upgrade masih dalam proses, harap tunggu hingga selesai."); 
        }    
    }
}
