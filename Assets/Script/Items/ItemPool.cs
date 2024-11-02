using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPool : MonoBehaviour
{
    public static ItemPool Instance;

    [SerializeField] GameObject itemDropPrefab;

    [SerializeField] List<Item> items;

    private void Awake()
    {
        Instance = this;
    }

    public Item GetItem(string name, int count = 1, int level = 1)
    {
        Item itemToGet = items.Find(x => x.itemName == name);
        if (itemToGet != null)
        {
            itemToGet.stackCount = count;
            itemToGet.Level = level;
            return Instantiate(itemToGet);
        }
        else
        {
            Debug.LogWarning($"Item with name {name} not found in ItemPool!");
            return null;
        }
    }

    public void DropItem(string itemName, Vector2 pos, GameObject itemDrop , int count = 1, int level = 1)
    {
        GameObject droppedItem = Instantiate(itemDrop, pos, Quaternion.identity);

        // Tambahkan komponen Rigidbody2D jika belum ada
        Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = droppedItem.AddComponent<Rigidbody2D>();
        }

        // Atur gravityScale kecil untuk efek jatuh ringan
        rb.gravityScale = 0.5f;

        // Tambahkan sedikit force untuk gerakan jatuh
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
