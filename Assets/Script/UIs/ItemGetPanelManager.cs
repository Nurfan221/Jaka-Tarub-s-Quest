using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemGetPanelManager : MonoBehaviour
{
    public static ItemGetPanelManager Instance { get; private set; }

    // Prefab template untuk setiap slot item
    public GameObject itemSlotTemplate;

    // Kontainer di UI tempat slot item akan dibuat
    public Transform contentParent;

    [Header("Component Animation")]
    // Komponen UI yang diperlukan
    //public RectTransform questUI;
    //public Button questButton;
    // Variabel untuk animasi
    public float animationDuration = 0.5f; // Durasi animasi dalam detik
    public float targetPosY = 300f; // Ketinggian akhir UI
    public float startPosY = 0f; // Ketinggian awal UI

    private bool isUIActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // Metode ini akan menampilkan item yang diberikan
    public void ShowItems(ItemData itemToShow)
    {
        ItemData itemData = itemToShow;
        // Pastikan tidak ada item sebelumnya yang tersisa
        GameObject newSlot = Instantiate(itemSlotTemplate, contentParent);

        // Pastikan template memiliki komponen-komponen ini
        //Image itemImage = newSlot.transform.Find("ItemImage").GetComponent<Image>();
        //TMP_Text itemName = newSlot.transform.Find("ItemName").GetComponent<TMP_Text>();

        Item itemTemplate = ItemPool.Instance.GetItemWithQuality(itemToShow.itemName, itemToShow.quality);

        // Atur data item
        Image templateImage = newSlot.transform.Find("Image").GetComponent<Image>();
        Image image = templateImage.transform.Find("ItemImage").GetComponent<Image>();
        image.sprite = itemTemplate.sprite;
        Image templateNameText = newSlot.transform.Find("NameItem").GetComponent<Image>();
        TMP_Text templateName = templateNameText.transform.Find("NameItemGet").GetComponent<TMP_Text>();
        templateName.text = itemToShow.itemName + " x" + itemToShow.count;
        newSlot.SetActive(true);
        ItemGetAnimator slotAnimator = newSlot.GetComponent<ItemGetAnimator>();
        if (slotAnimator != null)
        {
            slotAnimator.PlayItemGetAnimation();
        }
        // Aktifkan panel utama

    }

   

    // Coroutine untuk menyembunyikan UI (kebalikan dari AnimateShowUI)
  
    //private IEnumerator AnimateShowUI(Transform objekTarnsform)
    //{
    //    objekTarnsform.gameObject.SetActive(true);
    //    float timer = 0f;
    //    Vector2 startPos = new Vector2(objekTarnsform.x, startPosY);
    //    Vector2 targetPos = new Vector2(objekTarnsform.x, targetPosY);
    //    float startPosY = 0f;
    //    float targetPosY = -40; // Posisi Y agar terlihat 'menggulung' dari atas

    //    // Ubah posisi jangkar (anchor) ke bagian atas
    //    objekTarnsform.pivot = new Vector2(0.5f, 1f);
    //    objekTarnsform.anchorMin = new Vector2(0.5f, 1f);
    //    objekTarnsform.anchorMax = new Vector2(0.5f, 1f);
    //    objekTarnsform.sizeDelta = startPos;
    //    objekTarnsform.anchoredPosition = new Vector2(objekTarnsform.anchoredPosition.x, startPosY);

    //    // Loop untuk menggerakkan dan mengubah ukuran UI
    //    while (timer < animationDuration)
    //    {
    //        timer += Time.deltaTime;
    //        float progress = timer / animationDuration;

    //        // Perbarui ukuran dan posisi Y
    //        float newHeight = Mathf.Lerp(startPosY, targetPosY, progress);
    //        float newPosY = Mathf.Lerp(startPosY, targetPosY, progress);

    //        objekTarnsform.sizeDelta = new Vector2(objekTarnsform.sizeDelta.x, newHeight);
    //        objekTarnsform.anchoredPosition = new Vector2(objekTarnsform.anchoredPosition.x, newPosY);

    //        yield return null;
    //    }

    //    // Pastikan posisi dan ukuran akhir sudah tepat
    //    objekTarnsform.sizeDelta = targetPos;
    //    objekTarnsform.anchoredPosition = new Vector2(objekTarnsform.anchoredPosition.x, targetPosY);
    //}
}