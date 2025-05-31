using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HoedTileData
{
    public Vector3Int tilePosition;
    public int hoedTime;
    public GameObject plantStatus;
    public bool watered;

    // Konstruktor untuk menyimpan posisi tile dan waktu cangkul
    public HoedTileData(Vector3Int tilePos, int time)
    {
        tilePosition = tilePos;
        hoedTime = time;
        this.watered = false;
    }

    
}

