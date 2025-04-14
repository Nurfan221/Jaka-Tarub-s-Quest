using System;
using UnityEngine;

public class PintuManager : MonoBehaviour
{
    [Serializable]
    public class ArrayPintu
    {
        public string pintuName;
        public GameObject pintuIn;
        public GameObject pintuOut;
        public GameObject area;
    }

    public ArrayPintu[] pintuArray;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnterArea(GameObject pintu)
    {
        string pintuName = pintu.name;
        foreach (var pintuIn in pintuArray)
        {
            string pintuInName = pintuIn.pintuIn.name;
            if (pintuIn != null && pintuName == pintuInName)
            {
                Debug.Log("masuk ke area : " + pintuIn.area.name);
                LoadingScreenUI.Instance.LoadScene(0);
            }
        }
    }
}
