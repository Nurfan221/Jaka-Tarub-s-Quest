using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance;

    public Image dashUI;

    public Image specialAttackUI;
    public Image[] quickSlotsUI_HUD = new Image[2];
    public TMP_Text promptText;

    public Image healthUI;
    public Image staminaUI;

    public Image equippedUI;
    public Image[] quickSlotsUI_Inventory = new Image[2];
    public GameObject inventoryUI;

    public TMP_Text currentQuestText;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
}
