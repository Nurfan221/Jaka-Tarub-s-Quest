using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class KuburanInteractable : Interactable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public Sprite[] spritesKuburan;
    public Sprite[] notifikasiSprites;
    public int jedaKuburanKotor;
    public int jedaMembersihkanKuburan;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer notifikasiSpritesRenderer;
    public GameObject notifikasi;
    public float frameRate = 0.3f; // Waktu per frame (kecepatan animasi)
    private int currentFrame = 0; // Indeks frame saat ini
    public ItemData itemInteractable;


    //Logika kuburan kotor
    public bool isKotor;
    public int currentDayKotor;
    public int countDayKotor;

    private PlayerController stats;
    private void Awake()
    {


        // Ambil "Papan Pengumuman" dari Otak dan simpan ke jalan pintas kita.
        if (PlayerController.Instance != null)
        {
            stats = PlayerController.Instance;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }
    }
    void Start()
    {
        RandomCurrentDayKotor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void Interact()
    {


        //// Mulai animasi
        //StartAnimationOpen();
        // Panggil OpenStorage setelah animasi selesai
        if (stats.equipped1 == true )
        {
            // Jika ya, fokus pada item di slot pertama [0]
            ItemData itemDiSlotPertama = stats.equippedItemData[0];

            // Cek apakah namanya cocok
            if (itemDiSlotPertama.itemName == itemInteractable.itemName && isKotor)
            {
                // Lakukan aksi untuk slot pertama...
                Debug.Log("Item di slot pertama cocok! Membersihkan Makam ...");
                StartCoroutine(MembersihkanMakam());
            }
        }
        else
        {
            // Fokus pada item di slot kedua [1]
            ItemData itemDiSlotKedua = stats.equippedItemData[1];

            // Cek apakah namanya cocok
            if (itemDiSlotKedua.itemName == itemInteractable.itemName && isKotor)
            {
                Debug.Log("Item di slot pertama cocok! Mengisi ulang...");
                StartCoroutine(MembersihkanMakam());

            }
        }
       
    }


    // Di dalam skrip KuburanInteractable.cs
    public void kondisiKuburanKotor()
    {

        isKotor = true;
        notifikasi.SetActive(true);
        spriteRenderer.sprite = spritesKuburan[1]; // Ganti ke sprite kotor
        StartCoroutine(PlayNotifikationAnimation()); // Mulai animasi notifikasi

        promptMessage = "Bersihkan Kuburan";
    }

    private IEnumerator PlayNotifikationAnimation()
    {
        while (isKotor) // Hanya berjalan ketika kuburan kotor
        {
            if (notifikasiSprites.Length > 0)
            {
                notifikasiSpritesRenderer.sprite = notifikasiSprites[currentFrame];
                currentFrame = (currentFrame + 1) % notifikasiSprites.Length;
            }
            yield return new WaitForSeconds(frameRate);
        }
    }


    public IEnumerator MembersihkanMakam()
    {
        EnvironmentBehavior environmentBehavior = GetComponent<EnvironmentBehavior>();

        if (PlayerController.Instance.stamina > 0)
        {
            // Tampilkan loading screen sementara proses pembersihan
            StartCoroutine(LoadingScreenUI.Instance.SetLoadingandTimer(false)); // Ini akan memanggil PlayLoadingAnimation() dan PauseGame()
                                                               // Beri waktu tunggu minimal untuk pengalaman pengguna yang baik
            yield return new WaitForSecondsRealtime(1.5f); // Jeda minimal 1.5 detik agar tips terbaca
            LoadingScreenUI.Instance.HideLoading();

            // Ubah status kuburan menjadi bersih
            isKotor = false;
            spriteRenderer.sprite = spritesKuburan[0];  // Set sprite menjadi kuburan bersih
            notifikasi.SetActive(false);  // Matikan animasi notifikasi

            // Reset nilai currentDayKotor setelah kuburan dibersihkan
            RandomCurrentDayKotor();

            // Update jumlah kuburan yang dibersihkan
            //environmentKuburanManager.jumlahdiBersihkan++;
            MainEnvironmentManager.Instance.kuburanManager.UpdateStatusJob();
            QuestManager.Instance.UpdateCleanupQuest(environmentBehavior.nameEnvironment, EnvironmentType.Kuburan);
        }


        promptMessage = "Kuburan";

        //environmentKuburanManager.player_Health.SpendStamina(useStamina);
        //environmentKuburanManager.player_Health.ApplyFatigue(useStamina);
    }


    public void RandomCurrentDayKotor()
    {
        // Deklarasikan variabel hanya sekali di luar loop
        float randomCurrentDayKotor = UnityEngine.Random.Range(4f, 10f);

        // Menghasilkan nilai yang berbeda jika nilai random sama dengan currentDayKotor
        do
        {
            randomCurrentDayKotor = UnityEngine.Random.Range(4f, 10f);
        } while (randomCurrentDayKotor == currentDayKotor);

        // Bulatkan nilai random
        int nilaiRandomdiBulatkan = (int)Math.Round(randomCurrentDayKotor);

        // Set nilai random sebagai currentDayKotor
        currentDayKotor = nilaiRandomdiBulatkan;
    }


}
