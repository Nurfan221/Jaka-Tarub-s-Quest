using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPool : MonoBehaviour
{
    public static ItemPool Instance;

    [SerializeField]public List<Item> items;

    private void Awake()
    {
        Instance = this;

        // Inisialisasi itemIDs berdasarkan urutan dalam list
        for (int i = 0; i < items.Count; i++)
        {
            items[i].itemID = i + 1; // Mengatur itemID sesuai urutan, dimulai dari 1
        }
    }

    public Item GetItem(string name, int count = 1, int level = 1)
    {
        Item itemToGet = items.Find(x => x.itemName == name);
        if (itemToGet != null)
        {
            itemToGet.stackCount = count; // Ini akan menentukan jumlah item yang ada di stack

            itemToGet.Level = level;
            return Instantiate(itemToGet);
        }
        else
        {
            Debug.LogWarning($"Item with name {name} not found in ItemPool!");
            return null;
        }
    }

    public void DropItem(string itemName, Vector2 pos, GameObject itemDrop, int count = 1, int level = 1)
    {
        if (itemDrop == null)
        {
            Debug.LogError($"Item drop prefab untuk {itemName} tidak valid.");
            return;
        }

        GameObject droppedItem = Instantiate(itemDrop, pos, Quaternion.identity);

        // Set tag menjadi ItemDrop agar bisa dideteksi saat player mengambilnya
        droppedItem.tag = "ItemDrop";

        // Jika item memiliki komponen visual (misalnya SpriteRenderer)
        SpriteRenderer spriteRenderer = droppedItem.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Item itemData = ItemPool.Instance.items.Find(item => item.itemName == itemName);
            if (itemData != null)
            {
                spriteRenderer.sprite = itemData.sprite; // Ganti sprite sesuai dengan item
            }
        }

        // Menambahkan Rigidbody2D dan force untuk efek jatuh
        Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = droppedItem.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0.5f;
        rb.AddForce(new Vector2(Random.Range(-0.5f, 0.5f), -1f), ForceMode2D.Impulse);

        // Panggil StopGravity dari komponen ItemDropInteractable
        ItemDropInteractable interactable = droppedItem.GetComponent<ItemDropInteractable>();
        if (interactable != null)
        {
            interactable.StartCoroutine(interactable.StopGravity(rb, 0.8f));
        }

        droppedItem.GetComponent<ItemDropInteractable>().item = GetItem(itemName, count, level);
    }

}
