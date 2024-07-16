using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestQuanta : MonoBehaviour
{

    public void Take()
    {
        GameController.QuestItemCount++;
        if (GameController.QuestItemCount >= 5)
        {
            GameController.CanFinishStory = true;
            Player_Direction.Instance.Target = ForestController.Instance.VillagePortal;
            Player_Quest.Instance.SetQuest("Berbicara dengan Kak Ren");
        }
    }
}
