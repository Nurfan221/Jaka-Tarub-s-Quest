using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropInteractable : Interactable
{
    public Item item;
    [SerializeField] public string itemName;
    private Rigidbody2D rb;
    public float gravityDuration = 2f; // Durasi gravitasi setelah item jatuh

    private void Start()
    {

        if (item == null)
            item = ItemPool.Instance.GetItem(itemName);
        //promptMessage = "Inventory Full";
        //GetComponent<SpriteRenderer>().sprite = item.sprite;
    }

    //protected override void Interact()
    //{
    //    if (SoundManager.Instance != null)
    //        SoundManager.Instance.PlaySound("Pick");
    //    Debug.Log(item.itemName + " di ambil.");
    //    Player_Inventory.Instance.AddItem(ItemPool.Instance.GetItem(item.itemName));
    //    //if (item.type == ItemType.Quest) { GetComponent<QuestQuanta>().Take(); }

    //    Destroy(gameObject);
    //}

    public IEnumerator StopGravity(Rigidbody2D rb, float delay)
    {
        Debug.Log("ItemDropInteractable: StopGravity coroutine started.");
        yield return new WaitForSeconds(delay);
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
        Debug.Log("ItemDropInteractable: Gravity set to 0.");
    }
}
