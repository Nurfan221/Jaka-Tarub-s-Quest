using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;


public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector2 originalPosition; // Menyimpan posisi awal
    public Tilemap farmTilemap; // Tilemap tempat tanah berada
    public FarmTile farmTile;   // Akses ke FarmTile yang punya hoeedTile

    private Transform originalParent; // Untuk menyimpan parent asli item
    public Transform dragLayer; // Assign ini ke layer khusus di canvas untuk menempatkan item yang di-drag
    public GameObject plantPrefab; // Prefab tanaman yang akan ditanam
    public Transform plantsContainer; // Referensi ke GameObject kosong yang menampung tanaman

     private string itemNameSeed;// Ambil nama objek yang di-drag
     [SerializeField] InventoryUI inventoryUI;       
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>(); // Mendapatkan Canvas induk
    }

    public void CekSeed(Vector3Int cellPosition)
    {
        bool itemFound = false; // Flag untuk mengecek apakah item ditemukan

        foreach (Item item in Player_Inventory.Instance.itemList)
        {
            // Cek apakah nama item sama dengan itemNameSeed dan kategori item adalah Seed
            if (item.itemName == itemNameSeed && item.category == ItemCategory.Seed)
            {
                itemFound = true; // Tandai bahwa item ditemukan
                int stackItem = item.stackCount; // Ambil jumlah item yang ada
                plantPrefab = item.plantPrefab; // Set plantPrefab sesuai item

                Debug.Log("Item ditemukan: " + item.itemName + ", Kategori: " + item.category);
                
                // Panggil fungsi untuk menanam benih dengan menambahkan parameter growthImages dari item
                PlantSeed(cellPosition, item.itemName, item.dropItem, item.growthImages, item.growthTime);
                
                // Kurangi stack item setelah menanam
                stackItem--;
                item.stackCount = stackItem; // Perbarui jumlah stack dalam item
                
                if (stackItem <= 0)
                {
                    // Jika stack count habis, hapus item dari inventory
                    Player_Inventory.Instance.RemoveItem(item);
                    Debug.Log("Item habis dan dihapus dari inventory.");
                }
                else
                {
                    Debug.Log("Jumlah item tersisa: " + stackItem);
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
            Debug.Log("Item tidak ditemukan atau kategori item bukan Seed");
            rectTransform.SetParent(originalParent); // Kembalikan item ke posisi awal
        }
    }

   




    public void OnBeginDrag(PointerEventData eventData)
    {
        // Simpan parent asli untuk dikembalikan nanti
        originalParent = rectTransform.parent;
        itemNameSeed = gameObject.name; // Ambil nama objek yang di-drag

        // Pindahkan item ke DragLayer (harus berada di bawah canvas)
        rectTransform.SetParent(dragLayer);

        canvasGroup.blocksRaycasts = false; // Memungkinkan drag melewati item lain
        rectTransform.SetAsLastSibling(); // Menempatkan item di posisi paling atas dalam hierarchy
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor; // Menggerakkan item mengikuti pointer
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true; // Mengembalikan interaksi raycast

        // Jika item tidak dijatuhkan di tempat yang valid, kembalikan ke posisi awal
        if (!DroppedOnValidTile())
        {
            // Kembalikan item ke parent aslinya setelah drag selesai
            rectTransform.SetParent(originalParent);
        }
        else
        {
            
            Debug.Log("nama item yang di drag adalah : " + itemNameSeed);
            // PlantSeed(); // Menjalankan logika menanam jika dijatuhkan di tempat yang valid
        }
    }

    // Fungsi untuk mengecek apakah item dijatuhkan pada tile hasil cangkul (hoeedTile)
    private bool DroppedOnValidTile()
    {
        // Ambil posisi mouse di Screen Space
        Vector3 screenPosition = Input.mousePosition;

        // Konversi posisi dari Screen Space ke World Space, pastikan Z = 0
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(Camera.main.transform.position.z)));

        // Mengubah posisi world ke posisi tilemap
        Vector3Int cellPosition = farmTilemap.WorldToCell(worldPosition);

        Debug.Log("Posisi World dari item: " + worldPosition);
        Debug.Log("Posisi Tile Cell: " + cellPosition);

        // Mengecek tile pada posisi ini
        TileBase currentTile = farmTilemap.GetTile(cellPosition);

        if (currentTile == farmTile.hoeedTile || currentTile == farmTile.wateredTile)
        {
            Debug.Log("Item dijatuhkan di tile yang dicangkul.");
            // Tanam benih pada tile yang valid
            CekSeed(cellPosition);
            return true;
        }

        // Mengecek objek di posisi world
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition);
        if (hitCollider != null)
        {
            // Cek apakah objek tersebut memiliki script NpcBehavior
            NPCBehavior npc = hitCollider.GetComponent<NPCBehavior>();
            if (npc != null)
            {
                Debug.Log("Item dijatuhkan di objek NPC: " + hitCollider.name);
                CheckItem(npc);
                // npc.ReceiveItem(this); // Contoh fungsi untuk menerima item, pastikan Anda menambahkan fungsi ini pada `NpcBehavior`
                return true;
            }
        }

        Debug.Log("Tidak ada tile atau NPC di posisi ini.");
        return false;
    }

   

    // Fungsi untuk menanam benih
    private void PlantSeed(Vector3Int cellPosition, string namaSeed, GameObject dropItem, Sprite[] growthImages, float growthTime)
    {
        Debug.Log("Menanam benih...");
        // Konversi posisi tile ke World Space
        Vector3 spawnPosition = farmTilemap.GetCellCenterWorld(cellPosition);

        // Inisiasi prefab tanaman di posisi world yang sesuai dengan tile
        GameObject plant = Instantiate(plantPrefab, spawnPosition, Quaternion.identity);

        // Set parent prefab tanaman ke plantsContainer
        plant.transform.SetParent(plantsContainer);

        // Mendapatkan komponen Seed dari prefab tanaman
        SeedManager seedComponent = plant.GetComponent<SeedManager>();
        if (seedComponent != null)
        {
            // Mengatur nilai namaSeed, dropItem, dan growthImages
            seedComponent.namaSeed = namaSeed;
            seedComponent.dropItem = dropItem;
            seedComponent.growthImages = growthImages; // Simpan growthImages ke komponen Seed
            seedComponent.growthTime = growthTime; // Simpan growthTime ke komponen Seed
        }

        Debug.Log("Prefab tanaman ditanam di posisi: " + spawnPosition);
    }

    public void CheckItem(NPCBehavior npc)
    {
        bool itemFound = false; // Flag untuk mengecek apakah item ditemukan

        foreach (Item item in Player_Inventory.Instance.itemList)
        {
            // Cek apakah nama item sama dengan itemNameSeed dan kategori item adalah Seed
            if (item.itemName == itemNameSeed && item.category != ItemCategory.Seed)
            {
                itemFound = true; // Tandai bahwa item ditemukan
                int stackItem = item.stackCount; // Ambil jumlah item yang ada
                

                Debug.Log("Item ditemukan: " + item.itemName + ", Kategori: " + item.category);
                
                // Panggil fungsi untuk menanam benih dengan menambahkan parameter growthImages dari item
                // PlantSeed(cellPosition, item.itemName, item.dropItem, item.growthImages, item.growthTime);
                npc.itemQuest = item.itemName;
                npc.jumlahItem ++;
                npc.CheckItemGive();
                
                // Kurangi stack item setelah menanam
                stackItem--;
                item.stackCount = stackItem; // Perbarui jumlah stack dalam item
                
                if (stackItem <= 0)
                {
                    // Jika stack count habis, hapus item dari inventory
                    Player_Inventory.Instance.RemoveItem(item);
                    Debug.Log("Item habis dan dihapus dari inventory.");
                }
                else
                {
                    Debug.Log("Jumlah item tersisa: " + stackItem);
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
            Debug.Log("Item tidak ditemukan atau kategori item bukan Seed");
            rectTransform.SetParent(originalParent); // Kembalikan item ke posisi awal
        }
    }



    // Fungsi untuk menempatkan prefab di posisi tile yang valid
    
}