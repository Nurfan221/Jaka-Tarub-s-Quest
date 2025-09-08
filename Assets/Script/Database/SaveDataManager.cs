using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance;
    private string saveFilePath;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "game_save.json");
    }

    public void SaveGame()
    {
        GameSaveData saveData = new GameSaveData();
        CaptureAllSaveableStates(saveData);
        SaveToFile(saveData);
    }

    public GameSaveData LoadGame()
    {
        // Cek apakah file save ada
        if (File.Exists(saveFilePath))
        {
            Debug.Log("Memuat data dari: " + saveFilePath);
            string json = File.ReadAllText(saveFilePath);
            GameSaveData gameData = JsonUtility.FromJson<GameSaveData>(json);
            return gameData; // Kembalikan data yang sudah di-load
        }
        else
        {
            Debug.Log("File save tidak ditemukan.");
            return null; // Kembalikan null jika tidak ada file
        }
    }

    private void SaveToFile(GameSaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Game berhasil disimpan ke: " + saveFilePath);
    }

    private GameSaveData LoadFromFile()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.Log("File save tidak ditemukan.");
            return null;
        }

        string json = File.ReadAllText(saveFilePath);
        return JsonUtility.FromJson<GameSaveData>(json);
    }

    // "Manajer Sensus" mengumpulkan data (SUDAH DIPERBAIKI)
    private void CaptureAllSaveableStates(GameSaveData saveData)
    {
        // Cari semua komponen yang mengimplementasikan ISaveable
        foreach (ISaveable saveable in FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>())
        {
            // PERBAIKAN: Ubah 'saveable' (Interface) menjadi MonoBehaviour
            var monoBehaviour = saveable as MonoBehaviour;
            if (monoBehaviour == null) continue;

            var uniqueID = monoBehaviour.GetComponent<UniqueID>();
            if (uniqueID == null)
            {
                Debug.LogWarning($"Objek {monoBehaviour.name} bisa disimpan tapi tidak punya UniqueID!", monoBehaviour.gameObject);
                continue;
            }

            // Buat entri data baru dan tambahkan ke list
            saveData.savedEntities.Add(new SaveableEntityData
            {
                id = uniqueID.ID,
                state = saveable.CaptureState()
            });
        }
    }

    public void RestoreAllSaveableStates(GameSaveData saveData)
    {
        // Buat dictionary dari objek yang bisa di-save untuk pencarian cepat
        var saveableEntities = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>()
            .ToDictionary(e => (e as MonoBehaviour).GetComponent<UniqueID>()?.ID);

        foreach (var savedEntity in saveData.savedEntities)
        {
            // Cari objek di scene yang memiliki ID yang cocok
            if (saveableEntities.TryGetValue(savedEntity.id, out ISaveable saveable))
            {
                // Minta objek tersebut untuk me-restore datanya
                saveable.RestoreState(savedEntity.state);
            }
        }
    }
}