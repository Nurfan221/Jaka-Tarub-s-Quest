using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.Experimental.GraphView;
using System.Collections;
using Unity.VisualScripting; // Tambahkan ini!

public class MiniGameHewanUI : MonoBehaviour
{
    [System.Serializable]
    public class Rintangan
    {
        public string rintangan;
        public Transform[] locationRintangan;
    }

    public Rintangan[] rintangan;
    public GameObject[] dropitems;
    [SerializeField] MiniGameHewanUI miniGameHewanUI;
    [SerializeField] Player_Inventory playerInventory;
    [SerializeField] QuestManager questManager;
    [SerializeField] Player_Movement playerMovement;
    public Rigidbody2D playerRb; // Tambahkan Rigidbody2D
    public RectTransform playerTransform;

    public Image[] health;
    public Sprite healthImageNull;
    public Sprite healthImage;
    public Image finish;
    public Sprite[] animalIdle;
    public float frameRate; // Waktu per frame (kecepatan animasi)
    private int currentFrame = 0; // Indeks frame saat ini
    private Coroutine animationCoroutine;


    public Image gameOver;
    public Image winObjeck;
    [SerializeField] Transform ContentGO;
    [SerializeField] Transform SlotTemplate;
    public GameObject animal;

    [Header("Pergerakan")]
    public Image imageSprint;
    public Image playerImage;
    private Coroutine currentAnimation;
    public Sprite[] walkUpSprites;
    public Sprite[] walkDownSprites;
    public Sprite[] walkLeftSprites;
    public Sprite[] walkRightSprites;
    public Sprite[] takeDamage;
    public Sprite idleSprites;

    public float moveSpeed = 100f; // Kecepatan gerak
    public float sprintSpeed = 200f;
    public int countDownSpeed = 5;
    public float sprintMultiplier = 1.5f; // Kecepatan tambahan saat sprint
    private float currentSpeed;
    private float cooldownSprint = 10f;
    private Vector2 moveDirection = Vector2.zero;
    private bool isSprint = false;
    private Vector2 positionPlayer;
    private float cooldownProgress = 0f;
    private bool isCooldownActive = false;
    public bool isEvent;


    [Header("Button")]
    public Button btnClose, btnUp, btnDown, btnLeft, btnRight, btnSprint;

    private bool isMoving = false; // Apakah player sedang bergerak?

    [Header("Lampu")]
    public Image greenLight;
    public Image redLight;
    public bool isGreenLight;
    public int healthIndex = 0;

    void Start()
    {
        RandomizeRintangan();
        if (btnClose != null)
        {
            btnClose.onClick.AddListener(() => Close());
        }
        if (gameOver != null)
        {
            gameOver.gameObject.SetActive(false);
        }




        // Tambahkan event untuk hold button
        AddHoldListener(btnUp, Vector2.up, walkUpSprites);
        AddHoldListener(btnDown, Vector2.down, walkDownSprites);
        AddHoldListener(btnLeft, Vector2.left, walkLeftSprites);
        AddHoldListener(btnRight, Vector2.right, walkRightSprites);

        btnSprint.onClick.AddListener(ToggleSprint);


        currentSpeed = moveSpeed; // Set default speed
        positionPlayer = playerTransform.position;


        //StartCoroutine(ChangeLight());

    }



