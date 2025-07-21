using System;
using UnityEngine;

public class PintuManager : MonoBehaviour
{
    public static PintuManager Instance { get; private set; }
    [Serializable]
    public class ArrayPintu
    {
        public string lokasiName;
        public GameObject pintuIn;
        public GameObject pintuOut;
        //public GameObject area;
    }

    public ArrayPintu[] pintuArray;

    [Header("Daftar Hubungan")]
    public GameObject player;

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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void EnterArea(string pintu)
    {
        Debug.Log("Nama pintu: " + pintu);

        // Mencari pintu dalam pintuArray berdasarkan nama pintu
        foreach (var pintuTujuan in pintuArray)
        {
            if (pintuTujuan.pintuIn.name == pintu)
            {
                // Mengambil posisi pintuIn
                Vector3 posisiPintu = pintuTujuan.pintuOut.transform.position;
                Debug.Log("Posisi Pintu Masuk (pintuOut): " + posisiPintu);

                player.transform.position = posisiPintu;
            }else if (pintuTujuan.pintuOut.name == pintu)
            {
                // Mengambil posisi pintuIn
                Vector3 posisiPintu = pintuTujuan.pintuIn.transform.position;
                Debug.Log("Posisi Pintu Masuk (pintuIn): " + posisiPintu);
                player.transform.position = posisiPintu;
            }
        }
    }
}
