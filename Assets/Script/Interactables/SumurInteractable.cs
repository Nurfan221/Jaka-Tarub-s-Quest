using UnityEngine;

public class SumurInteractable : Interactable
{
    [Header("Daftar hubungan")]
    [SerializeField] Player_Inventory playerInventory;
    [SerializeField] PlayerUI playerUI;
    public Item itemInteractable;
    private PlayerData_SO stats;
    private void Awake()
    {


        // Ambil "Papan Pengumuman" dari Otak dan simpan ke jalan pintas kita.
        if (PlayerController.Instance != null)
        {
            stats = PlayerController.Instance.playerData;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }
    }
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
        Debug.Log("mengisi penyiram tanaman");
       if(playerInventory != null && stats.equippedCombat[0].itemName == itemInteractable.itemName)
       {
            stats.equippedCombat[0].health = stats.equippedCombat[0].maxhealth;
            playerUI.UpdateCapacityBar(stats.equippedItemData[0]);
       }
    }
}