    void Update()
    {
        if (isMoving)
        {
            Move();

        }

        if (greenLight.enabled && isMoving) // Jika lampu merah menyala dan player bergerak
        {
            StopMoving();
            TakeDamage(takeDamage);
        }
    }
    public void Open(GameObject animalObjek)
    {
        miniGameHewanUI.gameObject.SetActive(true);
        // Pastikan array memiliki ukuran yang sesuai

        animal = animalObjek;
        // Ambil komponen SpriteRenderer dari animal
        SpriteRenderer animalSpriteRenderer = animal.GetComponent<SpriteRenderer>();

        // Cek apakah SpriteRenderer ditemukan
        if (animalSpriteRenderer != null)
        {
            // Atur sprite milik animal ke dalam UI Image finish
            finish.sprite = animalSpriteRenderer.sprite;
        }
        else
        {
            Debug.LogWarning("Animal tidak memiliki komponen SpriteRenderer!");
        }



        AnimalBehavior animalBehavior = animalObjek.GetComponent<AnimalBehavior>();
        isEvent = animalBehavior.isAnimalEvent;
        if (animalBehavior != null)
        {
            dropitems = new GameObject[animalBehavior.dropitems.Length];
            // Simpan item ke dalam dropitems milik MiniGameHewanUI
            for (int i = 0; i < animalBehavior.dropitems.Length; i++)
            {
                dropitems[i] = animalBehavior.dropitems[i];
            }

            animalIdle = new Sprite[animalBehavior.animalIdle.Length];
            for (int i = 0; i < animalIdle.Length; i++)
            {
                animalIdle[i] = animalBehavior.animalIdle[i];
            }
        }

        // Mulai animasi saat UI diaktifkan
        if (animalIdle.Length > 0)
        {
            animationCoroutine = StartCoroutine(PlayAnimalIdleAnimation());
        }


        winObjeck.gameObject.SetActive(false);
        gameOver.gameObject.SetActive(false);



        RandomizeRintangan();
        // Nonaktifkan semua tombol gerak
        btnUp.gameObject.SetActive(true);
        btnDown.gameObject.SetActive(true);
        btnLeft.gameObject.SetActive(true);
        btnRight.gameObject.SetActive(true);
        btnSprint.gameObject.SetActive(true);

        for (int i = 0; i < health.Length; i++)
        {
            health[i].sprite = healthImage;
        }

        healthIndex = 0;


        StartCoroutine(ChangeLight());
        GameController.Instance.ShowPersistentUI(false);
        //GameController.Instance.PauseGame();
        
        playerMovement.movement = Vector2.zero;
        playerMovement.isMoving = false;

    }



    public void Close()
    {
        miniGameHewanUI.gameObject.SetActive(false);
        GameController.Instance.ShowPersistentUI(true);
        //GameController.Instance.ResumeGame();
        playerTransform.position = positionPlayer;

        // Jika ingin cooldown langsung selesai saat ditutup
        StopAllCoroutines();
        cooldownProgress = 0f;
        imageSprint.fillAmount = 1;
        btnSprint.interactable = true;

        // Hentikan animasi saat UI dinonaktifkan
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        // Hapus objek hewan dari dunia game
        if (animal != null)
        {
            //Destroy(animal);
            animal = null; // Hapus referensi agar tidak tersimpan di memori
        }


    }

    private IEnumerator PlayAnimalIdleAnimation()
    {
        while (true) // Loop tanpa batas untuk animasi berulang
        {
            if (animalIdle.Length > 0)
            {
                finish.sprite = animalIdle[currentFrame]; // Ganti sprite pada Image UI
                currentFrame = (currentFrame + 1) % animalIdle.Length; // Pindah ke frame berikutnya
            }
            yield return new WaitForSeconds(frameRate); // Tunggu sebelum mengganti frame berikutnya
        }
    }


    private void Move()
    {
        Vector2 nextPosition = playerRb.position + (moveDirection * currentSpeed * Time.deltaTime);

        // Cek apakah ada tabrakan dengan rintangan
        if (!IsColliding(nextPosition))
        {
            // Jika tidak bertabrakan, gerakkan player
            playerRb.MovePosition(nextPosition);

        }
    }



