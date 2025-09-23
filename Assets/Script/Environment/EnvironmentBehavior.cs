using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;


public class EnvironmentBehavior : UniqueIdentifiableObject
{ 
    //  Implementasi dari Kontrak IUniqueIdentifiable 
    public EnvironmentHardnessLevel hardnessLevel;
    public TypeObject typeObject;
    public TypePlant typePlant;

    //Animation idle 
    public Sprite[] rumputAnimation;
    public float frameRate = 0.3f; // Waktu per frame (kecepatan animasi)

    private SpriteRenderer spriteRenderer; // Komponen SpriteRenderer
    private int currentFrame = 0; // Indeks frame saat ini
    public string nameEnvironment;
    public Item itemDrop;
    public EnvironmentType environmentType;

    #region Unique ID Implementation

    public override string GetObjectType()
    {
        // Berikan kategori umum untuk objek ini.
        return typeObject.ToString();
    }

    public override EnvironmentHardnessLevel GetHardness()
    {
        // Ambil nilai dari variabel yang bisa diatur di Inspector.
        return hardnessLevel;
    }

    public override string GetBaseName()
    {
        // Ambil nama dasar dari variabel yang bisa diatur di Inspector.
        return typePlant.ToString();
    }

    public override string GetVariantName()
    {
        return environmentType.ToString();
    }

    #endregion
    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // Ambil komponen SpriteRenderer
        StartCoroutine(PlayrumputAnimation()); // Mulai animasi
    }

    private IEnumerator PlayrumputAnimation()
    {
        while (true) // Loop tanpa batas (animasi berulang)
        {
            if (rumputAnimation.Length > 0) // Pastikan array sprite tidak kosong
            {
                spriteRenderer.sprite = rumputAnimation[currentFrame]; // Setel sprite saat ini
                currentFrame = (currentFrame + 1) % rumputAnimation.Length; // Pindah ke frame berikutnya (loop)
            }
            yield return new WaitForSeconds(frameRate); // Tunggu sebelum beralih ke frame berikutnya
        }
    }

    public void GetItemDrop()
    {

        ItemPool.Instance.DropItem(itemDrop.itemName, itemDrop.health, itemDrop.quality, transform.position + new Vector3(0, 0.5f, 0));
        Destroy(gameObject);
    }
}

