using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MainQuest1_Controller;

public abstract class MainQuestController : MonoBehaviour
{
    [Header("Data Umum Quest")]
    public string questName = "Nama Main Quest";
    public List<ItemData> itemRequirements; // List item yang dibutuhkan quest

    public int goldReward;
    public List<ItemData> itemRewards;
    [TextArea(3, 5)]
    public string questDescription = "Deskripsi singkat quest.";

    // Anda bisa mendefinisikan hadiah di sini jika semua main quest punya struktur hadiah yang sama.
    // public Reward[] rewards;

    // 'protected' berarti variabel ini hanya bisa diakses oleh kelas ini dan kelas turunannya.
    protected QuestManager questManager;
    protected MainQuestSO questData;
    protected bool isQuestComplete = false;

    protected PlayerMainQuestStatus playerQuestStatus;
    public static event Action<bool> OnItemGivenToNPC;

    public virtual void StartQuest(QuestManager manager, MainQuestSO so, PlayerMainQuestStatus status)
    {
        this.questManager = manager;
        this.questData = so; // Simpan referensi ke naskahnya
        this.isQuestComplete = false;
        // Ambil nama dari SO agar konsisten
        this.playerQuestStatus = status;
        this.questName = so.questName;
        this.itemRequirements = so.itemRequirements;
        this.goldReward = so.goldReward;
        this.itemRewards = so.itemRewards;

        Debug.Log($"Memulai Main Quest: {questName}");
    }

  
    public abstract void UpdateQuest();

  
    public abstract void SetInitialState(System.Enum state);

  
    public bool IsComplete()
    {
        return isQuestComplete;
    }

    public abstract string GetCurrentObjectiveInfo();

    public ItemData GetRequiredItem(string itemName)
    {
        // PENTING: Akses itemRequirements melalui questData
        if (questData == null || questData.itemRequirements == null || questData.itemRequirements.Count == 0)
        {
            // Debug.LogWarning($"Quest '{questName}' tidak memiliki itemRequirements atau questData null.");
            return null; // Quest ini tidak membutuhkan item apapun atau data quest belum diset
        }

        // Mencari item di dalam daftar itemRequirements quest ini
        return questData.itemRequirements.FirstOrDefault(req => req.itemName.Equals(itemName, System.StringComparison.OrdinalIgnoreCase));
    }



    public bool AreAllItemRequirementsMet() // Tidak perlu parameter currentItemProgress lagi
    {
        if (questData == null || questData.itemRequirements == null || questData.itemRequirements.Count == 0)
        {
            return true;
        }

        // Pastikan playerQuestStatus dan itemProgress-nya tidak null
        if (playerQuestStatus == null || playerQuestStatus.itemProgress == null)
        {
            Debug.LogError($"playerQuestStatus atau itemProgress-nya null untuk '{questName}'.");
            return false;
        }

        foreach (var requiredItem in questData.itemRequirements)
        {
            if (!playerQuestStatus.itemProgress.ContainsKey(requiredItem.itemName))
            {
                return false;
            }
            if (playerQuestStatus.itemProgress[requiredItem.itemName] < requiredItem.count)
            {
                return false;
            }
        }
        return true;
    }

    public int GetNeededItemCount(string itemName) // Tidak perlu parameter currentItemProgress lagi
    {
        ItemData requiredItem = GetRequiredItem(itemName);
        if (requiredItem == null)
        {
            return -1;
        }

        if (playerQuestStatus == null || playerQuestStatus.itemProgress == null)
        {
            Debug.LogError($"playerQuestStatus atau itemProgress-nya null untuk '{questName}'.");
            return requiredItem.count;
        }

        int currentProgress = 0;
        if (playerQuestStatus.itemProgress.ContainsKey(itemName))
        {
            currentProgress = playerQuestStatus.itemProgress[itemName];
        }

        return requiredItem.count - currentProgress;
    }

    public bool TryProcessGivenItem(ItemData givenItemData)
    {
        // Pastikan kita punya status quest yang valid
        if (playerQuestStatus == null || playerQuestStatus.Progress != QuestProgress.Accepted)
        {
            Debug.LogWarning("Tidak ada Main Quest aktif atau sudah selesai untuk memproses item.");
            return false;
        }

        Debug.Log($"Main Quest Controller: Memproses item '{givenItemData.itemName}'...");

        ItemData requiredItem = GetRequiredItem(givenItemData.itemName); // Menggunakan fungsi dari base class
        if (requiredItem == null) return false;

        int neededAmount = GetNeededItemCount(givenItemData.itemName); // Menggunakan fungsi dari base class
        if (neededAmount <= 0) return false;

        int amountToProcess = Mathf.Min(givenItemData.count, neededAmount);

        if (amountToProcess > 0)
        {
            // Update progres item di PlayerMainQuestStatus yang disimpan di base class
            playerQuestStatus.itemProgress[givenItemData.itemName] += amountToProcess;

            Debug.Log($"Progres '{givenItemData.itemName}' untuk '{questName}' diupdate: {playerQuestStatus.itemProgress[givenItemData.itemName]}/{requiredItem.count}");

            if (AreAllItemRequirementsMet()) // Menggunakan fungsi dari base class tanpa parameter
            {
                Debug.Log($"SEMUA ITEM DIBUTUHKAN UNTUK MAIN QUEST '{questName}' TELAH TERPENUHI!");
                OnItemGivenToNPC?.Invoke(true); // Panggil event jika semua item terpenuhi
            }
            return true;
        }
        return false;
        OnItemGivenToNPC?.Invoke(false); // Panggil event jika item tidak relevan
    }
}