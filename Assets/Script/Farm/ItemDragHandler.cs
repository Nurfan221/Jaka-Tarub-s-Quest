
using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using static QuestManager;


public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector2 originalPosition; // Menyimpan posisi awal

    //public FarmTile farmTile;   // Akses ke FarmTile yang punya hoeedTile

    private Transform originalParent; // Untuk menyimpan parent asli item
    public Transform dragLayer; // Assign ini ke layer khusus di canvas untuk menempatkan item yang di-drag
    public GameObject plantPrefab; // Prefab tanaman yang akan ditanam
    public Transform plantsContainer; // Referensi ke GameObject kosong yang menampung tanaman
    public Transform prefabContainer; // referansi ke gameobject kosont yang menampung prefab game objek 


     private string itemInDrag;// Ambil nama objek yang di-drag

    [SerializeField] GiveCountContainer giveCountContainer;
    public int itemCount; // Index item yang di berikan
     
     [SerializeField] InventoryUI inventoryUI;
    [SerializeField] FenceBehavior fenceBehavior;
    [SerializeField] PlantContainer plantContainer;

    //public static class TileManager
    //{
    //    // Menyimpan status setiap tile di seluruh permainan
    //    public static Dictionary<Vector3Int, bool> tilePlantStatus = new Dictionary<Vector3Int, bool>();
    //}


    private PlayerData_SO stats;
    public FarmData_SO statsFarm;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>(); // Mendapatkan Canvas induk

       

        // Ambil "Papan Pengumuman" dari Otak dan simpan ke jalan pintas kita.
        if (PlayerController.Instance != null)
        {
            stats = PlayerController.Instance.playerData;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }

        if (FarmTile.Instance != null)
        {
            statsFarm = FarmTile.Instance.farmData;
        }
        else
        {
            Debug.LogError("FarmTile.Instance tidak ditemukan saat Awake!");
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
        foreach (var lokasihoedTile in statsFarm.hoedTilesList)
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
                plantPrefab = item.prefabItem;

                // Debug.Log("Item ditemukan: " + item.itemName);

                // Menanam benih
                PlantSeed(cellPosition, item.itemName, item.dropItem, item.growthImages, item.growthTime);

                // Panggil fungsi untuk menyimpan status tile
                SaveTileStatus(cellPosition, true);

                itemData.count -= 1;

                if (itemData.count <= 0)
                {
                    stats.inventory.Remove(itemData);
                    Debug.Log("Item habis dan dihapus.");
                }

                // Refresh UI
                inventoryUI.RefreshInventoryItems();
                inventoryUI.UpdateSixItemDisplay();
                break;
            }
        }
    }







    //logika menanam pohon
    public void CheckPrefabItem(Vector3Int cellPosition)
    {
        bool itemFound = false; // Flag untuk mengecek apakah item ditemukan
        Debug.Log(" fungsi cek tree seed di jalankan");

        foreach (ItemData itemData in stats.inventory)
        {
            Item item = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);

            // Debug.Log("nama itemInDrag  " + itemInDrag);
            // Cek apakah nama item sama dengan itemInDrag dan kategori item adalah Seed
            if (item.itemName == itemInDrag && !item.IsInCategory(ItemCategory.PlantSeed) && item.types == ItemType.ItemPrefab)
            {
                // Debug.Log("nama item.itemname " + item.itemName);
                itemFound = true; // Tandai bahwa item ditemukan
                //int stackItem = item.stackCount; // Ambil jumlah item yang ada
                plantPrefab = item.prefabItem; // Set plantPrefab sesuai item

                // Debug.Log("Item ditemukan: " + item.itemName + ", Kategori: " + item.categories);



                // Panggil fungsi untuk menanam benih dengan menambahkan parameter growthImages dari item
                if (item.IsInCategory(ItemCategory.TreeSeed))
                {

                    plantPrefab = item.growthObject[0]; // Set plantPrefab sesuai item

                    PlantTree(cellPosition, item.itemName, item.growthObject, item.growthTime);
                }else
                {
                    PlacePrefab(cellPosition, item.itemName, item.prefabItem, item.health);
                }

                // Kurangi stack item setelah menanam
                itemData.count--;
                //item.stackCount = stackItem; // Perbarui jumlah stack dalam item

                if (itemData.count <= 0)
                {
                    stats.inventory.Remove(itemData);
                    // Jika stack count habis, hapus item dari inventory
                    //Player_Inventory.Instance.RemoveItem(item);
                    //    Debug.Log("Item habis dan dihapus dari inventory.");
                }
                else
                {
                    // Debug.Log("Jumlah item tersisa: " + stackItem);
                }

                rectTransform.SetParent(originalParent); // Kembalikan item ke posisi awal

                // Refresh UI setelah perubahan
                inventoryUI.RefreshInventoryItems();
                inventoryUI.UpdateSixItemDisplay();
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
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Menggerakkan item mengikuti pointer
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        // Mengecek posisi item selama drag
        CheckItemPositionDuringDrag();


    }

    // Fungsi untuk memeriksa posisi item selama drag
    private void CheckItemPositionDuringDrag()
    {
        // Ambil posisi mouse di Screen Space
        Vector3 screenPosition = Input.mousePosition;

        // Konversi posisi dari Screen Space ke World Space
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(Camera.main.transform.position.z)));

        // Mengubah posisi world ke posisi tilemap
        Vector3Int cellPosition = FarmTile.Instance.tilemap.WorldToCell(worldPosition);
        // Mengecek tile pada posisi ini
        TileBase currentTile = FarmTile.Instance.tilemap.GetTile(cellPosition);

        if ((currentTile == statsFarm.hoeedTile || currentTile == statsFarm.wateredTile))
        {
            // Cek apakah tile sudah tertanami
            foreach (var lokasihoedTile in statsFarm.hoedTilesList)
            {
                if (lokasihoedTile.tilePosition == cellPosition && lokasihoedTile.isPlanted == true)  // Membandingkan dengan Vector3Int
                {
                    return;
                }
            }

            // Debug.Log("Item sedang di atas tile yang dicangkul dan belum ada tanaman.");
            // Tanam benih pada tile yang valid
            CekSeed(cellPosition); // Menjalankan logika menanam benih

            inventoryUI.UpdateSixItemDisplay();
            inventoryUI.RefreshInventoryItems();
        }
        //else if (currentTile == farmTile.grassTile)
        //{
        //    Debug.Log("Item sedang di atas tile tanah.");
        //    Debug.Log("item yang di drag adalah : " + itemInDrag);
        //    CheckPrefabItem(cellPosition); // Menjalankan logika sesuai tile tanah
        //}
        else
        {
            //Debug.Log("Item tidak berada di posisi yang valid.");
        }

        
    }




    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true; // Mengembalikan interaksi raycast

        // Ambil posisi mouse di Screen Space
        Vector3 screenPosition = Input.mousePosition;

        // Konversi posisi dari Screen Space ke World Space
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(Camera.main.transform.position.z)));

        // Mengubah posisi world ke posisi tilemap
        Vector3Int cellPosition = FarmTile.Instance.tilemap.WorldToCell(worldPosition);

        // Mengecek tile pada posisi ini
        TileBase currentTile = FarmTile.Instance.tilemap.GetTile(cellPosition);

        if ((currentTile == statsFarm.hoeedTile || currentTile == statsFarm.wateredTile))
        {
            // Cek apakah tile sudah tertanami
            foreach (var lokasihoedTile in statsFarm.hoedTilesList)
            {
                if (lokasihoedTile.tilePosition == cellPosition && lokasihoedTile.isPlanted == true)  // Membandingkan dengan Vector3Int
                {
                    bool berhasil = PlacePestisida(cellPosition);

                    if (berhasil)
                    {
                        Debug.Log("Berhasil membasmi serangga!");
                    }
                    else
                    {
                        inventoryUI.RefreshInventoryItems();
                        inventoryUI.UpdateSixItemDisplay();
                        // Debug.Log("Gagal membasmi serangga, item dikembalikan.");
                        rectTransform.SetParent(originalParent); // Baru kembalikan kalau gagal
                    }
                    inventoryUI.RefreshInventoryItems();
                    inventoryUI.UpdateSixItemDisplay();
                    rectTransform.SetParent(originalParent); // Baru kembalikan kalau gagal
                    return;
                }
            }
           

            // Debug.Log("Item dijatuhkan di tile yang dicangkul dan belum ada tanaman.");
            // Tanam benih pada tile yang valid
            CekSeed(cellPosition); // Menjalankan logika menanam benih
           
        }
        else
        {
            inventoryUI.RefreshInventoryItems();
            inventoryUI.UpdateSixItemDisplay();
            // Debug.Log("Item tidak berada di posisi yang valid.");
            rectTransform.SetParent(originalParent); // Kembalikan item ke posisi awal jika tidak valid
        }

        DroppedOnValidTile();
        inventoryUI.RefreshInventoryItems();
        inventoryUI.UpdateSixItemDisplay();
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
    private void PlantSeed(Vector3Int cellPosition, string namaSeed, GameObject dropItem, Sprite[] growthImages, float growthTime)
    {
        // Konversi posisi tile ke World Space
        Vector3 spawnPosition = FarmTile.Instance.tilemap.GetCellCenterWorld(cellPosition);

        // Konversi spawnPosition menjadi Vector3Int untuk perbandingan yang tepat
        Vector3Int spawnPositionInt = new Vector3Int(Mathf.FloorToInt(spawnPosition.x), Mathf.FloorToInt(spawnPosition.y), Mathf.FloorToInt(spawnPosition.z));

        // Inisiasi prefab tanaman di posisi world yang sesuai dengan tile
        GameObject plant = Instantiate(plantPrefab, spawnPosition, Quaternion.identity);

        // Set parent prefab tanaman ke plantsContainer
        plant.transform.SetParent(plantsContainer);
        FarmTile.Instance.activePlants.Add(cellPosition, plant);
        // Mendapatkan komponen Seed dari prefab tanaman
        PlantSeed seedComponent = plant.GetComponent<PlantSeed>();
        if (seedComponent != null)
        {
            // Mengatur nilai namaSeed, dropItem, dan growthImages
            seedComponent.namaSeed = namaSeed;
            seedComponent.dropItem = dropItem;
            seedComponent.growthImages = growthImages; // Simpan growthImages ke komponen Seed
            seedComponent.growthTime = growthTime; // Simpan growthTime ke komponen Seed
            //seedComponent.plantLocation = spawnPosition;
        }

        // Memeriksa apakah ada tile yang dicangkul dan menambahkan plantStatus
        foreach (var lokasihoedTile in statsFarm.hoedTilesList)
        {
            if (lokasihoedTile.tilePosition == spawnPositionInt)  // Membandingkan dengan Vector3Int
            {
                lokasihoedTile.isPlanted = true;
                lokasihoedTile.growthProgress = 0;
                lokasihoedTile.plantedItemName = namaSeed;
            }
        }
    }



    // Fungsi untuk menanam benih
    private void PlantTree(Vector3Int cellPosition, string namaSeed, GameObject[] gameObjects, float growthTime)
    {
        // Debug.Log("Menanam pohon...");

        // Konversi posisi tile ke World Space
        Vector3 spawnPosition = FarmTile.Instance.tilemap.GetCellCenterWorld(cellPosition);

        // Inisiasi prefab tanaman di posisi world yang sesuai dengan tile
        GameObject plant = Instantiate(plantPrefab, spawnPosition, Quaternion.identity);

        

        // Set parent prefab tanaman ke plantsContainer
        plant.transform.SetParent(plantsContainer);
        plantContainer.plantObject.Add(plant);

        // Mendapatkan komponen TreeBehavior dari prefab tanaman
        TreeBehavior treeComponent = plant.GetComponent<TreeBehavior>();
        if (treeComponent != null)
        {
            treeComponent.nameEnvironment= namaSeed;
            treeComponent.growthObject = gameObjects;
            treeComponent.growthTime = growthTime;
            treeComponent.plantsContainer = plantsContainer;
            treeComponent.growthSpeed = growthTime / gameObjects.Length;




        }

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

            // Cek apakah ini item yang dicari
            if (itemSO.itemName == itemInDrag && itemSO.categories != ItemCategory.PlantSeed)
            {
                // Panggil CheckItemGive. Fungsi ini akan mengubah inventoryItemData.count secara langsung.
                // Coba untuk Side Quest, jika gagal, coba untuk Mini Quest.
                bool success = npc.CheckItemGive(inventoryItemData) || npc.CheckItemGive(inventoryItemData);

                if (success)
                {
                    // Setelah item berhasil diberikan, periksa jumlahnya di inventaris
                    if (inventoryItemData.count <= 0)
                    {
                        // Hapus dari list jika habis
                        stats.inventory.RemoveAt(i);
                        Debug.Log($"{inventoryItemData.itemName} habis dan dihapus dari inventory.");
                    }

                    itemFoundAndProcessed = true;
                    break; // Keluar dari loop karena item sudah ditemukan dan diproses
                }
            }
        }

        // Kembalikan item yang di-drag ke slotnya (jika ini adalah sistem drag-and-drop)
        if (rectTransform != null && originalParent != null)
        {
            rectTransform.SetParent(originalParent);
        }

        // Selalu refresh UI di akhir untuk menampilkan perubahan
        inventoryUI.RefreshInventoryItems();
        inventoryUI.UpdateSixItemDisplay();
    }

    private void PlacePrefab(Vector3Int cellPosition, string namaItem, GameObject prefabItem, float health)
    {

        // Debug.Log("Menambahkan Game Object...");
        // Konversi posisi tile ke World Space
        Vector3 spawnPosition = FarmTile.Instance.tilemap.GetCellCenterWorld(cellPosition);

        // Inisiasi prefab tanaman di posisi world yang sesuai dengan tile
        GameObject plant = Instantiate(prefabItem, spawnPosition, Quaternion.identity);

        // Set parent prefab tanaman ke plantsContainer
        plant.transform.SetParent(prefabContainer);

        //Cek apakah prefab memiliki komponen PrefabItemBehavior
        PrefabItemBehavior prefabBehavior = plant.GetComponent<PrefabItemBehavior>();
        FenceBehavior fenceBehavior = plant.GetComponent<FenceBehavior>();
        if (prefabBehavior != null || fenceBehavior)
        {
            // Set health dari prefab berdasarkan nilai health dari item di inventory
            prefabBehavior.health = health;
            fenceBehavior.UpdateFenceSprite();
            //Debug.Log("Prefab memiliki PrefabItemBehavior. Health diset ke: " + health);
        }
        else
        {
            Debug.LogWarning("Prefab tidak memiliki komponen PrefabItemBehavior.");
        }



        //Debug.Log("Prefab tanaman ditanam di posisi: " + spawnPosition);
    }


  
    public bool PlacePestisida(Vector3Int cellPosition)
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
                        plantSeed.UpdateParticleEffect();

                        inventoryUI.RefreshInventoryItems();
                        inventoryUI.UpdateSixItemDisplay();

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
        }

        return false;
    }




}

