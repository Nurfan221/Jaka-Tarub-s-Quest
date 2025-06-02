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
    public Image[] imageBuff;
    [Header("buff damage")]
    public int jumlahBuffDamage;
    public float waktuActiveBuffDamage;
    public float sisaWaktuActiveBuffDamage;
    public bool isBuffDamage;
   

    [Header("buff protection")]
    public int jumlahBuffProtection;
    public float waktuActiveBuffProtection;
    public float sisaWaktuActiveBuffProtection;
    public bool isBuffProtection;

    [Header("buff sprint")]
    public int jumlahBuffSprint;
    public float waktuActiveBuffSprint;
    public float sisaWaktuActiveBuffSprint;
    public bool isBuffSprint;


    [Header("Daftar Hubungan")]
    [SerializeField] Player_Health player_Health;


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

    public void GetBuff(Item item)
    {
        // Mengecek Buff Damage
        if (item.buffDamage > 0)
        {
            isBuffDamage = true;
            jumlahBuffDamage = item.buffDamage;
            waktuActiveBuffDamage = item.waktuBuffDamage;
            imageBuff[0].gameObject.SetActive(true);
        }

        // Mengecek Buff Protection
        if (item.buffProtection > 0)
        {
            isBuffProtection = true;
            jumlahBuffProtection = item.buffProtection;
            waktuActiveBuffProtection = item.waktuBuffProtection;
            imageBuff[1].gameObject.SetActive(true);
        }

        // Mengecek Buff Sprint
        if (item.buffSprint > 0)
        {
            isBuffSprint = true;
            jumlahBuffSprint = item.buffSprint;
            waktuActiveBuffSprint = item.waktuBuffSprint;
            imageBuff[2].gameObject.SetActive(true);
        }

        // Mengecek Heal
        if (item.countHeal > 0)
        {
            Debug.Log("Heal effect triggered");
            player_Health.Heal(item.countHeal, item.countStamina); // Menyembuhkan player
        }

        

        // Jika ada efek Buff yang aktif, proses yang sesuai
        if (isBuffDamage)
        {
            Debug.Log("Buff Damage applied");
        }

        if (isBuffProtection)
        {
            Debug.Log("Buff Protection applied");
        }

        if (isBuffSprint)
        {
            Debug.Log("Buff Sprint applied");
        }
    }

    // Fungsi untuk mengurangi waktu sisa aktif buff
    public void UpdateBuffTime()
    {
        // Pengecekan untuk Buff Damage
        if (isBuffDamage)
        {
            sisaWaktuActiveBuffDamage++;
            if (sisaWaktuActiveBuffDamage >= waktuActiveBuffDamage)
            {
                isBuffDamage = false;
                sisaWaktuActiveBuffDamage = 0;
                waktuActiveBuffDamage = 0;
                imageBuff[0].gameObject.SetActive(false); // Menyembunyikan gambar Buff Damage
            }
        }

        // Pengecekan untuk Buff Protection
        if (isBuffProtection)
        {
            sisaWaktuActiveBuffProtection++;
            if (sisaWaktuActiveBuffProtection >= waktuActiveBuffProtection)
            {
                isBuffProtection = false;
                sisaWaktuActiveBuffProtection = 0;
                waktuActiveBuffProtection = 0;
                imageBuff[2].gameObject.SetActive(false); // Menyembunyikan gambar Buff Protection
            }
        }

        // Pengecekan untuk Buff Sprint
        if (isBuffSprint)
        {
            sisaWaktuActiveBuffSprint++;
            if (sisaWaktuActiveBuffSprint >= waktuActiveBuffSprint)
            {
                isBuffSprint = false;
                sisaWaktuActiveBuffSprint = 0;
                waktuActiveBuffSprint = 0;
                imageBuff[3].gameObject.SetActive(false); // Menyembunyikan gambar Buff Sprint
            }
        }
    }


}
