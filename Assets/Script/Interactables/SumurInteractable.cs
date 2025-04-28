using UnityEngine;

public class SumurInteractable : Interactable
{
    [Header("Daftar hubungan")]
    [SerializeField] Player_Inventory playerInventory;
    [SerializeField] PlayerUI playerUI;
    public Item itemInteractable;

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
       if(playerInventory != null && playerInventory.equippedCombat[0].itemName == itemInteractable.itemName)
       {
            playerInventory.equippedCombat[0].health = playerInventory.equippedCombat[0].maxhealth;
            playerUI.UpdateCapacityBar(playerInventory.equippedCombat[0]);
       }
    }
}
