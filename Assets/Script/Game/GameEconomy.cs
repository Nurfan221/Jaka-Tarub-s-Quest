using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameEconomy : MonoBehaviour // Attach this to a persistent game object like Game Controller
{
    public static GameEconomy Instance;

    public int money;

    private void Awake()
    {
        Instance = this;
    }

    public bool SpendMoney(int price)
    {
        if (price > money)
        {
            return false;
        }
        else
        {
            money -= price;
            return true;
        }
    }

    public void GainMoney(int riches)
    {
        money += riches;
    }

    public void LostMoney(int lost)
    {
        money -= lost;
        if (money < 0)
            money = 0;
    }
}
