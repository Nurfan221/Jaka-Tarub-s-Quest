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

            ItemPool.Instance.AddItem(sampahItem);
            isFull = false;
            TongKosong();
        }
    }


}
