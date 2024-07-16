using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaidCalling : MonoBehaviour
{
    [SerializeField] bool startRaidCallingTimer;

    [SerializeField] float intervalRaid = 5 * 60;
    float timer;

    private void Update()
    {
        if (startRaidCallingTimer)
        {
            timer += Time.deltaTime;
            if (timer > intervalRaid)
            {
                timer = 0;
                GameController.Instance.supposedRaid = true;
            }
        }
        else
        {
            timer = 0;
        }
    }
}
