using UnityEngine;

public class ItemBehavior : MonoBehaviour
{
    public Item item; // Referensi ke ScriptableObject Item

    public float currentHealth;

    void Start()
    {
        // Saat prefab di-spawn, salin health dari data item
        if (item != null)
        {
            currentHealth = item.health;
            Sprite sprite = item.sprite;
        }
    }


}

