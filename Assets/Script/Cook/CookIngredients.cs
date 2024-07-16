using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookIngredients : MonoBehaviour
{
    [SerializeField] CookUI cookUI; // Referensi ke skrip CookUI
    [SerializeField] Transform parentTransform; // Parent untuk menempatkan hasil resep
    [SerializeField] GameObject hasilCook; // Referensi ke tampilan hasil masakan
    [SerializeField] DropCookSlot slotCook1; // Referensi ke DropCookSlot untuk SlotCook1
    [SerializeField] DropCookSlot slotCook2; // Referensi ke DropCookSlot untuk SlotCook2
    [SerializeField] DropCookSlot slotCook3; // Referensi ke DropCookSlot untuk SlotCook3

    [SerializeField] GameObject errorPopup; // Referensi ke tampilan pesan error

    // Background
    [SerializeField] Color backgroundColor = new Color(0.85f, 0.85f, 0.85f); // Warna latar belakang
    [SerializeField] Sprite backgroundImage; // Tetapkan ini di inspector

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
            // wrapperImage.sprite = backgroundImage; // Tidak perlu mengatur background image

            GameObject resultItem = new GameObject(recipe.result.itemName);
            resultItem.transform.SetParent(wrapper.transform, false);

            Image imageComponent = resultItem.AddComponent<Image>();
            imageComponent.sprite = recipe.result.sprite;

            Button buttonComponent = resultItem.AddComponent<Button>();
            buttonComponent.onClick.AddListener(() =>
            {
                // button cek ingredient
                if (slotCook1.item != null || slotCook2.item != null || slotCook3.item != null)
                {
                   
                    ShowErrorPopupForSeconds(2.0f);
                }
                else
                {
                    SoundManager.Instance.PlaySound("Click");
                    DisplayRecipeInHasilCook(recipe, 0.8f);
                }
            });

            RectTransform rectTransform = resultItem.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(240, 240);
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one;
        }
    }


    public void DisplayRecipeInHasilCook(CookUI.CookRecipe recipe, float opacity)
    {
        if (hasilCook == null)
        {
            Debug.LogError("hasilCook tidak diinisialisasi!");
            return;
        }
        else
        {
            DestroyRecipeInHasilCook();
        }

        GameObject resultItem = new GameObject(recipe.result.itemName);
        resultItem.transform.SetParent(hasilCook.transform, false);

        Image imageComponent = resultItem.AddComponent<Image>();
        imageComponent.sprite = recipe.result.sprite;

        Button button = resultItem.AddComponent<Button>();
        button.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySound("Click");
            DestroyRecipeInHasilCook();
        });

        Color imageColor = imageComponent.color;
        imageColor.a = opacity;
        imageComponent.color = imageColor;

        RectTransform rectTransform = resultItem.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(240, 240);
        rectTransform.localPosition = Vector3.zero;
        rectTransform.localScale = Vector3.one;

        CekIngredient1(recipe, opacity);
        CekIngredient2(recipe, opacity);
        CekIngredient3(recipe, opacity);
    }

    public void DestroyRecipeInHasilCook()
    {
        if (slotCook1.item != null || slotCook2.item != null || slotCook3.item != null)
        {
            Debug.Log("Ada isi nya bro ga boleh di hapus ");
        }
        else
        {
            foreach (Transform child in hasilCook.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in slotCook1.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in slotCook2.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in slotCook3.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

   void CekIngredient1(CookUI.CookRecipe recipe, float opacity)
{
    if (recipe.ingredients.Count > 0 && recipe.ingredients[0] != null)
    {
        GameObject resultItemIngredients1 = new GameObject(recipe.ingredients[0].itemName);
        resultItemIngredients1.transform.SetParent(slotCook1.transform, false);

        Image imageComponentIngredients1 = resultItemIngredients1.AddComponent<Image>();
        imageComponentIngredients1.sprite = recipe.ingredients[0].sprite;

        Color imageColorIngredients1 = imageComponentIngredients1.color;
        imageColorIngredients1.a = opacity;
        imageComponentIngredients1.color = imageColorIngredients1;

        RectTransform rectTransformIngredients1 = resultItemIngredients1.GetComponent<RectTransform>();
        rectTransformIngredients1.sizeDelta = new Vector2(240, 240);
        rectTransformIngredients1.localPosition = Vector3.zero;
        rectTransformIngredients1.localScale = Vector3.one;
    }
    else
    {
        Debug.Log("Resep ke-1 Kosong");
    }
}

void CekIngredient2(CookUI.CookRecipe recipe, float opacity)
{
    if (recipe.ingredients.Count > 1 && recipe.ingredients[1] != null)
    {
        GameObject resultItemIngredients2 = new GameObject(recipe.ingredients[1].itemName);
        resultItemIngredients2.transform.SetParent(slotCook2.transform, false);

        Image imageComponentIngredients2 = resultItemIngredients2.AddComponent<Image>();
        imageComponentIngredients2.sprite = recipe.ingredients[1].sprite;

        Color imageColorIngredients2 = imageComponentIngredients2.color;
        imageColorIngredients2.a = opacity;
        imageComponentIngredients2.color = imageColorIngredients2;

        RectTransform rectTransformIngredients2 = resultItemIngredients2.GetComponent<RectTransform>();
        rectTransformIngredients2.sizeDelta = new Vector2(240, 240);
        rectTransformIngredients2.localPosition = Vector3.zero;
        rectTransformIngredients2.localScale = Vector3.one;
    }
    else
    {
        Debug.Log("Resep ke-2 kosong");
    }
}

void CekIngredient3(CookUI.CookRecipe recipe, float opacity)
{
    if (recipe.ingredients.Count > 2 && recipe.ingredients[2] != null)
    {
        GameObject resultItemIngredients3 = new GameObject(recipe.ingredients[2].itemName);
        resultItemIngredients3.transform.SetParent(slotCook3.transform, false);

        Image imageComponentIngredients3 = resultItemIngredients3.AddComponent<Image>();
        imageComponentIngredients3.sprite = recipe.ingredients[2].sprite;

        Color imageColorIngredients3 = imageComponentIngredients3.color;
        imageColorIngredients3.a = opacity;
        imageComponentIngredients3.color = imageColorIngredients3;

        RectTransform rectTransformIngredients3 = resultItemIngredients3.GetComponent<RectTransform>();
        rectTransformIngredients3.sizeDelta = new Vector2(240, 240);
        rectTransformIngredients3.localPosition = Vector3.zero;
        rectTransformIngredients3.localScale = Vector3.one;
    }
    else
    {
        Debug.Log("Resep ke-3 kosong");
    }
}


    void ShowErrorPopupForSeconds(float seconds)
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
}
