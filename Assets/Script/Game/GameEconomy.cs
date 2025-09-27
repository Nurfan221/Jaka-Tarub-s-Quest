using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class GameEconomy : MonoBehaviour // Attach this to a persistent game object like Game Controller
{
    public static GameEconomy Instance { get; private set; }

    private void OnEnable()
    {
        TimeManager.OnDayChanged += UpdateMoneyText; // Subscribe to the event when a new day starts
    }

    private void OnDisable()
    {
        TimeManager.OnDayChanged -= UpdateMoneyText; // Unsubscribe to avoid memory leaks
    }

    public int money;
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
    private void Start()
    {
        UpdateMoneyText();
    }
    public bool SpendMoney(int price)
    {
        if (price > money)
        {
            UpdateMoneyText();
            return false;
        }
        else
        {
            money -= price;
            UpdateMoneyText();
            return true;
        }
    }

    public void GainMoney(int riches)
    {
        money += riches;
        UpdateMoneyText();
    }

    public void LostMoney(int lost)
    {
        money -= lost;
        if (money < 0)
            money = 0;

        UpdateMoneyText();
    }

    public void UpdateMoneyText()
    {
        if (PlayerUI.Instance.moneyText != null)
        {
            PlayerUI.Instance.moneyText.text = money.ToString();
        }
        else
        {
            Debug.LogWarning("Money Text UI element is not assigned!");
        }
    }   
}
