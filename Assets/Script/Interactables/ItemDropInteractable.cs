using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropInteractable : Interactable
{
    //public Item item;
    public ItemData itemdata;
    [SerializeField] public string itemName;
    private Rigidbody2D rb;
    public float gravityDuration = 2f; // Durasi gravitasi setelah item jatuh
    [Tooltip("Tag yang akan digunakan setelah item bisa diambil kembali.")]
    public string pickableTag = "ItemDrop"; // Tag akhir setelah jeda

    [Tooltip("Waktu tunda dalam detik sebelum item bisa diambil.")]
    public float pickupDelay = 5.0f; // Jeda waktu 5 detik sesuai permintaan Anda
    public bool itemDrop;

    private void Start()
    {

       
        //promptMessage = "Inventory Full";
        //GetComponent<SpriteRenderer>().sprite = item.sprite;
        itemdata.itemName = itemName;
        itemdata.count = 1;

        if (itemDrop)
        {
            StartCoroutine(ActivatePickupAfterDelay());
        }
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
        // 1. Saat item baru saja dibuat, langsung ubah tag-nya menjadi "Untagged"
        //    agar tidak bisa langsung diambil oleh player.
        gameObject.tag = "Untagged";
        Debug.Log($"Item '{this.name}' dijatuhkan, tag diubah menjadi 'Untagged'. Menunggu {pickupDelay} detik...");

        // 2. Jeda eksekusi fungsi ini selama 'pickupDelay' detik.
        //    Bagian ini tidak akan membekukan (freeze) seluruh game.
        yield return new WaitForSeconds(pickupDelay);

        // 3. Setelah jeda selesai, kode di bawah ini akan dijalankan.
        //    Ubah kembali tag-nya agar bisa diambil oleh player.
        gameObject.tag = pickableTag;
        Debug.Log($"Item '{this.name}' sekarang bisa diambil! Tag diubah menjadi '{pickableTag}'.");
    }
}
