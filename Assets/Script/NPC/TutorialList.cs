using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialList : MonoBehaviour
{


 

    [Header("UI STUFF")]
    public Transform ContentList;
    public Transform SlotTemplateList;
    public TMP_Text judulTutorial;
    public GameObject listTutorialTemplate;
    public Transform ContentTutorialList;

   

    void Start()
    {
        //RefreshNPCList(); // Tambahkan pemanggilan fungsi di Start jika ingin langsung memuat daftar NPC
    }

    public void RefreshTutorialList()
    {
        Debug.Log("Memanggil fungsi RefreshNPCList");

        ClearChildrenExceptTemplate(ContentList, SlotTemplateList);

        foreach (var tutorial in DatabaseManager.Instance.gameTutorialDatabase.allTutorials)
        {
            Transform tutorialList = Instantiate(SlotTemplateList, ContentList);
            tutorialList.gameObject.SetActive(true);
            tutorialList.name = tutorial.tutorialID;

            // Perbaikan: Mengubah teks pada hasil instansiasi, bukan template aslinya
            tutorialList.GetChild(0).GetComponent<TMP_Text>().text = tutorial.tutorialID;

            Button btnDeskripsi = tutorialList.GetComponent<Button>();

            btnDeskripsi.onClick.RemoveAllListeners();
            btnDeskripsi.onClick.AddListener(() =>
            {
                TutorialDeskripsi(tutorial);
            });
        }
    }

    private void ClearChildrenExceptTemplate(Transform parent, Transform template)
    {
        foreach (Transform child in parent)
        {
            if (child != template)
                Destroy(child.gameObject);
        }
    }

    private void TutorialDeskripsi(TutorialData tutorialData)
    {
        Debug.Log("memanggil fungsi npc deskripsi");
        if (judulTutorial != null)
        {
            judulTutorial.text = tutorialData.tutorialID;
        }


        foreach (Transform child in ContentTutorialList)
        {
            // Jangan hapus template jika template itu ditaruh di dalam container
            if (child.gameObject != listTutorialTemplate)
            {
                Destroy(child.gameObject);
            }
        }

        for (int i = 0; i < tutorialData.dialogueContent.TheDialogues.Count; i++)
        {
            var dataDialogue = tutorialData.dialogueContent.TheDialogues[i];

            GameObject newItem = Instantiate(listTutorialTemplate, ContentTutorialList);
            newItem.SetActive(true);

            TextMeshProUGUI textComponent = newItem.GetComponent<TextMeshProUGUI>();
            if (textComponent == null) textComponent = newItem.GetComponentInChildren<TextMeshProUGUI>();

            if (textComponent != null)
            {
                Debug.Log("Text component found on the instantiated item.");

                textComponent.text = $"{i + 1}. <indent=1.5em>{dataDialogue.sentence}</indent>";
            }else
            {
                Debug.Log("Text component not found on the instantiated item.");
            }
        }


    }
}
