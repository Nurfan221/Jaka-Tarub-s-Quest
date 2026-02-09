using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Gunakan ini untuk Button standard

public class TutorialManager : MonoBehaviour, ISaveable
{
    public static TutorialManager Instance;

    [Header("Database Reference")]
    public GameTutorialDatabase tutorialDatabase;

    // KAMUS DATA: Untuk pencarian cepat (ID -> Data)
    private Dictionary<string, TutorialData> tutorialMap;

    // KAMUS STATUS: Untuk menyimpan save data (ID -> Sudah Selesai?)
    private Dictionary<string, bool> completionStatus = new Dictionary<string, bool>();
    [Header("Data")]
    public Dialogues tutorialDialogue;

    [Header("UI References")]
    public Transform tutorialUI;
    public TextMeshProUGUI judulText;

    public Transform kontenContainer;

    public GameObject textTemplate;

    public Button closeButton;
    private void Awake()
    {
        if (Instance == null) Instance = this;

        // Inisialisasi Map saat game mulai
        if (tutorialDatabase != null)
        {
            tutorialMap = tutorialDatabase.GetTutorialDictionary();
        }
    }
    void Start()
    {
        if (closeButton != null)
        {
            // Hapus listener lama jika ada (good practice)
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseTutorialUI);

        }

        if (textTemplate != null) textTemplate.SetActive(false);

    }
    public object CaptureState()
    {
        Debug.Log("[SAVE] Menyimpan data tutorial...");

        // Panggil fungsi helper yang sudah kita diskusikan sebelumnya
        // Fungsi ini mengubah Dictionary<string, bool> menjadi List<string>
        return new List<string>(completionStatus.Keys);
    }



    public void RestoreState(object state)
    {
        // Cek dulu apakah state valid?
        if (state == null)
        {
            Debug.Log("[LOAD] Save data kosong, tutorial di-reset.");
            return;
        }

        Debug.Log("[LOAD] Merestorasi data tutorial...");

        // Casting aman menggunakan 'as' (jika gagal akan jadi null, tidak crash)
        List<string> loadedIDs = state as List<string>;

        // Jika casting berhasil, baru load
        if (loadedIDs != null)
        {
            LoadFinishedTutorials(loadedIDs);
        }
        else
        {
            // Hati-hati: Terkadang JsonUtility membungkus list menjadi JArray/Object lain
            // Jika save system Anda menggunakan Newtonsoft.Json, baris di atas sudah aman.
            Debug.LogWarning("[LOAD] Format data tidak sesuai (Bukan List<string>)");
        }
    }
    public void OpenTutorialUI(Dialogues tutorialData)
    {
        tutorialUI.gameObject.SetActive(true);
        GameController.Instance.ShowPersistentUI(false);
        GameController.Instance.PauseGame();
        tutorialDialogue = tutorialData;
        ShowTutorial();
    }

    public void CloseTutorialUI()
    {
        tutorialUI.gameObject.SetActive(false);
        tutorialDialogue = null;
        GameController.Instance.ShowPersistentUI(true);
        GameController.Instance.ResumeGame();
    }

    public void ShowTutorial()
    {
        //gameObject.SetActive(true);

        if (judulText != null)
        {
            judulText.text = tutorialDialogue.name;
        }

       
        foreach (Transform child in kontenContainer)
        {
            // Jangan hapus template jika template itu ditaruh di dalam container
            if (child.gameObject != textTemplate)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (var dataDialogue in tutorialDialogue.TheDialogues)
        {
           
            GameObject newItem = Instantiate(textTemplate, kontenContainer);

            newItem.SetActive(true);

            TextMeshProUGUI textComponent = newItem.GetComponent<TextMeshProUGUI>();

            if (textComponent == null) textComponent = newItem.GetComponentInChildren<TextMeshProUGUI>();

            if (textComponent != null)
            {
                textComponent.text = $" - {dataDialogue.sentence}";
            }
        }
    }

    // Fungsi untuk memanggil Tutorial berdasarkan ID
    public void TriggerTutorial(string tutorialID)
    {
        // 1. Cek apakah tutorial ini ada di database?
        if (!tutorialMap.ContainsKey(tutorialID))
        {
            Debug.LogWarning($"Tutorial ID '{tutorialID}' tidak ditemukan di Database!");
            return;
        }

        // 2. Cek apakah tutorial ini SUDAH PERNAH selesai?
        if (IsTutorialFinished(tutorialID))
        {
            Debug.Log($"Tutorial '{tutorialID}' sudah selesai, skip.");
            return;
        }

        // 3. Jika belum, Jalankan Dialog
        TutorialData data = tutorialMap[tutorialID];
        Debug.Log($"Memulai Tutorial: {tutorialID}");

        // Panggil sistem dialog Anda di sini
        //DialogueSystem.Instance.HandlePlayDialogue(data.dialogueContent);
        OpenTutorialUI(data.dialogueContent);

        // 4. Tandai Selesai (Opsional: bisa juga ditandai setelah dialog tutup)
        CompleteTutorial(tutorialID);
    }

    public void CompleteTutorial(string tutorialID)
    {
        if (!completionStatus.ContainsKey(tutorialID))
        {
            completionStatus.Add(tutorialID, true);
            Debug.Log($"[SYSTEM] Tutorial '{tutorialID}' dicatat selesai.");
        }
    }

    public bool IsTutorialFinished(string tutorialID)
    {
        if (completionStatus.ContainsKey(tutorialID))
        {
            return completionStatus[tutorialID];
        }
        return false; // Kalau tidak ada di list status, berarti belum selesai
    }

    public List<string> GetFinishedTutorials()
    {
        List<string> finishedList = new List<string>();

        foreach (var pair in completionStatus)
        {
            // Jika statusnya true (selesai), masukkan ID-nya ke list
            if (pair.Value == true)
            {
                finishedList.Add(pair.Key);
            }
        }
        return finishedList;
    }

    // 2. Dipanggil saat Load Game
    // Mengubah List string dari SaveData kembali menjadi Dictionary
    public void LoadFinishedTutorials(List<string> savedIDs)
    {
        // Bersihkan dulu status lama (penting!)
        completionStatus.Clear();

        if (savedIDs != null)
        {
            foreach (string id in savedIDs)
            {
                // Masukkan kembali ke kamus dengan status True
                if (!completionStatus.ContainsKey(id))
                {
                    completionStatus.Add(id, true);
                }
            }
        }
        Debug.Log($"Tutorial Loaded: {completionStatus.Count} tutorial selesai.");
    }
}