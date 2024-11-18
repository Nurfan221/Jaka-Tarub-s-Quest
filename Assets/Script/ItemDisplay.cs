using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemDisplay : MonoBehaviour
{
    public static ItemDisplay Instance;
    [SerializeField] Image itemSprite;
    [SerializeField] TMP_Text itemNameText;
    [SerializeField] Button nextButton;

    [Header("STORY")]
    [SerializeField] Dialogues PedangRenPart2;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void ShowItemDisplay(Item item)
    {
        transform.GetChild(0).gameObject.SetActive(true);
        itemSprite.sprite = item.sprite;
        itemNameText.text = item.itemName;
    }

    public void AddButtonListener(UnityAction handle)
    {
        nextButton.onClick.RemoveAllListeners();
        if (handle != null)
            nextButton.onClick.AddListener(handle);
    }

    public void DialoguePedangPart2()
    {
        // DialogueSystem.Instance.StartDialogue(PedangRenPart2);
    }
}
