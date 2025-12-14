using UnityEngine;

public class TongSampahInteractable : Interactable
{
    public Sprite spriteFull;
    public Sprite spriteKosong;
    public bool isFull;
    public ItemData sampahItem;
    public SpriteRenderer spriteRenderer;
    [SerializeField] GlowEffect glowEffect;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isFull = false;
        //spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        Transform visualChild = transform.Find("Visual");

        if (visualChild != null)
        {
            // Ambil komponen dari anak tersebut
            spriteRenderer = visualChild.GetComponent<SpriteRenderer>();
        }
        else
        {
            Debug.LogError("Gawat! Tidak ada anak bernama 'Visual' di objek ini!");
        }
        spriteRenderer.sprite = spriteKosong;

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TongFull(ItemData sampah)
    {
        isFull = true;
        spriteRenderer.sprite = spriteFull;
        glowEffect.StartGlowEffect();
        sampahItem = sampah;
    }

    public void TongKosong()
    {
        isFull = false;
        spriteRenderer.sprite = spriteKosong;
        sampahItem = null;
        glowEffect.StopGlowEffect();
    }


    protected override void Interact()
    {
        if (isFull)
        {
            // Di dalam CookUI / Result Button Listener
            bool isSuccess = ItemPool.Instance.AddItem(sampahItem);

            if (isSuccess)
            {
                // Hapus item dari tungku
                isFull = false;
                TongKosong();
            }
            else
            {
                // Jangan hapus, biarkan di tungku
                Debug.Log("Tas penuh, item tetap di tungku.");
            }

        }
    }


}
