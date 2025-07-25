using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropInteractable : Interactable
{
    //public Item item;
    public ItemData itemdata;
 public string itemName;
    private Rigidbody2D rb;
    public float gravityDuration = 2f; // Durasi gravitasi setelah item jatuh
    [Tooltip("Tag yang akan digunakan setelah item bisa diambil kembali.")]
    public string pickableTag = "ItemDrop"; // Tag akhir setelah jeda

    [Tooltip("Waktu tunda dalam detik sebelum item bisa diambil.")]
    public float pickupDelay = 5.0f; // Jeda waktu 5 detik sesuai permintaan Anda

    private Collider2D itemCollider; // Tambahkan ini

    void Awake()
    {
        itemCollider = GetComponent<Collider2D>(); // Dapatkan referensi collider saat Awake
        if (itemCollider == null)
        {
            Debug.LogError("ItemDropInteractable requires a Collider2D component!");
        }


    }
    private void Start()
    {

       
        //promptMessage = "Inventory Full";
        //GetComponent<SpriteRenderer>().sprite = item.sprite;
        itemdata.itemName = itemName;
        itemdata.count = 1;

        StartCoroutine(ActivatePickupAfterDelay());
    }

    protected override void Interact()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Pick");
        Debug.Log(itemdata.itemName + " di ambil.");
        //Player_Inventory.Instance.AddItem(ItemPool.Instance.GetItem(item.itemName));
        //if (item.type == ItemType.Quest) { GetComponent<QuestQuanta>().Take(); }
        ItemPool.Instance.AddItem(itemdata);

        Destroy(gameObject);
    }

    public IEnumerator StopGravity(Rigidbody2D rb, float delay)
    {
        Debug.Log("ItemDropInteractable: StopGravity coroutine started.");
        yield return new WaitForSeconds(delay);
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
        Debug.Log("ItemDropInteractable: Gravity set to 0.");
    }

    private IEnumerator ActivatePickupAfterDelay()
    {
        // Item sudah memiliki tag "ItemDrop" sejak dijatuhkan.
        // Untuk mencegah player mengambilnya, nonaktifkan collidernya sementara.
        if (itemCollider != null)
        {
            itemCollider.enabled = false; // Nonaktifkan collider agar player tidak bisa berinteraksi
        }
        // Jika Anda menggunakan tag untuk deteksi player, Anda bisa ganti di sini:
        // gameObject.tag = "UntaggedForPlayer"; // Atau tag lain yang tidak dideteksi player

        Debug.Log($"Item '{this.name}' dijatuhkan, collider dinonaktifkan. Menunggu {pickupDelay} detik...");

        yield return new WaitForSeconds(pickupDelay);

        // Aktifkan kembali collider agar player bisa mengambilnya.
        if (itemCollider != null)
        {
            itemCollider.enabled = true; // Aktifkan collider
        }
        // Jika menggunakan tag khusus player:
        // gameObject.tag = pickableTag;

        Debug.Log($"Item '{this.name}' sekarang bisa diambil! Collider diaktifkan kembali.");
    }
}
