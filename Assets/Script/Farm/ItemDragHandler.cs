
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;


public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector2 originalPosition; // Menyimpan posisi awal
    // Variabel untuk mengingat tile terakhir yang kita proses
    private Vector3Int lastProcessedTile;
    private bool isDraggingActionActive = false;
    //public FarmTile farmTile;   // Akses ke FarmTile yang punya hoeedTile

    private Transform originalParent; // Untuk menyimpan parent asli item
    public Transform dragLayer; // Assign ini ke layer khusus di canvas untuk menempatkan item yang di-drag
    public GameObject plantPrefab; // Prefab tanaman yang akan ditanam
    public Transform plantsContainer; // Referensi ke GameObject kosong yang menampung tanaman
    public Transform prefabContainer; // referansi ke gameobject kosont yang menampung prefab game objek 
    public Transform treeContainer; // referansi ke gameobject kosont yang menampung prefab pohon
    //public string nameSpriteBar; // Untuk menyimpan nama sprite bar yang di gunakan


    private string itemInDrag;// Ambil nama objek yang di-drag

    [SerializeField] GiveCountContainer giveCountContainer;
    public int itemCount; // Index item yang di berikan


    [SerializeField] FenceBehavior fenceBehavior;

    //public static class TileManager
    //{
    //    // Menyimpan status setiap tile di seluruh permainan
    //    public static Dictionary<Vector3Int, bool> tilePlantStatus = new Dictionary<Vector3Int, bool>();
    //}


    private PlayerController stats;
    public FarmData_SO statsFarm;
    private FarmTile farmTile;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>(); // Mendapatkan Canvas induk
        plantPrefab = DatabaseManager.Instance.plantWorldPrefab; // Ambil prefab tanaman dari DatabaseManager
        farmTile = FarmTile.Instance; // Ambil instance FarmTile



        // Ambil "Papan Pengumuman" dari Otak dan simpan ke jalan pintas kita.
        if (PlayerController.Instance != null)
        {
            stats = PlayerController.Instance;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }

        if (DatabaseManager.Instance != null)
        {
            statsFarm = DatabaseManager.Instance.farmData_SO;
        }
        else
        {
            Debug.LogError("FarmTile.Instance tidak ditemukan saat Awake!");
        }

    }

    private void Start()
    {
        TreesManager environmentManager = MainEnvironmentManager.Instance.pohonManager;
        if (environmentManager != null)
        {
            treeContainer = environmentManager.parentEnvironment; // Atur parent tanaman ke parentEnvironment dari EnvironmentManager
        }
        else
        {
            Debug.LogError("EnvironmentManager tidak ditemukan saat Start!");
        }


    }



    public void UpdateItemCount(int count)
    {
        itemCount = count; // Update itemCount dengan nilai yang diterima
        //Debug.Log("Item Count Updated: " + itemCount);
    }


    public void CekSeed(Vector3Int cellPosition)
    {
        // Cek apakah tile sudah tertanami
        foreach (var lokasihoedTile in farmTile.hoedTilesList)
        {
            if (lokasihoedTile.tilePosition == cellPosition && lokasihoedTile.isPlanted == true)  // Membandingkan dengan Vector3Int
            {
                return;
            }
        }

        // Proses penanaman benih
        bool itemFound = false;

        foreach (ItemData itemData in stats.inventory)
        {
            Item item = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);
            if (item.itemName == itemInDrag && item.IsInCategory(ItemCategory.PlantSeed))
            {
                itemFound = true;
                //plantPrefab = item.prefabItem;

                // Debug.Log("Item ditemukan: " + item.itemName);

                // Menanam benih
                PlantSeed(cellPosition, item);

                // Panggil fungsi untuk menyimpan status tile
                SaveTileStatus(cellPosition, true);

                itemData.count -= 1;

                if (itemData.count <= 0)
                {
                    stats.inventory.Remove(itemData);
                    Debug.Log("Item habis dan dihapus.");
                }

                // Refresh UI
                MechanicController.Instance.HandleUpdateInventory();
                break;
            }
        }
    }







    //logika menanam pohon
    public void CheckPrefabItem(Vector3Int cellPosition)
    {
        bool itemFound = false; // Flag untuk mengecek apakah item ditemukan
        Debug.Log(" fungsi cek Prefab di jalankan");

        foreach (ItemData itemData in stats.inventory)
        {
            Item item = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);

            // Debug.Log("nama itemInDrag  " + itemInDrag);
            // Cek apakah nama item sama dengan itemInDrag dan kategori item adalah Seed
            if (item.itemName == itemInDrag && !item.IsInCategory(ItemCategory.PlantSeed) && item.IsInType(ItemType.ItemPrefab))
            {
                // Debug.Log("nama item.itemname " + item.itemName);
                itemFound = true; // Tandai bahwa item ditemukan
                //int stackItem = item.stackCount; // Ambil jumlah item yang ada
                //plantPrefab = item.prefabItem; // Set plantPrefab sesuai item

                // Debug.Log("Item ditemukan: " + item.itemName + ", Kategori: " + item.categories);



                // Panggil fungsi untuk menanam benih dengan menambahkan parameter growthImages dari item
                if (item.IsInCategory(ItemCategory.TreeSeed))
                {


                    PlantTree(cellPosition, item.itemName, item.growthTime);
                }
                else
                {
                    PlacePrefab(cellPosition, item.itemName, itemData);
                }

                // Kurangi stack item setelah menanam
                itemData.count--;
                //item.stackCount = stackItem; // Perbarui jumlah stack dalam item

                if (itemData.count <= 0)
                {
                    stats.inventory.Remove(itemData);
                    // Jika stack count habis, hapus item dari inventory

                }


                rectTransform.SetParent(originalParent); // Kembalikan item ke posisi awal

                // Refresh UI setelah perubahan
                MechanicController.Instance.HandleUpdateInventory();
                break; // Keluar dari loop setelah menemukan item
            }

        }

        if (!itemFound)
        {
            // Debug.Log("Item tidak ditemukan atau kategori item bukan Seed");
            rectTransform.SetParent(originalParent); // Kembalikan item ke posisi awal
        }
    }


    public void SaveTileStatus(Vector3Int cellPosition, bool isPlanted)
    {
        // Menyimpan status tile secara lokal menggunakan PlayerPrefs
        string key = cellPosition.ToString();
        PlayerPrefs.SetInt(key, isPlanted ? 1 : 0); // 1 untuk tertanam, 0 untuk belum
        PlayerPrefs.Save(); // Simpan perubahan
    }

    public bool LoadTileStatus(Vector3Int cellPosition)
    {
        // Memuat status tile dari PlayerPrefs
        string key = cellPosition.ToString();
        return PlayerPrefs.GetInt(key, 0) == 1; // 0 berarti belum tertanami
    }





    public void OnBeginDrag(PointerEventData eventData)
    {
        // Simpan parent asli untuk dikembalikan nanti
        originalParent = rectTransform.parent;
        itemInDrag = gameObject.name; // Ambil nama objek yang di-drag
                                      // Debug.Log("nama itemInDrag  " + itemInDrag);

        // Pindahkan item ke DragLayer (harus berada di bawah canvas)
        rectTransform.SetParent(dragLayer);

        canvasGroup.blocksRaycasts = false; // Memungkinkan drag melewati item lain
        rectTransform.SetAsLastSibling(); // Menempatkan item di posisi paling atas dalam hierarchy

        lastProcessedTile = new Vector3Int(99999, 99999, 99999);
        isDraggingActionActive = true;
    }



    public void OnDrag(PointerEventData eventData)
    {
        // Visual: Ikuti Mouse
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        // Logika Pintar (Hanya jalan jika pindah tile)
        if (isDraggingActionActive)
        {
            CheckAndExecuteAction();
        }
    }
    private void CheckAndExecuteAction()
    {
        //  Hitung Posisi Mouse
        Vector3 screenPosition = Input.mousePosition;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(Camera.main.transform.position.z)));
        Vector3Int currentCellPosition = FarmTile.Instance.tilemap.WorldToCell(worldPosition);

        //  GATEKEEPER: Apakah kita masih di tile yang sama dengan frame lalu?
        if (currentCellPosition == lastProcessedTile)
        {
            return; // STOP! Hemat CPU & Inventory.
        }

        // Kita panggil fungsi TryProcessAction (Code Anda yang sudah dirapikan)
        bool success = TryProcessAction(currentCellPosition);

        if (success)
        {
            // Catat tile ini agar tidak diproses 2x
            lastProcessedTile = currentCellPosition;

            MechanicController.Instance.HandleUpdateInventory();

            // Opsional: Cek jika item habis, hentikan drag visual
             //if (itemCount <= 0) OnEndDrag(null);
        }
    }

    private bool TryProcessAction(Vector3Int cellPosition)
    {
        // Cari Data Tile (Gunakan Find, HAPUS foreach loop yang tidak perlu)
        HoedTileData targetTile = FarmTile.Instance.hoedTilesList.Find(t => t.tilePosition == cellPosition);

        if (targetTile == null)
        {
            // Debug.Log("Tanah belum dicangkul!");
            return false;
        }

       

        if (targetTile.isPlanted)
        {
            // Coba gunakan item ke tanaman (Pastikan fungsi ini return bool)
            bool itemUsed = TryUseItemOnTile(cellPosition);

            if (itemUsed)
            {
                Debug.Log("Berhasil menggunakan item (Pestisida/Pupuk) ke tanaman!");
                return true; // Sukses -> Inventory berkurang
            }
            return false;
        }

        else
        {
            bool itemUsedAsFertilizer = TryUseItemOnTile(cellPosition);
            if (itemUsedAsFertilizer)
            {
                Debug.Log("Pupuk berhasil ditebar di tanah kosong!");
                return true;
            }

          

            CekSeed(cellPosition);

            Debug.Log("Posisi valid! Menanam benih...");
            // Asumsi CekSeed berhasil (karena tanah kosong & item valid).
            return true;
        }
    }

   
   



    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        DroppedOnValidTile();
        // Kembalikan visual item ke slot inventory
        rectTransform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero;

        isDraggingActionActive = false;

        // Update inventory terakhir kali untuk memastikan sinkronisasi
        MechanicController.Instance.HandleUpdateInventory();
    }



    // Fungsi untuk mengecek apakah item dijatuhkan pada tile hasil cangkul (hoeedTile)
    private bool DroppedOnValidTile()
    {
        // Debug.Log("item di jatuhkan");

        // Ambil posisi mouse di Screen Space
        Vector3 screenPosition = Input.mousePosition;

        // Konversi posisi dari Screen Space ke World Space, pastikan Z = 0
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(Camera.main.transform.position.z)));

        // Mengubah posisi world ke posisi tilemap
        Vector3Int cellPosition = FarmTile.Instance.tilemap.WorldToCell(worldPosition);



        //// Mengecek tile pada posisi ini
        TileBase currentTile = FarmTile.Instance.tilemap.GetTile(cellPosition);

        //if (currentTile == statsFarm.hoeedTile || currentTile == statsFarm.wateredTile)
        //{
        //    Debug.Log("Item dijatuhkan di tile yang dicangkul.");
        //    // Tanam benih pada tile yang valid
        //    CekSeed(cellPosition);
        //    return true;
        //}

        // Mengecek objek di posisi world
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition);
        if (hitCollider != null)
        {
            // Debug.Log("Item dijatuhkan di objek NPC: ");
            // Cek apakah objek tersebut memiliki script NpcBehavior
            NPCBehavior npc = hitCollider.GetComponent<NPCBehavior>();
            if (npc != null)
            {
                // Debug.Log("Item dijatuhkan di objek NPC: " + hitCollider.name);
                CheckItem(npc);
                // npc.ReceiveItem(this); // Contoh fungsi untuk menerima item, pastikan Anda menambahkan fungsi ini pada `NpcBehavior`
                return true;
            }
        }
        else
        {
            //Debug.Log("hitcollider = null  ");
        }

        if (currentTile != statsFarm.hoeedTile || currentTile != statsFarm.wateredTile)
        {
            // Debug.Log("item di jatuhkan pada tile tanah");
            // Debug.Log("item yang di drag adalah : " + itemInDrag);
            CheckPrefabItem(cellPosition);
            return true;
        }



        // Debug.Log("Tidak ada tile atau NPC di posisi ini.");
        return false;
    }



    // Fungsi untuk menanam benih
    private void PlantSeed(Vector3Int cellPosition, Item seedItem)
    {
        Vector3 spawnPosition = FarmTile.Instance.tilemap.GetCellCenterWorld(cellPosition);
        GameObject plant = Instantiate(plantPrefab, spawnPosition, Quaternion.identity);
        plant.name = $"Tanaman {seedItem.itemName}";

        // Setup Parent & Container
        plant.transform.SetParent(MainEnvironmentManager.Instance.plantContainer.transform);
        FarmTile.Instance.activePlants.Add(cellPosition, plant);

        // Setup Interactable
        PlantInteractable plantInteractable = plant.GetComponent<PlantInteractable>();
        if (plantInteractable != null)
        {
            plantInteractable.promptMessage = $"Tanaman {seedItem.itemName}";
        }

        PlantSeed seedComponent = plant.GetComponent<PlantSeed>();

        if (seedComponent != null)
        {
            // Copy data dasar dari Item Benih
            seedComponent.namaSeed = seedItem.itemName;
            seedComponent.dropItem = seedItem.itemDropName;
            seedComponent.growthImages = seedItem.growthImages;
            seedComponent.growthTime = seedItem.growthTime; 
            seedComponent.seedType = seedItem.seedType;
            seedComponent.plantSeedItem = seedItem;
            seedComponent.rarity = seedItem.rarity;

            // Logika Hujan
            seedComponent.isWatered = TimeManager.Instance.isRain;

            // Generate ID agar siap disimpan ke TileData
            seedComponent.ForceGenerateUniqueID();
        }

        HoedTileData tileData = FarmTile.Instance.hoedTilesList.Find(t => t.tilePosition == cellPosition);

        if (tileData != null)
        {
            tileData.isPlanted = true;
            tileData.growthProgress = 0;

            // Sekarang aman mengambil ID karena ForceGenerateUniqueID sudah dipanggil di atas
            if (seedComponent != null)
            {
                tileData.plantID = seedComponent.UniqueID;
            }

            tileData.plantSeedItem = seedItem;

            if (tileData.hasFertilizer && seedComponent != null)
            {
                seedComponent.hasFertilizer = true;

                float fertilizerStrength = tileData.fertilizerStrength;


                float newGrowthTime = ApplyFertilizer(fertilizerStrength, seedComponent.growthTime);
                seedComponent.growthTime = newGrowthTime;
                tileData.growthTime = newGrowthTime;
                Debug.Log($"Tanaman {seedItem.itemName} mendapat efek pupuk! Waktu tumbuh berkurang menjadi: {seedComponent.growthTime}");
            }
        }
        else
        {
            Debug.LogError($"Gagal menanam! Data tanah tidak ditemukan di posisi {cellPosition}");
            // Opsional: Destroy(plant) jika data tanah error agar tidak ada tanaman 'hantu'
        }
    }
    public float ApplyFertilizer(float percentage, float growthTime)
    {
        if (percentage > 0 && growthTime > 0)
        {
            // Rumus Diskon Waktu
            float multiplier = 1f - (percentage / 100f);
            float newTime = growthTime * multiplier;

            int hariFix = Mathf.FloorToInt(newTime);
            // Pastikan minimal 1 hari (agar tidak error/instan panen)
            return Mathf.Max(1f, hariFix);
        }
        return growthTime;
    }



    // Fungsi untuk menanam benih
    private void PlantTree(Vector3Int cellPosition, string namaSeed, float growthTime)
    {
        // Debug.Log("Menanam pohon...");
        Item itemTree = ItemPool.Instance.GetItemWithQuality(namaSeed, ItemQuality.Normal);
        // Konversi posisi tile ke World Space
        Vector3 spawnPosition = FarmTile.Instance.tilemap.GetCellCenterWorld(cellPosition);
        //GameObject seedTreePrefab = 
        GameObject treeObject = DatabaseManager.Instance.GetTreePrefab(namaSeed);
        int totalGrowthStages = DatabaseManager.Instance.GetTotalGrowthStages(namaSeed);
        // Inisiasi prefab tanaman di posisi world yang sesuai dengan tile
        GameObject plant = Instantiate(treeObject, spawnPosition, Quaternion.identity);



        // Set parent prefab tanaman ke plantsContainer
        plant.transform.SetParent(treeContainer);
        //treeContainer.plantObject.Add(plant);

        // Mendapatkan komponen TreeBehavior dari prefab tanaman
        TreeBehavior treeComponent = plant.GetComponent<TreeBehavior>();
        if (treeComponent != null)
        {
            treeComponent.nameEnvironment = namaSeed;
            treeComponent.currentStage = GrowthTree.Seed;
            treeComponent.growthTime = growthTime;
            treeComponent.growthSpeed = itemTree.growthTime / totalGrowthStages;
            //treeComponent.OnTreeChoppedDown();

            treeComponent.ForceGenerateUniqueID();


        }

        TreePlacementData newPlacementData = new TreePlacementData
        {
            TreeID = treeComponent.UniqueID,
            position = treeComponent.transform.position,
            typePlant = treeComponent.typePlant,
            isGrow = true,
            initialStage = treeComponent.currentStage,
            sudahTumbang = treeComponent.isRubuh,
        };

        // Tambahkan data baru ke dalam list di ScriptableObject
        MainEnvironmentManager.Instance.pohonManager.secondListTrees.Add(newPlacementData);
        //Debug.Log("Prefab tanaman ditanam di posisi: " + spawnPosition);
    }




    public void CheckItem(NPCBehavior npc)
    {
        bool itemFoundAndProcessed = false;

        // Loop dari belakang ke depan. Ini cara aman untuk menghapus elemen dari list saat iterasi.
        for (int i = stats.inventory.Count - 1; i >= 0; i--)
        {
            ItemData inventoryItemData = stats.inventory[i];
            Item itemSO = ItemPool.Instance.GetItemWithQuality(inventoryItemData.itemName, inventoryItemData.quality);

            if (itemSO.itemName == itemInDrag && itemSO.categories != ItemCategory.PlantSeed)
            {


                // Panggil fungsi HANYA SEKALI dan simpan hasilnya ke dalam variabel.
                int amountProcessed = QuestManager.Instance.ProcessItemGivenToNPC(inventoryItemData, npc.npcName);

                // GUNAKAN variabel yang sudah disimpan untuk semua pengecekan.
                if (amountProcessed > 0)
                {
                    // GUNAKAN variabel yang sama untuk mengurangi inventaris.
                    Debug.Log($"Memproses pemberian {amountProcessed} x {inventoryItemData.itemName} ke NPC {npc.npcName}.");
                    stats.inventory[i].count -= amountProcessed;

                    if (stats.inventory[i].count <= 0)
                    {
                        stats.inventory.RemoveAt(i);
                        Debug.Log($"{inventoryItemData.itemName} habis dan dihapus dari inventory.");
                    }

                    itemFoundAndProcessed = true;
                    MechanicController.Instance.HandleUpdateInventory();
                    break;
                }

            }
        }

        // Kembalikan item yang di-drag ke slotnya (jika ini adalah sistem drag-and-drop)
        if (rectTransform != null && originalParent != null)
        {
            rectTransform.SetParent(originalParent);
        }

        // Selalu refresh UI di akhir untuk menampilkan perubahan
        MechanicController.Instance.HandleUpdateInventory();
    }

    private void PlacePrefab(Vector3Int cellPosition, string namaItem, ItemData prefabItem)
    {
        Debug.Log("Menambahkan Game Object...");
        // Konversi posisi tile ke World Space
        Vector3 spawnPosition = FarmTile.Instance.tilemap.GetCellCenterWorld(cellPosition);
        Item item = ItemPool.Instance.GetItemWithQuality(namaItem, ItemQuality.Normal);
        GameObject prefabObject = null;

        // Inisiasi prefab tanaman di posisi world yang sesuai dengan tile
        if (item.IsInType(ItemType.Perangkap))
        {
            prefabObject = Instantiate(DatabaseManager.Instance.perangkapWorldPrefab, spawnPosition, Quaternion.identity);

            PerangkapBehavior perangkapBehavior = prefabObject.GetComponent<PerangkapBehavior>();
            if (perangkapBehavior == null)
            {
                Debug.LogError($"Prefab 'perangkapWorldPrefab' tidak memiliki komponen PerangkapBehavior!", prefabObject);
                Destroy(prefabObject); // Hancurkan objek yg salah dibuat
                return; // Hentikan fungsi agar tidak crash
            }

            Transform locationTransform = MainEnvironmentManager.Instance.perangkapManager.transform;
            prefabObject.transform.SetParent(locationTransform);
            perangkapBehavior.ForceGenerateUniqueID();
            perangkapBehavior.perangkapHealth = prefabItem.itemHealth;

            // buat data penempatan perangkap baru
            PerangkapSaveData newPlacementData = new PerangkapSaveData
            {
                id = perangkapBehavior.UniqueID,
                perangkapPosition = perangkapBehavior.transform.position,
                healthPerangkap = prefabItem.itemHealth,
                hasilTangkap = null
            };

            // Tambahkan data baru ke dalam list Objek Manager (PerangkapManager)
            MainEnvironmentManager.Instance.perangkapManager.perangkapListActive.Add(newPlacementData);
        }
        else if (item.IsInType(ItemType.Pelebur))
        {
            // Sesuaikan posisi Y jika perlu (bisa diringkas)
            spawnPosition.z = -1f;

            prefabObject = Instantiate(DatabaseManager.Instance.furnanceWorldPrefab, spawnPosition, Quaternion.identity);

            CookInteractable peleburpBehavior = prefabObject.GetComponent<CookInteractable>();
            if (peleburpBehavior == null)
            {
                Debug.LogError($"Prefab 'peleburWorldPrefab' tidak memiliki komponen CookInteractable!", prefabObject);
                Destroy(prefabObject); // Hancurkan objek yg salah dibuat
                return; // Hentikan fungsi
            }

            InteractableUniqueID interactableUniqueID = prefabObject.GetComponent<InteractableUniqueID>();
            if (interactableUniqueID == null)
            {
                Debug.LogError($"Prefab 'peleburWorldPrefab' tidak memiliki komponen InteractableUniqueID!", prefabObject);
                Destroy(prefabObject); // Hancurkan objek yg salah dibuat
                return; // Hentikan fungsi
            }

            Transform locationTransform = MainEnvironmentManager.Instance.komporManager.transform;
            prefabObject.transform.SetParent(locationTransform);
            interactableUniqueID.ForceGenerateUniqueID();

            FurnanceSaveData newFurnanceSaveData = new FurnanceSaveData
            {
                id = interactableUniqueID.UniqueID,
                furnancePosition = spawnPosition,
                itemCook = peleburpBehavior.itemCook,
                fuelCook = peleburpBehavior.fuelCook,
                itemResult = peleburpBehavior.itemResult,
                quantityFuel = 0
            };

            // Tambahkan data baru ke dalam list Objek Manager
            MainEnvironmentManager.Instance.komporManager.environmentList.Add(newFurnanceSaveData);
        }

        //Debug.Log("Prefab ditanam di posisi: " + spawnPosition);
    }



    public bool TryUseItemOnTile(Vector3Int cellPosition)
    {
        Debug.Log("Memanggil fungsi place pestisida");

        GameObject tanaman = FarmTile.Instance.GetPlantAtPosition(cellPosition);

        if (tanaman != null)
        {
            PlantSeed plantSeed = tanaman.GetComponent<PlantSeed>();

            if (plantSeed != null && plantSeed.isInfected)
            {
                foreach (ItemData itemData in stats.inventory)
                {
                    Item item = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);

                    Debug.Log("Memeriksa item: " + item.itemName + " dengan itemInDrag: " + itemInDrag);

                    // Cek apakah item yang dipilih sesuai
                    if (string.Equals(item.itemName, itemInDrag, StringComparison.OrdinalIgnoreCase) &&
                        item.IsInCategory(ItemCategory.Insectisida) &&
                        item.IsInType(ItemType.Pestisida))
                    {
                        Debug.Log("Item cocok ditemukan: " + item.itemName);

                        // Kurangi jumlah item dalam inventory
                        itemData.count--;

                        if (itemData.count <= 0)
                        {
                            //Player_Inventory.Instance.RemoveItem(item);
                            stats.inventory.Remove(itemData);
                        }

                        plantSeed.isInfected = false;
                        plantSeed.isWatered = false;
                        FarmTile.Instance.HandlePlacePestisida(plantSeed.UniqueID);
                        plantSeed.UpdateParticleEffect();

                        MechanicController.Instance.HandleUpdateInventory();
                        PlantInteractable plantInteractable = tanaman.GetComponent<PlantInteractable>();
                        plantInteractable.promptMessage = $"Tanaman {plantSeed.namaSeed}";
                        Debug.Log("Serangga berhasil dibasmi di tanaman: " + tanaman.name);
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning("Item yang digunakan tidak cocok: " +
                            "Nama item: " + item.itemName + ", Item drag: " + itemInDrag);
                    }
                }

                Debug.Log("Tidak ada item yang sesuai di inventory.");
            }
            else
            {
                Debug.Log("Tanaman tidak memiliki serangga atau tidak ada komponen PlantSeed.");

            }
        }
        else
        {
            Debug.Log("Tanaman tidak ditemukan di posisi: " + cellPosition);
            HoedTileData hoedTileData = farmTile.GetTileData(cellPosition);
            if (hoedTileData != null && !hoedTileData.hasFertilizer)
            {
                foreach (ItemData itemData in stats.inventory)
                {
                    Item item = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);

                    // Cek apakah item yang di-drag adalah PUPUK
                    if (string.Equals(item.itemName, itemInDrag, StringComparison.OrdinalIgnoreCase) &&
                        item.IsInCategory(ItemCategory.Pupuk) && // Sesuaikan kategori pupuk 
                        item.IsInType(ItemType.Pestisida)) // Pastikan tipe item adalah Pestisida
                    {
                        // Kurangi Inventory
                        itemData.count--;
                        if (itemData.count <= 0)
                        {
                            stats.inventory.Remove(itemData);
                        }

                        // Eksekusi Logika Pupuk
                        FarmTile.Instance.BeriPupuk(cellPosition, item.persentasePupuk);

                        MechanicController.Instance.HandleUpdateInventory();

                        Debug.Log("Berhasil memberi pupuk ke tanah kosong!");
                        return true; // Berhasil
                    }
                }
            }
            else
            {
                Debug.Log("HoedTileData TIDAK ditemukan untuk posisi: " + cellPosition);
            }
        }

        return false;
    }



}
