using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class BuffScrollController : MonoBehaviour
{
    public RectTransform contentTransform; // Referensi ke Content
    public float scrollSpeed = 3f; // Kecepatan scroll

    private int totalBuffs; // Total buff yang ada
    public int maxBuffsInView = 3; // Jumlah maksimal buff yang bisa dilihat (misalnya 5)
    private bool isScrolling = false;
    [Header("logika buff")]
    public bool isBuffDamage;
    public bool isBuffProtection;
    public bool buffSprint;
    public Image[] imageBuff;
    

    void Start()
    {
        totalBuffs = contentTransform.childCount; // Total buff berdasarkan jumlah child dalam Content
        //if (totalBuffs > maxBuffsInView)
        //{
        //    StartScrolling();
        //}
    }

    void StartScrolling()
    {
        isScrolling = true;
        float targetX = Mathf.Max(0, totalBuffs - maxBuffsInView); // Tentukan berapa banyak yang perlu digeser
        StartCoroutine(ScrollContent(targetX));
    }

    public IEnumerator ScrollContent(float targetX)
    {
        // Mendapatkan posisi awal
        float startX = contentTransform.anchoredPosition.x;
        float endX = targetX * contentTransform.rect.width / totalBuffs; // Hitung posisi akhir untuk scroll
        float timeElapsed = 0f;

        // Lakukan scroll selama waktu tertentu
        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime * scrollSpeed;
            contentTransform.anchoredPosition = new Vector2(Mathf.Lerp(startX, endX, timeElapsed), contentTransform.anchoredPosition.y);
            yield return null;
        }
    }

    public void UpdateBuffs(int newBuffCount)
    {
        totalBuffs = newBuffCount;
        if (totalBuffs > maxBuffsInView)
        {
            StartScrolling();
        }
    }

    public void GetBuff(int valueBuff)
    {
        switch(valueBuff)
        {
            case 0:
                Debug.Log("Buff Damage");
                break;
            case 1:
                Debug.Log("Buff Protection");
                break;
            case 2:
                Debug.Log("Buff Sprint");
                break;
            default:
                break;
        }
    }
}
