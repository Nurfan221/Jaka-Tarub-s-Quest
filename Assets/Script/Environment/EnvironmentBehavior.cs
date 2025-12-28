using UnityEngine;


public class EnvironmentBehavior : UniqueIdentifiableObject
{
    //  Implementasi dari Kontrak IUniqueIdentifiable 
    public EnvironmentHardnessLevel hardnessLevel;
    public TypeObject typeObject;
    public TypePlant typePlant;
    public ArahObject arahObject;
    public EnvironmentType environmentType;
    public bool useAnimation;

    private SpriteRenderer spriteRenderer; // Komponen SpriteRenderer
    public string nameEnvironment;
    public Item itemDrop;

    [Header("Animation Settings")]
    // Masukkan file Override Animator Controller yang spesifik untuk batu ini
    public RuntimeAnimatorController stoneAnimatorController;
    private Animator stoneAnimator;

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
        if (typePlant == TypePlant.None && arahObject != ArahObject.None)
        {
            return arahObject.ToString();
        }
        else
        {
            return typePlant.ToString();
        }

    }

    public override string GetVariantName()
    {
        return environmentType.ToString();

    }

    #endregion
    public void Start()
    {
        if (useAnimation)
        {
            SetupVisualComponent();

        }
    }

    private void SetupVisualComponent()
    {
        Transform visualChild = transform.Find("Visual");
        Debug.Log("memanggil fungsi setup component visual");

        if (visualChild != null)
        {
            Debug.Log("ya component visual ditemukan");
            spriteRenderer = visualChild.GetComponent<SpriteRenderer>();

            // Kita cek dulu biar ga double, kalau belum ada baru tambah
            stoneAnimator = visualChild.GetComponent<Animator>();
            if (stoneAnimator == null)
            {
                stoneAnimator = visualChild.gameObject.AddComponent<Animator>();
            }
            if (useAnimation)
            {
                // Jika butuh animasi, baru kita cek apakah controllernya sudah dipasang
                if (stoneAnimatorController != null)
                {
                    stoneAnimator.runtimeAnimatorController = stoneAnimatorController;
                }
                else
                {
                    // Niatnya mau pakai animasi (useAnimation = true), tapi lupa pasang controller.
                    Debug.LogError($"Gawat! {typeObject} disetting pakai animasi, tapi stoneAnimatorController-nya kosong!");
                }
            }

            // Karena ini lewat code, kita harus set manual agar tidak lag di kota
            stoneAnimator.cullingMode = AnimatorCullingMode.CullCompletely;
        }
        //else
        //{
        //    Debug.LogError("ohhh tidak component visual tidak ditemukan");
        //}
    }


    public void GetItemDrop()
    {

        ItemData newItemData = new ItemData
        {
            itemName = itemDrop.itemName,
            count = 1,
            quality = itemDrop.quality,
            itemHealth = itemDrop.health
        };
        ItemPool.Instance.AddItem(newItemData);

        Destroy(gameObject);
    }

    public void DropItem()
    {
        Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(0.1f, 0.5f));
        ItemPool.Instance.DropItem(itemDrop.itemName, itemDrop.health, itemDrop.quality, transform.position + offset, 1);

        Destroy(gameObject);
    }
}