    private bool IsColliding(Vector2 targetPosition)
    {
        // Konversi posisi UI ke World Space
        Vector2 worldPosition = RectTransformUtility.WorldToScreenPoint(null, targetPosition);

        // Cek tabrakan dengan rintangan
        foreach (var rint in rintangan)
        {
            foreach (Transform lokasi in rint.locationRintangan)
            {
                Collider2D rintanganCollider = lokasi.GetComponent<Collider2D>();

                if (rintanganCollider != null && rintanganCollider.enabled && lokasi.gameObject.activeSelf)
                {
                    if (rintanganCollider.OverlapPoint(worldPosition))
                    {
                        return true; // Ada tabrakan, player tidak bisa lewat
                    }
                }
            }
        }

        //Cek apakah menyentuh Finish
        if (finish != null)
        {
            Collider2D finishCollider = finish.GetComponent<Collider2D>();
            if (finishCollider != null && finishCollider.OverlapPoint(worldPosition))
            {
                Win(); // Panggil fungsi kemenangan

                if (isEvent)
                {
                    //questManager.currentMainQuest.currentQuestState = MainQuest1State.BunuhRusa;
                    //questManager.NextQuestState();

                }
                else
                {
                    // Pastikan 'animal' sudah diatur dari MiniGameHewanUI.Open()
                    if (animal != null && dropitems != null && dropitems.Length > 0)
                    {
                        // Bersihkan quest sebelumnya hanya sekali sebelum menampilkan item
                        foreach (Transform child in ContentGO)
                        {
                            if (child == SlotTemplate) continue;
                            Destroy(child.gameObject);
                        }

                        // Tampilkan semua item drop dari dropitems
                        foreach (var item in dropitems)
                        {
                            SpriteRenderer spriteRenderer = item.GetComponent<SpriteRenderer>();
                            if (spriteRenderer != null)
                            {
                                Sprite itemImage = spriteRenderer.sprite;
                                string itemName = item.name;
                                CreateItemPrefab(itemImage, itemName);
                            }
                        }

                        // Panggil DropItem() dari AnimalBehavior untuk menjatuhkan item
                        AnimalBehavior animalBehavior = animal.GetComponent<AnimalBehavior>();
                        if (animalBehavior != null)
                        {
                            animalBehavior.DropItem();
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Animal atau dropitems tidak valid di MiniGameHewanUI.");
                    }
                }
            }
        }


        return false; // Tidak ada tabrakan
    }


    private void CreateItemPrefab(Sprite itemImage, string itemName)
    {
        // **1️⃣ Duplikasi SlotTemplate**
        Transform imageContent = Instantiate(SlotTemplate, ContentGO);
        imageContent.gameObject.name = itemName;
        imageContent.gameObject.SetActive(true);

        // **2️⃣ Atur Sprite**
        Image image = imageContent.GetComponentInChildren<Image>();
        if (image != null)
        {
            image.sprite = itemImage;
        }
        else
        {
            Debug.LogError("Image tidak ditemukan dalam SlotTemplate!");
        }
    }




    private void ToggleSprint()
    {
        currentSpeed = sprintSpeed;
        btnSprint.interactable = false; // Matikan tombol sprint
        StartCoroutine(CountDownSprint());
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CountDownSprint()
    {
        yield return new WaitForSeconds(countDownSpeed);
        currentSpeed = moveSpeed; // Kembalikan ke speed normal

    }

    private IEnumerator CooldownRoutine()
    {
        isCooldownActive = true; // Tandai bahwa cooldown aktif
        while (cooldownProgress < cooldownSprint)
        {
            cooldownProgress += Time.deltaTime;
            imageSprint.fillAmount = cooldownProgress / cooldownSprint;
            yield return null;
        }

        imageSprint.fillAmount = 1;
        btnSprint.interactable = true;
        isCooldownActive = false; // Tandai cooldown selesai
    }


    private void RandomizeRintangan()
    {
        foreach (var rint in rintangan)
        {
            List<Transform> lokasiTersedia = new List<Transform>(rint.locationRintangan);

            // Reset semua rintangan sebelum mengaktifkan yang baru
            foreach (var lokasi in lokasiTersedia)
            {
                if (lokasi.gameObject.activeSelf)
                {
                    lokasi.gameObject.GetComponent<Collider2D>().enabled = false;
                    lokasi.gameObject.SetActive(false);
                }
            }

            int jumlahAktif = Mathf.Min(2, lokasiTersedia.Count); // Maksimal 2 lokasi aktif

            for (int i = 0; i < jumlahAktif; i++)
            {
                int randomIndex = Random.Range(0, lokasiTersedia.Count);
                Transform lokasi = lokasiTersedia[randomIndex];

                lokasi.gameObject.SetActive(true);
                lokasi.gameObject.GetComponent<Collider2D>().enabled = true; // Aktifkan Collider

                lokasiTersedia.RemoveAt(randomIndex);
            }
        }
    }


    //Menambahkan event listener untuk menahan tombol
    private void AddHoldListener(Button button, Vector2 direction, Sprite[] animationSprites)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();


        // Saat tombol ditekan (hold)
        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { StartMoving(direction, animationSprites); });
        trigger.triggers.Add(pointerDown);


        // Saat tombol dilepas
        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => { StopMoving(); });
        trigger.triggers.Add(pointerUp);
    }



    private void StartMoving(Vector2 direction, Sprite[] animationSprites)
    {
        moveDirection = direction;
        isMoving = true;

        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(PlaySpriteAnimation(animationSprites));
    }

    private void StopMoving()
    {
        isMoving = false;

        if (playerImage != null)
        {
            playerImage.sprite = idleSprites;

        }
    }

    // Coroutine untuk mengganti sprite frame-by-frame
    private IEnumerator PlaySpriteAnimation(Sprite[] animationSprites)
    {
        int index = 0;
        while (isMoving)
        {
            if (animationSprites.Length > 0)
            {
                playerImage.sprite = animationSprites[index];
                index = (index + 1) % animationSprites.Length; // Looping animasi
            }
            yield return new WaitForSeconds(0.1f); // Ubah kecepatan animasi jika perlu
        }
    }

    private IEnumerator ChangeLight()
    {
        while (true)
        {
            if (isGreenLight) // Jika lampu hijau menyala
            {
                greenLight.enabled = true;
                redLight.enabled = false;

                // Acak waktu untuk lampu hijau
                int greenDuration = Random.Range(3, 6);
                //Debug.Log("Lampu Hijau : " + greenDuration + " detik ");

                yield return StartCoroutine(Countdown(greenDuration));
            }
            else // Jika lampu merah menyala
            {
                greenLight.enabled = false;
                redLight.enabled = true;

                // Acak waktu untuk lampu merah
                int redDuration = Random.Range(2, 5);
                //Debug.Log("Lampu Merah : " + redDuration + " detik "); // Diperbaiki dari "Lampu Hijau : redDuration"

                yield return StartCoroutine(Countdown(redDuration));
            }

            // Ganti status lampu setelah hitung mundur selesai
            isGreenLight = !isGreenLight;
        }
    }

    private IEnumerator Countdown(int duration)
    {
        for (int i = duration; i > 0; i--)
        {
            //Debug.Log("Hitung Mundur: " + i);
            yield return new WaitForSeconds(1);
        }
    }


    private void TakeDamage(Sprite[] animationSprites)
    {


        if (healthIndex < health.Length) // Pastikan tidak melebihi batas array
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            currentAnimation = StartCoroutine(PlayTakeDamageAnimation(animationSprites));

            health[healthIndex].sprite = healthImageNull; // Ganti sprite health
            healthIndex++; // Tambah index untuk damage berikutnya


        }

        if (healthIndex >= health.Length) // Jika semua health habis
        {
            Debug.Log("Game Over! Hewan kabur!");
            GameOver(); // Panggil fungsi game over atau reset mini-game
        }
    }

    private IEnumerator PlayTakeDamageAnimation(Sprite[] animationSprites)
    {
        for (int i = 0; i < animationSprites.Length; i++)
        {
            playerImage.sprite = animationSprites[i]; // Ganti sprite setiap frame
            yield return new WaitForSeconds(0.1f); // Atur kecepatan animasi
        }

        // Setelah animasi selesai, kembalikan ke sprite Idle
        playerImage.sprite = idleSprites;
        currentAnimation = null; // Reset animasi aktif
    }

    private void GameOver()
    {
        // Tampilkan UI game over
        gameOver.gameObject.SetActive(true);
        Debug.Log("Mini-game selesai! Player kalah.");

        // Nonaktifkan semua tombol gerak
        btnUp.gameObject.SetActive(false);
        btnDown.gameObject.SetActive(false);
        btnLeft.gameObject.SetActive(false);
        btnRight.gameObject.SetActive(false);
        btnSprint.gameObject.SetActive(false);

        // Hentikan semua coroutine agar lampu tidak berganti lagi
        StopAllCoroutines();

        // Hentikan animasi saat UI dinonaktifkan
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
    }

    private void Win()
    {

        Debug.Log("menang cuyy");
        // Panggil fungsi menang & tampilkan drop items
        winObjeck.gameObject.SetActive(true);

        // Nonaktifkan semua tombol gerak
        btnUp.gameObject.SetActive(false);
        btnDown.gameObject.SetActive(false);
        btnLeft.gameObject.SetActive(false);
        btnRight.gameObject.SetActive(false);
        btnSprint.gameObject.SetActive(false);

        // Hentikan semua coroutine agar lampu tidak berganti lagi
        StopAllCoroutines();

        //Pastikan player tidak bergerak lagi
        isMoving = false;

        // Hentikan animasi saat UI dinonaktifkan
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

    }


}