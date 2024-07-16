using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Quest : MonoBehaviour
{
    public static Player_Quest Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SetQuest(string quest)
    {
        PlayerUI.Instance.currentQuestText.text = quest;
    }

    public string GetQuest()
    {
        return PlayerUI.Instance.currentQuestText.text;
    }
}
