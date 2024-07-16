using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameEconomy : MonoBehaviour // Attach this to a persistent game object like Game Controller
{
    public static GameEconomy Instance;

    public int Money;

    private void Awake()
    {
        Instance = this;
    }

    public bool SpendMoney(int price)
    {
        if (price > Money)
        {
            return false;
        }
        else
        {
            Money -= price;
            return true;
        }
    }

    public void GainMoney(int riches)
    {
        Money += riches;
    }

    public void LostMoney(int lost)
    {
        Money -= lost;
        if (Money < 0)
            Money = 0;
    }
}
