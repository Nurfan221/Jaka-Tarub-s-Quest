using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UpgradeToolsUI : MonoBehaviour
{
    [Header("Database")]
    public PlayerController stats;
    public UpgradeToolsDatabaseSO upgradeToolsDatabaseSO;
    public UIShaker uiShaker;
    public UpgradeToolsInteractable upgradeToolsInteractable;
    public Dialogues startUpgradeDialogue;

    [Header("UI Setting")]
    public Transform inventoryContent;
    public Transform inventorySlotTemplate;

    public Transform recipeContent;
    public Transform recipeSlotTemplate;

    public Transform upgradeSlotTemplate;

    //UI UPGRADE TOOLS 
    public Button upgradeButton;
    public Transform deskripsiTemplate;
    public Transform resultRequiredTemplate;
    public Transform toolRequiredTemplate;
    public Transform itemRequiredTemplate;
    public Transform constRequiredTemplate;
    public Transform errorUI;
    public string errorMessage = "Syarat upgrade belum terpenuhi!";

    public Button closebutton;

    [Header("Animation")]
    public bool isDropping = false;
    public float startY = 700f;   // posisi awal UI saat muncul (mis. di atas)
    public float targetY = 0f;    // posisi akhir saat tampil (mis. 0)
    public float smoothTime = 0.18f;

    private float downVelocity = 0f; // untuk Drop/Down
    private float upVelocity = 0f;   // untuk Up (naik)
    private Coroutine currentAnim = null;

    private bool isClosing = false;
    private Coroutine errorCoroutine; // simpan coroutine yang sedang jalan
    private bool isErrorShowing = false;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (closebutton != null)
        {
            closebutton.onClick.AddListener(CloseUpgradeToolsUI);
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(TryUpgrade);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TryUpgrade()
    {
         //canUpgrade = (itemRequired != null && itemRequired.count > 0);

        if (!upgradeToolsInteractable.canUpgrade)
        {
            Debug.Log("Syarat upgrade belum terpenuhi!");
            upgradeButton.GetComponent<UIShaker>().Shake(); // Efek guncangan!
            CanvasGroup errorCanvasGroup = errorUI.GetComponent<CanvasGroup>();
            if (errorCanvasGroup != null)
            {
                StartCoroutine(ShowErrorUI(2f, 0.5f));
            }
            return;
        }

        // Jika berhasil upgrade
        DoUpgrade();
    }

    public IEnumerator ShowErrorUI(float showDuration = 3f, float fadeDuration = 0.2f)
    {
        // Jika sudah ada animasi error yang sedang jalan, hentikan dulu
        if (errorCoroutine != null)
        {
            StopCoroutine(errorCoroutine);
            errorCoroutine = null;
        }

        errorCoroutine = StartCoroutine(ShowErrorUICoroutine(showDuration, fadeDuration));
        yield return null;
    }

    private IEnumerator ShowErrorUICoroutine(float showDuration, float fadeDuration)
    {
        isErrorShowing = true;

        CanvasGroup errorCanvasGroup = errorUI.GetComponent<CanvasGroup>();
        TMP_Text errorText = errorUI.GetChild(0).GetComponent<TMP_Text>();
        errorText.text = errorMessage;

        errorUI.gameObject.SetActive(true);
        errorCanvasGroup.alpha = 0f;

        // Fade In
        float timer = 0f;
        while (timer < fadeDuration)
        {
            // Kalau UI utama tiba-tiba ditutup, hentikan animasi aman
            if (!errorUI.gameObject.activeInHierarchy)
            {
                ResetErrorUI();
                yield break;
            }

            timer += Time.deltaTime;
            errorCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        errorCanvasGroup.alpha = 1f;

        // Tahan beberapa detik
        yield return new WaitForSeconds(showDuration);

        // Fade Out
        timer = 0f;
        while (timer < fadeDuration)
        {
            if (!errorUI.gameObject.activeInHierarchy)
            {
                ResetErrorUI();
                yield break;
            }

            timer += Time.deltaTime;
            errorCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        errorCanvasGroup.alpha = 0f;

        ResetErrorUI();
    }

    private void ResetErrorUI()
    {
        if (errorUI != null)
            errorUI.gameObject.SetActive(false);

        isErrorShowing = false;
        if (errorCoroutine != null)
        {
            StopCoroutine(errorCoroutine);
            errorCoroutine = null;
        }
    }

    private bool IsUpgradeRequirementIncomplete()
    {
        bool isToolEmpty = upgradeToolsInteractable.itemToUpgrade == null || string.IsNullOrEmpty(upgradeToolsInteractable.itemToUpgrade.itemName) || upgradeToolsInteractable.itemToUpgrade.count <= 0;
        bool isMaterialEmpty = upgradeToolsInteractable.itemRequired == null || string.IsNullOrEmpty(upgradeToolsInteractable.itemRequired.itemName) || upgradeToolsInteractable.itemRequired.count <= 0;

        return isToolEmpty || isMaterialEmpty;
    }



    public void OpenUpgradeToolsUI(UpgradeToolsInteractable upgradeToolsInteractable)
    {
        stats = PlayerController.Instance;
        upgradeToolsDatabaseSO = DatabaseManager.Instance.upgradeToolsDatabase;
        uiShaker = upgradeButton.GetComponent<UIShaker>();
        this.upgradeToolsInteractable = upgradeToolsInteractable;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");
        GameController.Instance.ShowPersistentUI(false);
        gameObject.SetActive(true);
        RefreshSlots();
        RefreshRecipeSlots();
    }

    public void CloseUpgradeToolsUI()
    {
        if (isDropping)
        {
            StartCoroutine(CloseUpgradeCoroutine());
        }else
        {
            //  Kembalikan UI utama
            GameController.Instance.ShowPersistentUI(true);

            //  Matikan UI setelah animasi selesai
            gameObject.SetActive(false);

            isClosing = false;
        }
            
     


    }

    private IEnumerator CloseUpgradeCoroutine()
    {

        isDropping = false;
        ResetErrorUI();
        PlayUp(); // jalankan animasi naik

        // Tunggu hingga animasi selesai (sesuaikan durasi dengan animasi PlayUp)
        yield return new WaitForSeconds(0.5f);

        //  Reset posisi ke posisi awal (startY)
        RectTransform uiObject = upgradeSlotTemplate.GetComponent<RectTransform>();
        Vector2 resetPos = uiObject.anchoredPosition;
        resetPos.y = startY;
        uiObject.anchoredPosition = resetPos;

        // Reset nilai awal
        if (!upgradeToolsInteractable.startedUpgrade)
        {
            upgradeToolsInteractable.itemToUpgrade = null;
            upgradeToolsInteractable.itemRequired = null;
            upgradeToolsInteractable.resultItemUpgrade = null;
        }
        //  Kembalikan UI utama
        GameController.Instance.ShowPersistentUI(true);

        //  Matikan UI setelah animasi selesai
        gameObject.SetActive(false);
        if (upgradeToolsInteractable.startedUpgrade)
        {
            DialogueSystem.Instance.HandlePlayDialogue(startUpgradeDialogue);
        }
        isClosing = false;
    }

    public void RefreshSlots()
    {
        Debug.Log("Merefresh slot upgrade tools UI...");
        foreach (Transform child in inventoryContent)
        {
            if (child == inventorySlotTemplate)
                continue;
            Destroy(child.gameObject);
        }

        //  Mengisi slot inventory baru (Kode Anda sudah benar)
        foreach (ItemData itemData in stats.inventory)
        {
            Item item = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);

            if (item.IsInCategory(ItemCategory.tools) || item.IsInCategory(ItemCategory.Ingot)) // Tampilkan kategory tools
            {
                Debug.Log("Menambahkan item ke inventory upgrade tools UI: " + item.itemName);
                Transform theItem = Instantiate(inventorySlotTemplate, inventoryContent);
                theItem.name = item.itemName;
                theItem.gameObject.SetActive(true);

                theItem.GetChild(0).GetComponent<Image>().sprite = item.sprite;
                theItem.GetChild(1).GetComponent<TMP_Text>().text = itemData.count.ToString();
                theItem.GetComponent<DragCook>().itemName = item.itemName;

                ItemData currentItemData = itemData;
                //theItem.GetComponent<Button>().onClick.AddListener(() => OnClickItemInInventory());
            }else
            {
                Debug.Log("Item " + item.itemName + " bukan kategori tools, tidak ditampilkan di inventory upgrade tools UI.");
            }

        }

       
    }

    public void OnClickItemInInventory(UpgradeToolsDatabase upgradeToolsDatabase)
    {
        StartCoroutine(HandleClickInventory(upgradeToolsDatabase));
    }

    private IEnumerator HandleClickInventory(UpgradeToolsDatabase upgradeToolsDatabase)
    {
        if (!isDropping)
        {
            isDropping = true;
            UpdateUIUpgrade(upgradeToolsDatabase);

            // Tambahkan jeda sebelum animasi turun
            yield return new WaitForSeconds(0.3f);

            PlayDown();
        }
        else
        {
            PlayUp();
            // Tambahkan jeda sebelum memperbarui UI
            yield return new WaitForSeconds(0.3f);

            UpdateUIUpgrade(upgradeToolsDatabase);

            // Tambahkan jeda sebelum animasi naik
            yield return new WaitForSeconds(0.2f);
            PlayDown();

        }
    }


    public void UpdateUIUpgrade(UpgradeToolsDatabase upgradeToolsDatabase)
    {
        upgradeToolsInteractable.upgradeToolsDatabase = upgradeToolsDatabase;
        GetItemForUpgrade();

        //  Update UI dasar 
        //Deskripsi text
        TMP_Text deskripsiText = deskripsiTemplate.GetChild(0).GetComponent<TMP_Text>();
        deskripsiText.text = $"Upgrade {upgradeToolsDatabase.itemToolRequired.itemName} menjadi {upgradeToolsDatabase.itemToolResult.itemName} Dibutuhkan Waktu Selama {upgradeToolsDatabase.upgradeTimeInDays} Hari";

        // template untuk menampilkan hasil upgrade
        Image resultRequiredImage = resultRequiredTemplate.GetChild(0).GetComponent<Image>();
        TMP_Text resultRequiredCountText = resultRequiredTemplate.GetChild(1).GetComponent<TMP_Text>();

        resultRequiredImage.sprite = upgradeToolsDatabase.itemToolResult.sprite;
        resultRequiredCountText.text = upgradeToolsDatabase.itemToolResult.itemName;

        // template untuk menampilkan alat yang di upgrade
        Image toolRequiredImage = toolRequiredTemplate.GetChild(0).GetComponent<Image>();
        TMP_Text toolNameText = toolRequiredTemplate.GetChild(1).GetComponent<TMP_Text>();
        TMP_Text countToolRequired = toolRequiredTemplate.GetChild(2).GetComponent<TMP_Text>();
        toolRequiredImage.sprite = upgradeToolsDatabase.itemToolRequired.sprite;
        toolNameText.text = upgradeToolsDatabase.itemToolRequired.itemName;
        countToolRequired.text = "1";


        // template untuk menampilkan item yang dibutuhkan
        Image itemRequiredImage = itemRequiredTemplate.GetChild(0).GetComponent<Image>();
        TMP_Text itemUpgradeRequiredName = itemRequiredTemplate.GetChild(1).GetComponent<TMP_Text>();
        TMP_Text itemUpgradeRequiredCount = itemRequiredTemplate.GetChild(2).GetComponent<TMP_Text>();
        itemRequiredImage.sprite = upgradeToolsDatabase.itemUpgradeRequired.sprite;
        itemUpgradeRequiredName.text = upgradeToolsDatabase.itemUpgradeRequired.itemName;
        itemUpgradeRequiredCount.text = upgradeToolsDatabase.itemUpgradeRequiredCount.ToString();

        // template untuk menampilkan biaya upgrade
        CanvasGroup constCanvasGroup = constRequiredTemplate.GetComponent<CanvasGroup>();
        TMP_Text upgradeCostText = constRequiredTemplate.GetChild(0).GetComponent<TMP_Text>();


        // Jika item tidak ada, atau jumlahnya kurang, maka buat pudar
        if (upgradeToolsInteractable.itemToUpgrade == null || upgradeToolsInteractable.itemToUpgrade.count < 1)
        {
            SetImageAlpha(toolRequiredImage, 0.4f); // pudar
            errorMessage = "Alat untuk di upgrade tidak tersedia!";
        }
        else
        {
            SetImageAlpha(toolRequiredImage, 1f); // normal
        }

        if (upgradeToolsInteractable.itemRequired == null || upgradeToolsInteractable.itemRequired.count < upgradeToolsDatabase.itemUpgradeRequiredCount)
        {
            SetImageAlpha(itemRequiredImage, 0.4f);
            errorMessage = "Bahan upgrade tidak mencukupi!";
        }
        else
        {
            SetImageAlpha(itemRequiredImage, 1f);
        }

   

        if (IsUpgradeRequirementIncomplete())
        {
            Debug.LogWarning("Upgrade tidak bisa dilakukan: item belum lengkap!");
            upgradeToolsInteractable.canUpgrade = false;
        }
        else
        {
            if (GameEconomy.Instance.coins >= upgradeToolsInteractable.upgradeCostAmount)
            {
                upgradeToolsInteractable.canUpgrade = true;
                Debug.Log("Uang tidak cukup untuk upgrade.");
                constCanvasGroup.alpha = 1f;

            }
            else
            {
                upgradeToolsInteractable.canUpgrade = false;
                Debug.LogWarning("Upgrade tidak bisa dilakukan: uang tidak cukup!");

                Debug.Log("Uang cukup untuk upgrade.");
                constCanvasGroup.alpha = 0.4f;
                errorMessage = "Koin anda tidak cukup untuk upgrade!";
            }
            Debug.Log("Upgrade bisa dilakukan: semua item lengkap.");
        }
    }


    private void SetImageAlpha(Image image, float alpha)
    {
        if (image == null) return;
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }

    public void PlayDown()
    {
        // hentikan animasi sebelumnya kalau ada
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(DownAnimation());
    }

    // panggil ini untuk menaikkan UI (sembunyi)
    public void PlayUp()
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(UpAnimation());
    }
    public void RefreshRecipeSlots()
    {
        foreach (Transform child in recipeContent)
        {
            if (child == recipeSlotTemplate)
                continue;
            Destroy(child.gameObject);
        }

        //  Mengisi slot inventory baru (Kode Anda sudah benar)
        foreach (UpgradeToolsDatabase itemData in upgradeToolsDatabaseSO.upgradeTools)
        {


            if (itemData.itemToolResult.IsInCategory(ItemCategory.tools)) // Tampilkan kategory tools
            {
                Debug.Log("Menambahkan item ke inventory upgrade tools UI: " + itemData.itemToolResult.itemName);
                Transform theItem = Instantiate(recipeSlotTemplate, recipeContent);
                theItem.name = itemData.itemToolResult.itemName;
                theItem.gameObject.SetActive(true);

                theItem.GetChild(0).GetComponent<Image>().sprite = itemData.itemToolResult.sprite;

                //ItemData currentItemData = itemData;
                theItem.GetComponent<Button>().onClick.AddListener(() => OnClickItemInInventory(itemData));

            }
            else
            {
                Debug.Log("Item " + itemData.itemToolResult.itemName + " bukan kategori tools, tidak ditampilkan di inventory upgrade tools UI.");
            }

        }
    }



    public IEnumerator DownAnimation()
    {
        RectTransform uiObject = upgradeSlotTemplate.GetComponent<RectTransform>();
        // pastikan aktif dan pos di startY
        uiObject.gameObject.SetActive(true);

        Vector2 pos = uiObject.anchoredPosition;
        pos.y = startY;
        uiObject.anchoredPosition = pos;

        // turun ke targetY
        while (Mathf.Abs(uiObject.anchoredPosition.y - targetY) > 0.5f)
        {
            float newY = Mathf.SmoothDamp(
                uiObject.anchoredPosition.y,
                targetY,
                ref downVelocity,
                smoothTime
            );

            uiObject.anchoredPosition = new Vector2(uiObject.anchoredPosition.x, newY);
            yield return null;
        }

        uiObject.anchoredPosition = new Vector2(uiObject.anchoredPosition.x, targetY);
        downVelocity = 0f;
        currentAnim = null;
    }

    public IEnumerator UpAnimation()
    {
        RectTransform uiObject = upgradeSlotTemplate.GetComponent<RectTransform>();

        // mulai dari posisi target (mis. sudah di 0)
        Vector2 pos = uiObject.anchoredPosition;
        pos.y = targetY;
        uiObject.anchoredPosition = pos;

        // naik ke startY
        while (Mathf.Abs(uiObject.anchoredPosition.y - startY) > 0.5f)
        {
            float newY = Mathf.SmoothDamp(
                uiObject.anchoredPosition.y,
                startY,
                ref upVelocity,
                smoothTime
            );

            uiObject.anchoredPosition = new Vector2(uiObject.anchoredPosition.x, newY);
            yield return null;
        }

        uiObject.anchoredPosition = new Vector2(uiObject.anchoredPosition.x, startY);
        upVelocity = 0f;

        // jika ingin menyembunyikan setelah keluar layar, uncomment:
        // uiObject.gameObject.SetActive(false);

        currentAnim = null;
    }
    public void GetItemForUpgrade()
    {
        // ambil referensi database
        var db = upgradeToolsInteractable.upgradeToolsDatabase;

        // buat template lookup (tidak mengubah inventory di sini)
        ItemData itemDataToolTemplate = new ItemData(
            db.itemToolRequired.itemName,
            1,
            ItemQuality.Normal,
            db.itemToolRequired.health
        );

        ItemData itemDataRequiredTemplate = new ItemData(
            db.itemUpgradeRequired.itemName,
            db.itemUpgradeRequiredCount,
            ItemQuality.Normal,
            db.itemUpgradeRequired.health
        );

        // set result template & meta
        upgradeToolsInteractable.resultItemUpgrade = new ItemData(
            db.itemToolResult.itemName,
            1,
            ItemQuality.Normal,
            db.itemToolResult.health
        );

        upgradeToolsInteractable.upgradeCostAmount = db.upgradeCost;
        upgradeToolsInteractable.upgradeTime = TimeManager.Instance.date + db.upgradeTimeInDays;

        // reset referensi
        upgradeToolsInteractable.itemToUpgrade = null;
        upgradeToolsInteractable.itemRequired = null;

        // cari di inventory (jangan mengubah count di sini)
        foreach (ItemData itemData in stats.inventory)
        {
            if (itemData.itemName == itemDataToolTemplate.itemName)
            {
                upgradeToolsInteractable.itemToUpgrade = itemData; // referensi ke inventory item
            }
            else if (itemData.itemName == itemDataRequiredTemplate.itemName)
            {
                upgradeToolsInteractable.itemRequired = itemData; // referensi ke inventory item
                                                                  // jangan ubah itemRequired.count di sini!
            }
        }
    }

    public void DoUpgrade()
    {
        Debug.Log("[Upgrade] Melakukan upgrade alat...");
        GetItemForUpgrade();

        // Validasi eksistensi bahan & jumlah
        var db = upgradeToolsInteractable.upgradeToolsDatabase;
        if (upgradeToolsInteractable.itemToUpgrade == null)
        {
            Debug.LogWarning("[Upgrade] Item alat untuk upgrade tidak ditemukan di inventory!");
            upgradeButton.GetComponent<UIShaker>()?.Shake();
            return;
        }

        if (upgradeToolsInteractable.itemRequired == null ||
            upgradeToolsInteractable.itemRequired.count < db.itemUpgradeRequiredCount)
        {
            Debug.LogWarning("[Upgrade] Item bahan upgrade tidak cukup!");
            upgradeButton.GetComponent<UIShaker>()?.Shake();
            return;
        }

        // Kurangi jumlah item bahan di inventory (ini mengubah stats.inventory karena itemRequired adalah referensi)
        upgradeToolsInteractable.itemRequired.count -= db.itemUpgradeRequiredCount;
        if (upgradeToolsInteractable.itemRequired.count <= 0)
        {
            // Hapus item dari inventory
            stats.inventory.Remove(upgradeToolsInteractable.itemRequired);
            Debug.Log("[Upgrade] Bahan upgrade habis, dihapus dari inventory.");
            // setelah remove jangan lagi memakai referensi lama untuk count
        }

        // Kurangi alat yang di-upgrade (ambil 1)
        upgradeToolsInteractable.itemToUpgrade.count -= 1;
        if (upgradeToolsInteractable.itemToUpgrade.count <= 0)
        {
            stats.inventory.Remove(upgradeToolsInteractable.itemToUpgrade);
            Debug.Log("[Upgrade] Alat yang di-upgrade dihapus dari inventory sementara (sedang di-upgrade).");
        }

        // Buat salinan item yang sedang di-upgrade agar inventory terpisah dari "in-progress"
        upgradeToolsInteractable.itemToUpgrade = new ItemData(
            db.itemToolRequired.itemName,
            1,
            upgradeToolsInteractable.itemToUpgrade != null ? upgradeToolsInteractable.itemToUpgrade.quality : ItemQuality.Normal,
            db.itemToolRequired.health
        );

        // Deduct biaya koin (pakai sistem ekonomi)
        if (!GameEconomy.Instance.SpendMoney(upgradeToolsInteractable.upgradeCostAmount))
        {
            Debug.LogWarning("[Upgrade] Gagal mengambil biaya upgrade (koin tidak cukup)!");
            // jika gagal bayar seharusnya rollback perubahan di inventory (tambah kembali)
            // di implementasi sederhana di bawah kita akan rollback:
            RollbackUpgradeMaterial(db);
            return;
        }

        // set started flag dan schedule complete
        upgradeToolsInteractable.startedUpgrade = true;
        // TimeManager.Instance.ScheduleAction(upgradeToolsInteractable.upgradeTime, CompleteUpgrade);


        CloseUpgradeToolsUI();

        Debug.Log($"[Upgrade] Proses upgrade dimulai! Akan selesai di hari {upgradeToolsInteractable.upgradeTime}");
    }

    // helper untuk rollback jika biaya tidak bisa dipotong
    private void RollbackUpgradeMaterial(UpgradeToolsDatabase db)
    {
        // kembalikan itemRequired
        var requiredName = db.itemUpgradeRequired.itemName;
        var toolName = db.itemToolRequired.itemName;

        // cari kembali di inventory, jika ada tambahkan count, jika tidak buat ulang
        ItemData invRequired = stats.inventory.Find(i => i.itemName == requiredName);
        if (invRequired != null)
            invRequired.count += db.itemUpgradeRequiredCount;
        else
            stats.inventory.Add(new ItemData(requiredName, db.itemUpgradeRequiredCount, ItemQuality.Normal, db.itemUpgradeRequired.health));

        // kembalikan alat yang dipakai
        ItemData invTool = stats.inventory.Find(i => i.itemName == toolName);
        if (invTool != null)
            invTool.count += 1;
        else
            stats.inventory.Add(new ItemData(toolName, 1, ItemQuality.Normal, db.itemToolRequired.health));
    }
    


}
