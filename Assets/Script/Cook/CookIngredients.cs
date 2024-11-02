using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookIngredients : MonoBehaviour
{
    [SerializeField] CookUI cookUI; // Referensi ke skrip CookUI
    [SerializeField] Transform parentTransform; // Parent untuk menempatkan hasil resep
    [SerializeField] GameObject hasilCook; // Referensi ke tampilan hasil masakan
    [SerializeField] GameObject itemCook; // Referensi ke UI untuk menampilkan bahan-bahan masakan
    [SerializeField] GameObject errorPopup; // Referensi ke tampilan pesan error

    // Background
    [SerializeField] Color backgroundColor = new Color(0.85f, 0.85f, 0.85f); // Warna latar belakang
    [SerializeField] Sprite backgroundImage; // Tetapkan ini di inspector

    public bool cekIngredient = false;

    void Start()
    {
        CekIngredients();
    }

    public void CekIngredients()
    {
        Debug.Log("Fungsi CekIngredients dijalankan");

        if (cookUI == null || parentTransform == null)
        {
            Debug.LogError("CookUI atau parentTransform tidak ditetapkan!");
            return;
        }

        foreach (var recipe in cookUI.recipes)
        {
            GameObject wrapper = new GameObject("Wrapper_" + recipe.result.itemName);
            wrapper.transform.SetParent(parentTransform, false);

            RectTransform wrapperRectTransform = wrapper.AddComponent<RectTransform>();
            wrapperRectTransform.sizeDelta = new Vector2(120, 120);

            Image wrapperImage = wrapper.AddComponent<Image>();
            Color transparentColor = backgroundColor;
            transparentColor.a = 0f; // Mengatur opasitas menjadi 0
            wrapperImage.color = transparentColor;

            GameObject resultItem = new GameObject(recipe.result.itemName);
            resultItem.transform.SetParent(wrapper.transform, false);

            Image imageComponent = resultItem.AddComponent<Image>();
            imageComponent.sprite = recipe.result.sprite;

            Button buttonComponent = resultItem.AddComponent<Button>();
            buttonComponent.onClick.AddListener(() =>
            {
                if (cookUI == null)
                {
                    Debug.LogError("cookUI tidak diinisialisasi!");
                    return;
                }

                if (cookUI.itemCookValue == true || cookUI.resultCookValue == true)
                {
                    ShowErrorPopupForSeconds(2.0f);
                }
                else
                {
                    // // SoundManager.Instance.PlaySound("Click");
                    // DisplayRecipeInHasilCook(recipe);

                    DisplayIngredients(recipe);
                }
            });

            RectTransform rectTransform = resultItem.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(240, 240);
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one;
        }
    }

    // Fungsi baru untuk menampilkan ingredients ke dalam itemCook
    public void DisplayIngredients(CookUI.CookRecipe recipe)
    {
        cekIngredient = true; // Menandakan bahwa ingredient dicek

        if (itemCook == null)
        {
            Debug.LogError("itemCook tidak diinisialisasi!");
            return;
        }

        // Menghapus semua child dari itemCook sebelum menambahkan ingredient baru
        foreach (Transform child in itemCook.transform)
        {
            Destroy(child.gameObject);
        }

        // Menambahkan semua ingredient dari recipe ke UI
        for (int i = 0; i < recipe.ingredients.Count; i++)
        {
            GameObject ingredientItem = new GameObject(recipe.ingredients[i].itemName); // Membuat GameObject untuk setiap ingredient
            ingredientItem.transform.SetParent(itemCook.transform, false); // Menetapkan parent-nya ke itemCook

            Image imageComponent = ingredientItem.AddComponent<Image>(); // Menambahkan komponen Image
            imageComponent.sprite = recipe.ingredients[i].sprite; // Menetapkan sprite ingredient

            Text ingredientCountText = ingredientItem.AddComponent<Text>(); // Menambahkan komponen Text
            // ingredientCountText.text = "x" + recipe.ingredientsCount[i]; // (jika ada logika untuk menampilkan jumlah ingredient)

            RectTransform rectTransform = ingredientItem.GetComponent<RectTransform>(); // Menyesuaikan ukuran dan posisi
            rectTransform.sizeDelta = new Vector2(240, 240);
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one;


        // Menambahkan komponen Button
        Button buttonComponent = ingredientItem.AddComponent<Button>();
        // Menetapkan listener untuk menghapus item ketika diklik
        buttonComponent.onClick.AddListener(() => DestroyCekIngredient());
        }


        // Menampilkan hasil masakan
        DisplayRecipeInHasilCook(recipe);
    }


    public void DisplayRecipeInHasilCook(CookUI.CookRecipe recipe)
    {
        if (hasilCook == null)
        {
            Debug.LogError("hasilCook tidak diinisialisasi!");
            return;
        }

        GameObject resultItem = new GameObject(recipe.result.itemName);
        resultItem.transform.SetParent(hasilCook.transform, false);

        Image imageComponent = resultItem.AddComponent<Image>();
        imageComponent.sprite = recipe.result.sprite;

        Button button = resultItem.AddComponent<Button>();
        button.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySound("Click");
        });

        Color imageColor = imageComponent.color;
       
        imageComponent.color = imageColor;

        RectTransform rectTransform = resultItem.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(240, 240);
        rectTransform.localPosition = Vector3.zero;
        rectTransform.localScale = Vector3.one;

         // CekIngredient1 ditambahkan
    }

   


   public void ShowErrorPopupForSeconds(float seconds)
    {
        if (errorPopup != null)
        {
            errorPopup.SetActive(true);
            StartCoroutine(HideErrorPopupAfterSeconds(seconds));
        }
        else
        {
            Debug.LogError("Error popup tidak ditetapkan!");
        }
    }

    IEnumerator HideErrorPopupAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (errorPopup != null)
        {
            errorPopup.SetActive(false);
        }
    }


    public void DestroyCekIngredient()
    {
        cekIngredient = false;
           // Hapus item yang sudah ada di tampilan Cook
        foreach (Transform child in cookUI.ItemCook.transform)
        {
            Destroy(child.gameObject);
        }

         // Hapus item yang sudah ada di tampilan Cook
        foreach (Transform child in cookUI.cookingResult.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
