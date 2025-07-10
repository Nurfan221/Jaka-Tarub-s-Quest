using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class PrefabItemBehavior : MonoBehaviour
{
    public float health;
    private string namePrefab;

    public GameObject itemDrop;
    private int minItemDrop = 2;
    private int maxItemDrop = 4;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
        health -= Mathf.Min(damage, health);
        Debug.Log($"Pohon terkena damage. Sisa HP: {health}");

        if (health <= 0)
        {
            DestroyPrefab();
        }
    }

    private void DestroyPrefab()
    {
        Debug.Log("Pohon dihancurkan!");

        // Hitung jumlah acak untuk setiap jenis item
        int woodCount = (int)Random.Range(minItemDrop, maxItemDrop + 1);

       

        // Drop kayu
        for (int i = 0; i < woodCount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
            if (itemDrop != null)
                ItemPool.Instance.DropItem(itemDrop.name, transform.position + offset, itemDrop);
        }



        // Hancurkan pohon setelah menjatuhkan item
        Destroy(gameObject);
    }

}
