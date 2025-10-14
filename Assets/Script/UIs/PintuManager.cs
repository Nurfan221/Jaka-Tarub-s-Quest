using System;
using UnityEngine;
using System.Collections;

public class PintuManager : MonoBehaviour
{
    public static PintuManager Instance { get; private set; }
    public DatabaseManager databaseManager;

    [Header("Daftar Hubungan")]
    public GameObject player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.gameObject;
        if (player == null)
        {
            Debug.LogError("Player not found in the scene!");
            return;
        }
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
        databaseManager = DatabaseManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void EnterArea(IdPintu idPintu, bool isPintuIn)
    {
        Debug.Log("Nama pintu: " + idPintu);

        // Mencari pintu dalam pintuArray berdasarkan nama pintu
        foreach (var pintuTujuan in databaseManager.listPintu)
        {
            if (pintuTujuan.idPintu == idPintu)
            {
                if (isPintuIn)
                {
                    // Mengambil posisi pintuIn
                    Debug.Log("Posisi Pintu Masuk (pintuOut): " + pintuTujuan.pintuIn);
                    Debug.Log("posisi player sebelum pindah : " + player.transform.position);
                    player.transform.position = pintuTujuan.pintuOut;
                    Debug.Log("posisi player di pindahkan ke : " + player.transform.position);
                    StartCoroutine(LoadingScreenUI.Instance.SetLoadingandTimer(false));
                    SmoothCameraFollow.Instance.EnterHouse(true);
                }
                else
                {
                    // Mengambil posisi pintuOut
                    Debug.Log("Posisi Pintu Masuk (pintuIn): " + pintuTujuan.pintuIn);

                    Debug.Log("posisi player sebelum pindah : " + player.transform.position);
                    player.transform.position = pintuTujuan.pintuIn;
                    Debug.Log("posisi player di pindahkan ke : " + player.transform.position);
                    StartCoroutine(LoadingScreenUI.Instance.SetLoadingandTimer(false));
                    SmoothCameraFollow.Instance.EnterHouse(false);
                }


            }

        }
    }
}
