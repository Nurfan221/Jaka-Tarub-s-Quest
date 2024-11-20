using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GiveCountContainer : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI textCount;
    [SerializeField] NPCBehavior npcbehavior;
    [SerializeField] private Slider itemQuantitySlider;
    [SerializeField] GameObject itemQuantityPanel;
    public int countGift;

    public Button yes;
    public Button no;
    
    private ItemDragHandler itemDragHandler; // Tambahkan referensi ke ItemDragHandler

    void Start()
    {
        // Dapatkan referensi ke ItemDragHandler
        itemDragHandler = FindObjectOfType<ItemDragHandler>();

        UpdateQuantityText(itemQuantitySlider.value);
        itemQuantitySlider.onValueChanged.AddListener(UpdateQuantityText);
        
        yes.GetComponent<Button>().onClick.AddListener(() => {
            // Ambil nilai terbaru dari slider untuk countGift
            countGift = (int)itemQuantitySlider.value; 
            Debug.Log("NIlai count gift"+ countGift);
            // Panggil metode di ItemDragHandler untuk mengupdate itemCount
            itemDragHandler.UpdateItemCount(countGift);
            SetCountInactive();
        });
        
        no.GetComponent<Button>().onClick.AddListener(() => SetCountInactive());
    }

    void UpdateQuantityText(float value)
    {
        textCount.text = "Quantity: " + value.ToString("0");
        countGift = (int)value;
    }

    public void SetCountActive()
    {
        itemQuantityPanel.SetActive(true);
    }

    public void SetCountInactive()
    {
        itemQuantityPanel.SetActive(false);
    }
}
