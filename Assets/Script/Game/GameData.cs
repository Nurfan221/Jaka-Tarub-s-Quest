using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class GameData
{
    public string playerName;

    public int LatestMap;
    public float[] playerPos = new float[3];


    #region STORY_STUFF
    public string currentQuest;

    public bool gameEvent_DoneFirstNarration;
    public bool gameEvent_DoneDialogue_1;
    public bool gameEvent_DoneDialogue_2;
    public bool gameEvent_DoneDialogue_3;
    public bool gameEvent_DoneDialogue_4;
    public bool gameEvent_DoneDialogue_5;
    public bool gameEvent_DoneDialogue_6;
    public bool gameEvent_DoneDialogue_7;

    public bool pedangKakRen;

    public bool quanta_pedang;
    public bool quanta_tongkat;
    public bool quanta_perisai;
    public bool quanta_armor;
    public bool quanta_buku;
    #endregion

    #region ITEM_STUFF

    [System.Serializable]
    public class SimpleItem
    {
        public string itemName;
        public int stackCount;
        public int level;

        public SimpleItem(string itemName, int stackCount, int level)
        {
            this.itemName = itemName;
            this.stackCount = stackCount;
            this.level = level;
        }
    }
    public List<SimpleItem> PlayerInventory_ItemNameAndCount = new();
    public SimpleItem[] PlayerInventory_ActiveItemAndCount = new SimpleItem[4];

    public Dictionary<int, List<SimpleItem>> Storages_ItemNameAndCount = new();

    #endregion

    public GameData(bool empty = false)
    {
        if (!empty)
        {
            Player_Inventory inventory = Player_Inventory.Instance;

            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            playerPos[0] = player.position.x;
            playerPos[1] = player.position.y;


            playerName = GameController.Instance.playerName;
            LatestMap = SceneManager.GetActiveScene().buildIndex;

            int index;
            // Save Player Inventory Items
            foreach (Item item in inventory.itemList)
            {
                PlayerInventory_ItemNameAndCount.Add(new SimpleItem(item.itemName, item.stackCount, item.Level));
            }

            // Save Player Active Items
            PlayerInventory_ActiveItemAndCount[0] = (new(inventory.equippedCombat[0].itemName, 1, inventory.equippedCombat[0].Level));
            PlayerInventory_ActiveItemAndCount[1] = (new(inventory.equippedCombat[1].itemName, inventory.equippedCombat[1].stackCount, inventory.equippedCombat[1].Level));
            PlayerInventory_ActiveItemAndCount[2] = (new(inventory.quickSlots[0].itemName, inventory.quickSlots[0].stackCount, 1));
            PlayerInventory_ActiveItemAndCount[3] = (new(inventory.quickSlots[1].itemName, inventory.quickSlots[1].stackCount, 1));

            // Save Storages 
            index = 0;
            if (StorageSystem.Instance != null)
            {
                foreach (StorageInteractable storage in StorageSystem.Instance.GetStorages())
                {
                    List<SimpleItem> items = new();
                    foreach (Item item in storage.Items)
                    {
                        items.Add(new SimpleItem(item.itemName, item.stackCount, item.Level));
                    }
                    Storages_ItemNameAndCount.Add(index, items);
                    index++;
                }
            }

            currentQuest = Player_Quest.Instance.GetQuest();

            // GAME EVENTS
            gameEvent_DoneFirstNarration = GameEventSystem.Instance.DoneFirstNarration;
            gameEvent_DoneDialogue_1 = GameEventSystem.Instance.DoneDialogue_TamashiiGiveName;
            gameEvent_DoneDialogue_2 = GameEventSystem.Instance.DoneDialogue_DanauPertamaKeDesa;
            gameEvent_DoneDialogue_3 = GameEventSystem.Instance.DoneDialogue_FirstDesaWarga;
            gameEvent_DoneDialogue_4 = GameEventSystem.Instance.DoneDialogue_FirstKakRen;
            gameEvent_DoneDialogue_5 = GameEventSystem.Instance.DoneDialogue_FirstBandit;
            gameEvent_DoneDialogue_6 = GameEventSystem.Instance.DoneDialogue_FirstBanditDone;
            gameEvent_DoneDialogue_7 = GameEventSystem.Instance.DoneDialogue_FinshDialogue;

            if (VillageController.Instance != null)
            {
                pedangKakRen = VillageController.Instance.PedangKakRen == null;
            }

            if (ForestController.Instance != null)
            {
                var theForest = ForestController.Instance;

                quanta_pedang = theForest.QUEST_GagangPedang == null;
                quanta_tongkat = theForest.QUEST_Tongkat == null;
                quanta_perisai = theForest.QUEST_Perisai == null;
                quanta_armor = theForest.QUEST_Armor == null;
                quanta_buku = theForest.QUEST_Buku == null;
            }

        }
    }

    public void ResetGameData()
    {
        Debug.Log("Resetting Game Data");
        PlayerPrefs.SetInt("HaveSaved", 0);

        Player_Inventory inventory = Player_Inventory.Instance;

        playerPos[0] = 0;
        playerPos[1] = 0;

        playerName = "Charibert";
        LatestMap = 1;

        // Save Player Inventory Items
        PlayerInventory_ItemNameAndCount = new();


        // Save Player Active Items
        PlayerInventory_ActiveItemAndCount[0] = (new("Empty", 0, 0));
        PlayerInventory_ActiveItemAndCount[1] = (new("Empty", 0, 0));
        PlayerInventory_ActiveItemAndCount[2] = (new("Empty", 0, 0));
        PlayerInventory_ActiveItemAndCount[3] = (new("Empty", 0, 0));

        // Save Storages 
        Storages_ItemNameAndCount = new();

        // GAME EVENTS
        gameEvent_DoneFirstNarration = false;
        gameEvent_DoneDialogue_1 = false;
        gameEvent_DoneDialogue_2 = false;
        gameEvent_DoneDialogue_3 = false;
        gameEvent_DoneDialogue_4 = false;
        gameEvent_DoneDialogue_5 = false;
        gameEvent_DoneDialogue_6 = false;
        gameEvent_DoneDialogue_7 = false;

    }
}
